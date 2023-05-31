using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_AD
using UnityEngine.Advertisements;
#endif
#if ENABLE_ADMOB
using GoogleMobileAds.Api;
#endif
public class AdsController : MonoBehaviour
#if UNITY_AD
, IUnityAdsInitializationListener, IUnityAdsLoadListener, IUnityAdsShowListener
#endif
{
#pragma warning disable 0414
    const int InterstitialAttempts = 5;
    const int RewardedAttempts = 5;
    const int BannerAttempts = 5;

    //UNITY ADS
    string unityAdsId;
    string unityAdsInterstitialId;
    string unityAdsRewardedId;
    string unityAdsBannerId;
    bool IsUnityAdsInitialized = false;

    bool IsUnityAdsInterstitialReady = false;
    bool IsUnityAdsRewardedReady = false;
    bool IsUnityAdsBannerReady = false;

    //ADMOB
    string adMobInterstitialId;
    string adMobRewardedId;
    string adMobBannerId;
    bool IsAdMobInitialized = false;
    bool IsAdMobInterstitialReady = false;
    bool IsAdMobRewardedReady = false;
    bool IsAdMobBannerReady = false;
#if ENABLE_ADMOB
    RewardedAd rewardBasedVideo;
    InterstitialAd interstitial;
    BannerView bannerView;
    AdSize adSize;
#endif
    RewardedStatus? current;

    Action<bool> interstitialCallback;
    Action<bool> prepareInterstitialCallback;
    Action<RewardedStatus> rewardedCallback;
    Action<bool> prepareRewardedCallback;
    Action<bool> prepareBannerCallback;
    Action<bool> bannerCallback;
    Action closeBannerCallback;
    bool testMode = false;
    int interstitialPrepareAttempts = 0;
    int rewardedPrepareAttempts = 0;
    int bannerPrepareAttempts = 0;
    AdVendor currentVendor;

    bool isAdmobRewarded = false;

    public int RewardAmount
    {
        get
        {
            if (currentVendor == AdVendor.Admob && GameController.Instance.UseRewardSettingsFromConsole)
            {
#if ENABLE_ADMOB
                return Convert.ToInt32(rewardBasedVideo.GetRewardItem().Amount);
#else
                return GameController.Instance.CoinsAdReward;
#endif
            }
            else
            {
                return GameController.Instance.CoinsAdReward;
            }
        }
    }

    public int BannerHeightInPixels
    {
        get
        {
            if (currentVendor == AdVendor.Admob)
            {
#if ENABLE_ADMOB
                return (int)bannerView.GetHeightInPixels();
#else
                return 50;//@todo
#endif
            }
            else
            {
                if (Screen.width >= 768)
                {
                    return 50;
                }
                return 90;//@todo
            }
        }
    }

    public void UpdateVendor()
    {
        if (GameController.Instance.UnityAds && !GameController.Instance.AdMob)
        {
            currentVendor = AdVendor.UnityAds;
        }
        else if (!GameController.Instance.UnityAds && GameController.Instance.AdMob)
        {
            currentVendor = AdVendor.Admob;
        }
        else
        {
            currentVendor = UnityEngine.Random.Range(0, 2) == 1 ? AdVendor.Admob : AdVendor.UnityAds;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
#if UNITY_ANDROID
        unityAdsId = GameController.Instance.UnityAdsGooglePlay;
        unityAdsInterstitialId = GameController.Instance.UnityAdsAndroidInterstitialID;
        unityAdsRewardedId = GameController.Instance.UnityAdsAndroidRewardedID;
        unityAdsBannerId = GameController.Instance.UnityAdsAndroidBannerID;
#elif UNITY_IOS
        unityAdsId = GameController.Instance.UnityAdsIOS;
        unityAdsInterstitialId = GameController.Instance.UnityAdsIosInterstitialID;
        unityAdsRewardedId = GameController.Instance.UnityAdsIosRewardedID;
        unityAdsBannerId = GameController.Instance.UnityAdsIosBannerID;
#endif

#if UNITY_ANDROID
        adMobInterstitialId = GameController.Instance.AdMobAndroidInterstitialID;
        adMobRewardedId = GameController.Instance.AdMobAndroidRewardedID;
        adMobBannerId = GameController.Instance.AdMobAndroidBannerID;
#elif UNITY_IOS
        adMobInterstitialId = GameController.Instance.AdMobIosInterstitialID;
        adMobRewardedId = GameController.Instance.AdMobIosRewardedID;
        adMobBannerId = GameController.Instance.AdMobIosBannerID;
#endif

#if UNITY_EDITOR
        testMode = true;
#endif
#if UNITY_AD
        currentVendor = AdVendor.UnityAds;
        UnityEngine.Advertisements.Advertisement.Initialize(unityAdsId, testMode, this);
        Advertisement.Banner.SetPosition(GameController.Instance.BannerOnTop ? BannerPosition.TOP_CENTER : BannerPosition.BOTTOM_CENTER);
#endif
#if ENABLE_ADMOB
        currentVendor = AdVendor.Admob;
        MobileAds.Initialize(initStatus =>
        {
            IsAdMobInitialized = true;
        });
        adSize = AdSize.Banner;

        CreateAdMobRewarded();
        CreateAdMobInterstitial();
        CreateAdMobBanner();

#endif

        if (GameController.Instance.NonpersonalizedAds)
        {
#if UNITY_AD
            MetaData gdprMetaData = new MetaData("gdpr");
            gdprMetaData.Set("consent", "false");
            Advertisement.SetMetaData(gdprMetaData);
#endif 
        }
    }

    public void SetNPA(bool state)
    {
        PlayerPrefs.SetInt("gdpr", state ? 1 : 0);
        GameController.Instance.NonpersonalizedAds = state;
#if UNITY_AD
        MetaData gdprMetaData = new MetaData("gdpr");
        gdprMetaData.Set("consent", !state ? "true" : "false");
        Advertisement.SetMetaData(gdprMetaData);
#endif  
    }

    public void CreateAdMobRewarded()
    {
#if ENABLE_ADMOB
        rewardBasedVideo = new RewardedAd(adMobRewardedId);
        rewardBasedVideo.OnUserEarnedReward += OnAdmobRewarded; //Subscribe method for event that is triggered when AD successfully watched
        rewardBasedVideo.OnAdClosed += OnAdmobRewardedShowComplete;
        rewardBasedVideo.OnAdFailedToLoad += OnAdmobFailedToLoad;
        rewardBasedVideo.OnAdLoaded += OnAdmobLoaded;
#endif  
    }

    public void CreateAdMobInterstitial()
    {
#if ENABLE_ADMOB
        interstitial = new InterstitialAd(adMobInterstitialId);
        interstitial.OnAdFailedToLoad += OnAdmobFailedToLoad;
        interstitial.OnAdClosed += OnAdmobInterstitialShowComplete;
        interstitial.OnAdLoaded += OnAdmobLoaded;
#endif  
    }

    public void CreateAdMobBanner()
    {
#if ENABLE_ADMOB
        bannerView = new BannerView(adMobBannerId, adSize, GameController.Instance.BannerOnTop ? AdPosition.Top : AdPosition.Bottom);
        bannerView.OnAdFailedToLoad += OnAdmobFailedToLoad;
        bannerView.OnAdClosed += OnAdmobBannerClose;
        bannerView.OnAdLoaded += OnAdmobLoaded;
        bannerView.OnAdOpening += OnAdmobBannerClose;
#endif  
    }

    public void PrepareInterstitial(Action<bool> callBack = null)
    {
        // Debug.Log("PrepareInterstitial");
        prepareInterstitialCallback = callBack;
        if (currentVendor == AdVendor.UnityAds && IsUnityAdsInterstitialReady || currentVendor == AdVendor.Admob && IsAdMobInterstitialReady)
        {
            prepareInterstitialCallback?.Invoke(true);
            prepareInterstitialCallback = null;
            return;
        }
        interstitialPrepareAttempts++;
        if (currentVendor == AdVendor.UnityAds)
        {
#if UNITY_AD
            Advertisement.Load(unityAdsInterstitialId, this);
#endif
        }
        else if (currentVendor == AdVendor.Admob)
        {
#if ENABLE_ADMOB
            if (interstitial == null)
            {
                CreateAdMobInterstitial();
            }
            interstitial.LoadAd(GetAdRequest());
#endif
        }
    }
#if ENABLE_ADMOB
    AdRequest GetAdRequest()
    {
        AdRequest request;
        if (GameController.Instance.NonpersonalizedAds)
        {
            //Debug.LogWarning("NONPERSONALIZED AD REQUESTED");
            request = new AdRequest.Builder().AddExtra("npa", "1").Build();
        }
        else request = new AdRequest.Builder().Build();
        return request;
    }
#endif


    public void ShowInterstitial(Action<bool> callBack = null)
    {
        // Debug.Log("ShowInterstitial");
        interstitialCallback = callBack;
        if (currentVendor == AdVendor.UnityAds)
        {
            if (IsUnityAdsInterstitialReady)
            {
#if UNITY_AD
                Advertisement.Show(unityAdsInterstitialId, this);
#endif
            }
            else
            {
                interstitialCallback?.Invoke(false);
                interstitialCallback = null;
            }
            Debug.Log($"SHOW UNITY INTERSTITIAL {IsUnityAdsInterstitialReady}");

        }
        else if (currentVendor == AdVendor.Admob)
        {
#if ENABLE_ADMOB
            if (interstitial.IsLoaded())
            {
                interstitial.Show();
            }
            else
            {
                interstitialCallback?.Invoke(false);
                interstitialCallback = null;
            }
            Debug.Log($"SHOW ADMOB INTERSTITIAL {interstitial.IsLoaded()}");

#endif

        }

        interstitialPrepareAttempts = 0;
        IsUnityAdsInterstitialReady = false;
    }

    public void PrepareBanner(Action<bool> prepareBannerCallback = null)
    {
        Debug.Log("Try PrepareBanner");
        this.prepareBannerCallback = prepareBannerCallback;
        // Set up options to notify the SDK of load events:
        if (currentVendor == AdVendor.UnityAds && (IsUnityAdsBannerReady 
#if UNITY_AD
        || Advertisement.Banner.isLoaded
#endif
        )
            || currentVendor == AdVendor.Admob && IsAdMobBannerReady)
        {
            Debug.Log("Banner already prepared");
            prepareBannerCallback?.Invoke(true);
            prepareBannerCallback = null;
            return;
        }
        bannerPrepareAttempts++;
        if (currentVendor == AdVendor.UnityAds)
        {
#if UNITY_AD

            BannerLoadOptions options = new BannerLoadOptions
            {
                loadCallback = OnBannerLoaded,
                errorCallback = OnBannerError
            };
            Debug.Log("LOADING UNITY BANNER");
            // Load the Ad Unit with banner content:
            Advertisement.Banner.Load(unityAdsBannerId, options);
#endif
        }
        else if (currentVendor == AdVendor.Admob)
        {
#if ENABLE_ADMOB
            if (bannerView == null)
            {
                CreateAdMobBanner();
            }
            Debug.Log("LOADING ADMOB BANNER");
            bannerView?.LoadAd(GetAdRequest());
#endif
        }
    }

    public void ShowBanner(Action<bool> bannerCallBack = null, Action closeBannerCallback = null)
    {
        this.bannerCallback = bannerCallBack;
        this.closeBannerCallback = closeBannerCallback;
        // Set up options to notify the SDK of show events:
        if (currentVendor == AdVendor.UnityAds)
        {
            if (IsUnityAdsBannerReady 
#if UNITY_AD
            && Advertisement.Banner.isLoaded
#endif
            )
            {
#if UNITY_AD
                BannerOptions options = new BannerOptions
                {
                    clickCallback = OnBannerClicked,
                    hideCallback = OnBannerHidden,
                    showCallback = OnBannerShown
                };
                // Show the loaded Banner Ad Unit:
                Advertisement.Banner.Show(unityAdsBannerId, options);
#endif

            }
#if UNITY_AD
            Debug.Log($"SHOW UNITY BANNER {IsUnityAdsBannerReady && Advertisement.Banner.isLoaded}");
#endif
            bannerCallback?.Invoke(IsUnityAdsBannerReady);
            bannerCallback = null;
        }
        else if (currentVendor == AdVendor.Admob)
        {
            if (IsAdMobBannerReady)
            {
#if ENABLE_ADMOB
                bannerView.Show();
#endif
            }
            Debug.Log($"SHOW ADMOB BANNER {IsAdMobBannerReady}");
            bannerCallback?.Invoke(IsAdMobBannerReady);
            bannerCallback = null;
        }
        IsUnityAdsBannerReady = false;
    }

    public void PrepareRewarded(Action<bool> callBack = null)
    {
        // Debug.Log("PrepareRewarded");
        prepareRewardedCallback = callBack;
        if (currentVendor == AdVendor.UnityAds && IsUnityAdsRewardedReady || currentVendor == AdVendor.Admob && IsAdMobRewardedReady)
        {
            prepareRewardedCallback?.Invoke(true);
            prepareRewardedCallback = null;
            return;
        }
        rewardedPrepareAttempts++;
        if (currentVendor == AdVendor.UnityAds)
        {
#if UNITY_AD
            Debug.Log("PrepareRewarded Unity");
            Advertisement.Load(unityAdsRewardedId, this);
#endif
        }
        else if (currentVendor == AdVendor.Admob)
        {
#if ENABLE_ADMOB
            if (rewardBasedVideo == null)
            {
                CreateAdMobRewarded();
            }
            Debug.Log("PrepareRewarded Admob");
            // Load the rewarded video ad with the request.
            rewardBasedVideo.LoadAd(GetAdRequest());
#endif
        }
    }

    public void ShowRewarded(Action<RewardedStatus> callBack = null)
    {
        // Debug.Log("ShowRewarded");
        rewardedCallback = callBack;
        if (currentVendor == AdVendor.UnityAds)
        {
            Debug.Log($"SHOW UNITY REWARDED {IsUnityAdsRewardedReady}");
            if (IsUnityAdsRewardedReady)
            {
#if UNITY_AD
                Advertisement.Show(unityAdsRewardedId, this);
#endif
            }
            else
            {
                rewardedCallback?.Invoke(new RewardedStatus()
                {
                    isWatched = false,
                });
                rewardedCallback = null;
            }
        }
        else if (currentVendor == AdVendor.Admob)
        {
#if ENABLE_ADMOB
            bool isAdmobLoaded = rewardBasedVideo.IsLoaded();
            Debug.Log($"SHOW ADMOB REWARDED {isAdmobLoaded}");
            if (isAdmobLoaded)
            {
                rewardBasedVideo.Show();
            }
            else
            {
                rewardedCallback?.Invoke(new RewardedStatus()
                {
                    isWatched = false,
                });
                rewardedCallback = null;
            }

#endif

        }
        IsUnityAdsRewardedReady = false;
        rewardedPrepareAttempts = 0;
    }

    public void KillBanner()
    {
        // Hide the banner:
        Debug.Log("KILLING BANNER");
#if UNITY_AD
        Advertisement.Banner.Hide(true);
        IsUnityAdsBannerReady = false;
#endif
#if ENABLE_ADMOB
        bannerView?.Hide();
        bannerView?.Destroy();
        IsAdMobBannerReady = false;
#endif
        FindObjectOfType<GameSceneCanvasBehavior>()?.DefaultSize();
    }
#if UNITY_AD

    public void OnInitializationComplete()
    {
        // Debug.LogWarning("OnInitializationComplete");
        IsUnityAdsInitialized = true;
    }

    public void OnInitializationFailed(UnityAdsInitializationError error, string message)
    {
        // Debug.LogWarning("OnInitializationFailed");
        Debug.LogWarning(message);
        IsUnityAdsInitialized = false;

    }

    public void OnUnityAdsAdLoaded(string placementId)
    {
        // Debug.LogWarning("OnUnityAdsAdLoaded");

        if (placementId == unityAdsInterstitialId)
        {
            interstitialPrepareAttempts = 0;
            IsUnityAdsInterstitialReady = true;
            prepareInterstitialCallback?.Invoke(true);
            prepareInterstitialCallback = null;

        }
        if (placementId == unityAdsRewardedId)
        {
            rewardedPrepareAttempts = 0;
            IsUnityAdsRewardedReady = true;
            prepareRewardedCallback?.Invoke(true);
            prepareRewardedCallback = null;
        }
    }

    public void OnUnityAdsFailedToLoad(string placementId, UnityAdsLoadError error, string message)
    {
        // Debug.LogWarning("OnUnityAdsFailedToLoad");
        Debug.LogWarning(message);
        if (placementId == unityAdsInterstitialId)
        {
            if (interstitialPrepareAttempts >= InterstitialAttempts)
            {
                interstitialPrepareAttempts = 0;
                prepareInterstitialCallback?.Invoke(false);
                prepareInterstitialCallback = null;
                return;
            }
            PrepareInterstitial();

        }
        if (placementId == unityAdsRewardedId)
        {
            if (rewardedPrepareAttempts >= RewardedAttempts)
            {
                rewardedPrepareAttempts = 0;
                prepareRewardedCallback?.Invoke(false);
                prepareRewardedCallback = null;
                return;
            }
            PrepareRewarded();
        }

    }

    public void OnUnityAdsShowFailure(string placementId, UnityAdsShowError error, string message)
    {
        Debug.Log($"OnUnityAdsShowFailure {message}");

        if (placementId == unityAdsInterstitialId)
        {
            interstitialPrepareAttempts = 0;
            IsUnityAdsInterstitialReady = false;
            interstitialCallback?.Invoke(false);
            interstitialCallback = null;
        }
        if (placementId == unityAdsRewardedId)
        {
            rewardedPrepareAttempts = 0;
            IsUnityAdsRewardedReady = false;
            rewardedCallback?.Invoke(new RewardedStatus()
            {
                isWatched = false,
            });
            rewardedCallback = null;
        }
    }
    public void OnUnityAdsShowStart(string placementId)
    {

        // throw new System.NotImplementedException();
    }

    public void OnUnityAdsShowClick(string placementId)
    {
        // throw new System.NotImplementedException();
    }

    public void OnUnityAdsShowComplete(string placementId, UnityAdsShowCompletionState showCompletionState)
    {
        // Debug.Log("OnUnityAdsShowComplete");
        // Debug.Log(placementId);
        // Debug.Log(showCompletionState.ToString());
        if (placementId == unityAdsInterstitialId)
        {
            interstitialCallback?.Invoke(true);
            interstitialCallback = null;
            interstitialPrepareAttempts = 0;
            IsUnityAdsInterstitialReady = false;
        }
        if (placementId == unityAdsRewardedId)
        {
            rewardedCallback?.Invoke(new RewardedStatus()
            {
                isWatched = showCompletionState == UnityAdsShowCompletionState.COMPLETED,
                rewardAmount = GameController.Instance.CoinsAdReward
            });
            rewardedCallback = null;
            interstitialPrepareAttempts = 0;
            IsUnityAdsInterstitialReady = false;
        }
    }
#endif

    private void OnBannerShown()
    {
        bannerCallback?.Invoke(true);
        // throw new NotImplementedException();
    }

    private void OnBannerHidden()
    {
        closeBannerCallback?.Invoke();
        KillBanner();
    }

    private void OnBannerClicked()
    {
        // throw new NotImplementedException();
    }

    public void OnBannerLoaded()
    {
        Debug.LogWarning($"OnBannerLoaded");
        bannerPrepareAttempts = 0;
        IsUnityAdsBannerReady = true;
        this.prepareBannerCallback?.Invoke(true);
        this.prepareBannerCallback = null;
    }

    public void OnBannerError(string message)
    {
        Debug.LogWarning($"OnBannerError {message}");
        KillBanner();
        if (bannerPrepareAttempts >= BannerAttempts)
        {
            IsUnityAdsBannerReady = false;
            bannerPrepareAttempts = 0;
            prepareBannerCallback?.Invoke(false);
            prepareBannerCallback = null;
            return;
        }
        PrepareBanner();
    }

#if ENABLE_ADMOB

    public void OnAdmobInterstitialShowComplete(object sender, EventArgs e)
    {
        Debug.Log($"OnAdmobInterstitialShowComplete");
        interstitialPrepareAttempts = 0;
        IsUnityAdsInterstitialReady = false;
        CreateAdMobInterstitial();
        interstitialCallback?.Invoke(true);
        interstitialCallback = null;
    }

    private void OnAdmobBannerClose(object sender, EventArgs e)
    {
        //  Debug.LogWarning("Banner watched");
        closeBannerCallback?.Invoke();
        KillBanner();
        CreateAdMobBanner();
        IsAdMobBannerReady = false;
    }

    private void OnAdmobLoaded(object sender, EventArgs e)
    {
        if (sender.GetType() == typeof(RewardedAd))
        {
            Debug.Log("EVENT REWARDED READY");
            rewardedPrepareAttempts = 0;
            IsAdMobRewardedReady = true;
            prepareRewardedCallback?.Invoke(true);
            prepareRewardedCallback = null;
        }
        else if (sender.GetType() == typeof(BannerView))
        {
            Debug.Log("EVENT BANNER READY");
            bannerPrepareAttempts = 0;
            IsAdMobBannerReady = true;
            prepareBannerCallback?.Invoke(true);
            prepareBannerCallback = null;
        }
        else
        {
            Debug.Log("EVENT INTERSTITIAL READY");
            interstitialPrepareAttempts = 0;
            IsAdMobInterstitialReady = true;
            prepareInterstitialCallback?.Invoke(true);
            prepareInterstitialCallback = null;
        };
    }

    private void OnAdmobFailedToLoad(object sender, AdFailedToLoadEventArgs e)
    {
        if (sender.GetType() == typeof(RewardedAd))
        {
            Debug.LogError("ADMOB REWARDED FAILED TO LOAD \n" + e.LoadAdError);
            if (rewardedPrepareAttempts >= RewardedAttempts)
            {
                rewardedPrepareAttempts = 0;
                CreateAdMobRewarded();
                prepareRewardedCallback?.Invoke(false);
                prepareRewardedCallback = null;
                return;
            }
            PrepareRewarded();
        }
        else if (sender.GetType() == typeof(BannerView))
        {
            Debug.LogError("ADMOB BANNER FAILED TO LOAD \n" + e.LoadAdError);
            if (bannerPrepareAttempts >= BannerAttempts)
            {
                bannerPrepareAttempts = 0;
                CreateAdMobBanner();
                prepareBannerCallback?.Invoke(false);
                prepareBannerCallback = null;
                return;
            }
            PrepareBanner();
            // bannerView.Hide();
        }
        else
        {
            Debug.LogError("ADMOB INTERSTITIAL FAILED TO LOAD \n" + e.LoadAdError);
            if (interstitialPrepareAttempts >= InterstitialAttempts)
            {
                interstitialPrepareAttempts = 0;
                CreateAdMobInterstitial();
                prepareInterstitialCallback?.Invoke(false);
                prepareInterstitialCallback = null;
                return;
            }
            PrepareInterstitial();
        };

    }

    public void OnAdmobRewardedShowComplete(object sender, EventArgs args)
    {
        Debug.Log($"OnAdmobRewardedShowComplete {isAdmobRewarded}");
        if (!isAdmobRewarded)
        {
            IsAdMobRewardedReady = false;
            rewardedPrepareAttempts = 0;
            CreateAdMobRewarded();
            rewardedCallback(new RewardedStatus()
            {
                isWatched = false,
            });
            rewardedCallback = null;

        }
        isAdmobRewarded = false;
    }

    public void OnAdmobRewarded(object sender, Reward args)
    {
        isAdmobRewarded = true;
        Debug.Log("OnAdmobRewarded");

        int admobReward = Convert.ToInt32(args.Amount);
        int rewardAmount = GameController.Instance.UseRewardSettingsFromConsole && admobReward > 0 ? admobReward : GameController.Instance.CoinsAdReward;

        IsAdMobRewardedReady = false;
        rewardedPrepareAttempts = 0;

        CreateAdMobRewarded();
        rewardedCallback(new RewardedStatus()
        {
            isWatched = true,
            rewardAmount = rewardAmount
        });
        rewardedCallback = null;
    }
#endif

}

public struct RewardedStatus
{
    public bool isWatched;
    public int rewardAmount;
}

public enum AdVendor
{
    UnityAds,
    Admob,
}