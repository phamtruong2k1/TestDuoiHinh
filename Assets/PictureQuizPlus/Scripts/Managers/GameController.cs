using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.Events;

public class GameController : MonoBehaviour //Main class that handles and stores game data and preferences
{
    public Localization[] Localizations { get => LoadingSettings.localizations; }
    public bool IsContentStoredOnWebServer { get => LoadingSettings.isContentStoredOnWebServer; }
    public string HttpStorageUrl { get => LoadingSettings.httpStorageUrl; }
    public string CheckInternetConnectionUrl { get => LoadingSettings.checkInternetConnectionUrl; }
    public string CheckHostConnectionFile { get => LoadingSettings.checkHostConnectionFile; }
    public string RemoteImagesExtension { get => LoadingSettings.remoteImagesExtension; }

    public int DefaultWinReward { get => GameSettings.defaultWinReward; }
    public int SolveTaskCost { get => GameSettings.solveTaskCost; }
    public int StartCoins { get => GameSettings.startCoins; }
    public int FullLettersCount { get => GameSettings.fullLettersCount; }
    public bool IsThirdRowRequired { get => GameSettings.isThirdRowRequired; }
    public int AddSecondLineAfterXLetters { get => GameSettings.addSecondLineAfterXLetters; }
    public bool DisableCharacters { get => GameSettings.disableCharacters; }
    public bool DisableFireworks { get => GameSettings.disableFireworks; }
    public bool DisablePictureMoveInWithHand { get => GameSettings.disablePictureMoveInWithHand; }
    public bool SimplifiedWinPopup { get => GameSettings.simplifiedWinPopup; }
    public bool WithoutWinPopup { get => GameSettings.withoutWinPopup; }
    public bool EnableSubcategories { get => GameSettings.enableSubcategories; }
    public bool ImageDescriptionButton { get => GameSettings.imageDescriptionButton; }
    public bool DebugButton { get => GameSettings.debugButton; }
    public bool AboutButton { get => GameSettings.aboutButton; }
    public bool RightToLeftWords { get => GameSettings.rightToLeftWords; }
    public bool MultipleCoinsAdReward { get => GameSettings.multipleCoinsAdReward; }
    public int CoinsMultiplayer { get => GameSettings.coinsMultiplayer; }
    public bool EnableEducation { get => GameSettings.enableEducation; }
    public int PixelateFirst { get => GameSettings.pixelateFirst; }
    public int PixelateSecond { get => GameSettings.pixelateSecond; }
    public int PixelateThird { get => GameSettings.pixelateThird; }
    public int FinalImage { get => GameSettings.finalImage; }
    public float AnimationSpeed { get => GameSettings.animationSpeed; }
    public int PixelateCost { get => GameSettings.pixelateCost; }
    public Texture2D Pen { get => GameSettings.pen; }
    public float PenFrequnecy { get => GameSettings.penFrequnecy; }
    public int HuskFrequnecy { get => GameSettings.huskFrequnecy; }
    public int ErasureCost { get => GameSettings.erasureCost; }
    public int GridSize { get => GameSettings.gridSize; }
    public float AimSpeed { get => GameSettings.aimSpeed; }
    public bool DisableAiming { get => GameSettings.disableAiming; }
    public int PlankCost { get => GameSettings.plankCost; }
    public int FirstBet { get => GameSettings.firstBet; }
    public int SecondBet { get => GameSettings.secondBet; }
    public int ThirdBet { get => GameSettings.thirdBet; }
    public int FourthBet { get => GameSettings.fourthBet; }
    public bool UseBets { get => GameSettings.useBets; }
    public bool DeductCoinsWhenWrong { get => GameSettings.deductCoinsWhenWrong; }
    public Sprite BackgroundIfWithoutImage { get => GameSettings.backgroundIfWithoutImage; }
    public bool EnableLevelLocking { get => GameSettings.enableLevelLocking; }
    public int UnlockedAtStart { get => GameSettings.unlockedAtStart; }
    public int MustBeAvailableAtLeast { get => GameSettings.mustBeAvailableAtLeast; }
    public bool EnableSubcategoryLocking { get => GameSettings.enableSubcategoryLocking; }
    public int UnlockedSubCategoriesAtStart { get => GameSettings.unlockedSubCategoriesAtStart; }
    public int SubCategoriesMustBeAvailableAtLeast { get => GameSettings.subCategoriesMustBeAvailableAtLeast; }
    public bool IsRatePopupNeeded { get => GameSettings.isRatePopupNeeded; }
    public int AfterEeachLevel { get => GameSettings.afterEeachLevel; }
    public string GpAppUrl { get => GameSettings.gpAppUrl; }
    public string AppStoreUrl { get => GameSettings.iosAppUrl; }
    public bool IsMoreGamesEnabled { get => GameSettings.isMoreGamesEnabled; }
    public AnotherPublisherGame[] MoreGamesItems { get => GameSettings.moreGamesItems; }

    ////////////////////////// ADS IAP GDPR //////////////////////////
    public bool AnyAds { get => UnityAds || AdMob; }
    public bool UnityAds { get => GameSettings.unityAds; }
    public bool UnityIap { get => GameSettings.unityIap; }
    public bool AdMob { get => GameSettings.adMob; }
    public bool IsBanner { get => GameSettings.isBanner; }
    public bool UseRewardSettingsFromConsole { get => GameSettings.useRewardSettingsFromConsole; }
    public bool BannerOnTop { get => GameSettings.bannerOnTop; }
    public int BannerEachLevel { get => GameSettings.bannerEachLevel; }
    public bool NonpersonalizedAds { get => GameSettings.nonpersonalizedAds; set => GameSettings.nonpersonalizedAds = value; }
    public bool GooglePlaySaves { get => GameSettings.googlePlaySaves; }
    public bool InstaLoginGpGames { get => GameSettings.instaLoginGpGames; }
    public int SuggestToLoginToGPGamesAfterLevel { get => GameSettings.suggestToLoginToGPGamesAfterLevel; }
    public bool GDPRconsent { get => GameSettings.GDPRconsent; }
    public bool ConsentOnStart { get => GameSettings.consentOnStart; }
    public string PolicyLink { get => GameSettings.policyLink; }
    public bool DisableAdsOnPurchase { get => GameSettings.disableAdsOnPurchase; }
    public int CoinsAdReward { get => GameSettings.coinsAdReward; }
    public int ShowAdAfterLevel { get => GameSettings.showAdAfterLevel; }
    public string DelayBetweenAds { get => GameSettings.delayBetweenAds; }
    public bool Sharing { get => GameSettings.sharing; }
    //UNITY ADS
    public string UnityAdsGooglePlay { get => GameSettings.unityAdsGooglePlay; }
    public string UnityAdsIOS { get => GameSettings.unityAdsApple; }
    public string UnityAdsAndroidRewardedID { get => GameSettings.unityAdsAndroidRewardedID; }
    public string UnityAdsAndroidInterstitialID { get => GameSettings.unityAdsAndroidInterstitialID; }
    public string UnityAdsAndroidBannerID { get => GameSettings.unityAdsAndroidBannerID; }
    public string UnityAdsIosRewardedID { get => GameSettings.unityAdsIosRewardedID; }
    public string UnityAdsIosInterstitialID { get => GameSettings.unityAdsIosInterstitialID; }
    public string UnityAdsIosBannerID { get => GameSettings.unityAdsIosBannerID; }
    //ADMOB
    public string AdMobAndroidRewardedID { get => GameSettings.adMobAndroidRewardedID; }
    public string AdMobAndroidInterstitialID { get => GameSettings.adMobAndroidInterstitialID; }
    public string AdMobAndroidBannerID { get => GameSettings.adMobAndroidBannerID; }
    public string AdMobIosRewardedID { get => GameSettings.adMobIosRewardedID; }
    public string AdMobIosInterstitialID { get => GameSettings.adMobIosInterstitialID; }
    public string AdMobIosBannerID { get => GameSettings.adMobIosBannerID; }
    public bool UseSimpleMenu { get => GameSettings.useSimpleMenu; }

    private InAppProduct[] _inAppProducts;
    public InAppProduct[] InAppProducts { get => _inAppProducts; set => _inAppProducts = value; }

#if GP_SAVES
    public GooglePlaySaves gpSaves;
#endif  

    public string AppUrl
    {
        get
        {
#if UNITY_ANDROID
            return Instance.GpAppUrl;
#elif UNITY_IOS
            return Instance.AppStoreUrl;
#else
            return "";
#endif
        }
    }

    ///////////////////////// SELF PROPERTIES //////////////////////////
    public static GameController Instance { get; private set; }
    public LoadingSettings LoadingSettings { get; set; }
    private GameSettings GameSettings { get; set; }
    public ResourcesManager ResourcesManager { get; set; }
    public SaveManager SaveManager { get; private set; }
    private LoadingManager LoadingManager;
    private LocalizationData localizationData;
    public GenericPopupController popup;
    public IAPController iap;
    public AdsController ads;
    public DataToSave savedProgress;
    internal string errorMessage;
    internal bool shouldShowInterstitial = false;

    public Category CurrentCategory { get; set; }
    public Level CurrentLevel { get => CurrentCategory.currentLevel; }
    public List<Category> Categories { get; private set; }
    public List<SubCategory> SubCategories { get; private set; }
    public Dictionary<LocalizationItemType, string> LocalizedText { get; private set; }
    public bool IsDataReady
    {
        get { return LoadingManager != null && LoadingManager.IsReady && !LoadingManager.IsError; }
    }

    public bool LoadingScreenClosed => LoadingManager != null && LoadingManager.LoadingScreenClosed;


    public bool IsSettingsReady
    {
        get
        {
            return GameSettings != default(GameSettings);
        }
    }

    public char[] RandomLetters { get; private set; }
    public Color[] LevelColors { get => GameSettings.levelColors; }
    public bool IsCategoryCompleted { get; set; }
    public bool IsSharing { get; set; }
    public int Coins { get; private set; }
    public string Path { get; private set; }
    public Localization CurrentLocalization { get; private set; }

    private int attemptsToLoadNextLevelResource = 0;

    void Awake()
    {
        //Unity singleton 
        if (Instance == null)
        {
            // Utils.CheckResolution();
            Instance = this;
            DontDestroyOnLoad(gameObject);
            StartCoroutine(Initialize());
        }
        else
        {
            Destroy(gameObject);
            return;
        }

    }
    internal void Reload()
    {
        if (LoadingManager != null && !LoadingManager.IsReady && !LoadingManager.IsError)
        {
            return;
        }
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(0);
        asyncLoad.completed += (x) => StartCoroutine(Initialize());
    }

    public void ReloadDirectory()
    {
        LevelInfo reset = new LevelInfo() { directoryName = GameController.Instance.CurrentCategory.folderName, currentLevel = 1 };
        LevelStateController.ResetDirectory(reset);
        SaveGameState(reset);
    }

    private void resetData()
    {
        AssetBundle.UnloadAllAssetBundles(true);
        Destroy(LoadingManager);
        Categories = null;
        RandomLetters = null;
        SubCategories = null;
        LocalizedText = new Dictionary<LocalizationItemType, string>();
    }


    public IEnumerator prepareNextLevelData()
    {
        // Debug.Log("An attempt to prepare next level data");
        attemptsToLoadNextLevelResource++;

        if (attemptsToLoadNextLevelResource > 5) yield return false;

        yield return new WaitUntil(() => LevelStateController.IsLevelReady);
        yield return new WaitForSeconds(3f);

        if (CurrentCategory.currentLevel.index >= CurrentCategory.Length) yield return false;

        Level nextLevel = CurrentCategory.Levels.FirstOrDefault(l => l.index == (CurrentCategory.currentLevel.index + 1));

        if (nextLevel.index == default) yield return false;

        LoadingStep<LevelImagesResource> step = new LoadingStep<LevelImagesResource>()
        {
            CreateCoroutineWithData = () => GameController.Instance.ResourcesManager.GetAsync<LevelImagesResource, Sprite>(nextLevel),
        };

        step.Start();
        yield return new WaitUntil(() => step.IsReady || step.IsError);

        if (step.IsError)
        {
            yield return new WaitForSeconds(1f);
            StartCoroutine(prepareNextLevelData());
        }
    }

    public IEnumerator Initialize()
    {
        resetData();

        LocalResourcesManager localResourcesManager = new LocalResourcesManager();
        LoadingSettings = localResourcesManager.Get<LoadingSettingsResource, LoadingSettings>().GetFirst();

        if (PlayerPrefs.GetInt("lang_chosen", 0) == 0)
        {
            PlayerPrefs.SetString("language", Localizations.First().filename);
            if (Localizations.Length == 1)
            {
                PlayerPrefs.SetInt("lang_chosen", 1);
            }
        }

        CurrentLocalization = Localizations.First(l => l.filename == PlayerPrefs.GetString("language", "english"));

        if (IsContentStoredOnWebServer)
        {
            ResourcesManager = new RemoteResourcesManager();
        }
        else
        {
            ResourcesManager = localResourcesManager;
        }

        LoadingManager = gameObject.AddComponent<LoadingManager>();
        LoadingManager.stepCompleted += (ILoadingStep step) =>
        {
            switch (step.Type)
            {
                case LoadingStepType.settings:
                    GameSettings = ((GameSettingsResource)step.PayloadObject).GetFirst();
                    // Debug.Log(GameSettings);
                    SaveManager = new SaveManager();
                    GameSettings.nonpersonalizedAds = PlayerPrefs.GetInt("gdpr", 0) == 1 ? true : false;
                    _inAppProducts = GameSettings.inAppProducts;
                    break;
                case LoadingStepType.game_data:
                    localizationData = ((LocalizationDataResource)step.PayloadObject).GetFirst();
                    for (int i = 0; i < localizationData.gameItems.Length; i++)
                    {
                        if (!LocalizedText.ContainsKey(localizationData.gameItems[i].key))
                        {
                            LocalizedText.Add(localizationData.gameItems[i].key, localizationData.gameItems[i].value);
                        }
                    }
                    UpdateCategories();
                    RandomLetters = localizationData.randomLetters;
                    break;
                default:
                    break;
            }

        };

        popup = gameObject.AddComponent<GenericPopupController>();
        LoadingManager.Initialize();

        yield return new WaitUntil(() => LoadingManager.IsReady);

#if UNITY_IAP
        if (iap == null)
        {
            iap = gameObject.AddComponent<IAPController>();
        }
        else
        {
            iap.SetupLocalPrices();
        }
#endif
        if (AnyAds && ads == null)
        {
            ads = gameObject.AddComponent<AdsController>();
        }


#if GP_SAVES
        if (gpSaves == null)
        {
            gpSaves = gameObject.AddComponent<GooglePlaySaves>();
        }
#endif     
        Coins = SaveManager.GetCoins();
        LevelStateController.OnDataSave += SaveGameState; //Subscribe for the data saving event
    }

    public string GetLocalizedValue(LocalizationItemType key) //Method that is called from each element that needs to know localized text
    {
        if (key == LocalizationItemType.empty || LocalizedText == null) return "";
        if (LocalizedText.ContainsKey(key))
        {
            return Utils.RenderMustache(LocalizedText[key], key);
        }
        else return "Undefined";
    }

    public Color GetColor(ThemeColorEnum key) //Method that is called from each element that needs to know localized text
    {
        try
        {
            return GameSettings.colors.First(c => c.key == key).color;

        }
        catch (System.Exception)
        {
            throw new Exception($"No color found with key {key}");
        }
    }

    internal void LoadNextCategory()
    {
        UpdateCategories();
    }

    public int GetCoinsCount() //Return coins count to suppliant
    {
        return Coins;
    }

    public void SpendCoins(Hint hint, object obj) //Set coins count when hints are used
    {
        Coins -= LevelStateController.GetHintPrice(hint);
        SaveManager.SetCoinsData(Coins);
        Utils.SetCoinsText();
    }


    public void EarnCoins(int value, bool sound) //Set coins count when earning coins
    {
        Coins += value;
        if (sound)
        {
            SoundsController.instance.PlaySound("coins");
        }
        SaveManager.SetCoinsData(Coins);
        Utils.SetCoinsText();
    }

    public void SaveGameState(LevelInfo data) //Save current game state to the file
    {
        SaveManager.SetDirectoryState(data);
        SaveManager.Save();
        UpdateCategories();

    }


    public Level[] GetTasksInfo(string dirname)
    {
        foreach (var item in Categories)
        {
            if (item.Name == dirname)
            {
                return item.Levels;
            }
        }
        return null;
    }

    public void ResetHintsOnLanguageChange() //Reset hints when the language changed
    {
        SaveManager.ResetLetters();
        SaveManager.Save();
    }

    internal void CheckIsPopupNeeded(string saveString) //Check is GooglePlaySaves popup need to be shown
    {
        if (!SaveManager.Compare(saveString))
        {
            savedProgress = SaveManager.StringToStruct(saveString);
            popup.EnqueuePopup<SavedProgressPopup>(ThemeColorEnum.Secondary);
        }
        else
        {
            SaveManager.Save();
        }
    }

    public void SaveFromGP() //Save data tto the device from GooglePlaySaves
    {
        SaveManager.SetCoinsData(savedProgress.coinsCount);
        SaveManager.SetAds(savedProgress.ads);
        foreach (var item in savedProgress.levelsInfo)
        {
            SaveManager.SetDirectoryState(item);
        }
        SaveManager.Save();
        Reload();
    }

    public void DisableAds()
    {
        SaveManager.SetAds(false);
        ads?.KillBanner();
    }

    public bool IsAdsAllowed()
    {
        return SaveManager.GetAds();
    }
    public Category GetCategoryByName(string name)
    {
        return this.Categories.FirstOrDefault(c => c.Name == name);
    }

    private void UpdateCategories()
    {
        var hydratedCategories = localizationData.tasksData.OrderBy(c => c.sortingIndex)
                .Select((category, idx) =>
                {
                    category.sortingIndex = idx + 1;
                    category.savedData = SaveManager.GetDirectoryState(category.Name);
                    category.Levels = category.Levels
                        .OrderBy(l => l.index)
                        .Select((l, lidx) =>
                        {
                            l.index = lidx + 1;
                            l.Parents = l.Parents.Prepend(category.Name);
                            l.reward = l.reward == 0 ? DefaultWinReward : l.reward;
                            l.gameType = category.useMixedTypes ? l.gameType : category.gameType;
                            l.noImage = category.forceNoImage || l.noImage;
                            l.isComplete = l.index < category.savedData.currentLevel;
                            return l;
                        })
                        .ToArray();
                    category.currentLevel = category.Levels.FirstOrDefault(l => !l.isComplete);
                    category.previousLevel = category.Levels.LastOrDefault(l => l.isComplete);
                    category.isComplete = !category.Levels.Any(l => !l.isComplete);
                    return category;
                });
        if (this.EnableSubcategories)
        {
            var subCategories = localizationData.subCategories
                .OrderBy(sc => sc.sortingIndex)
                .Select((sc, scIdx) =>
                {
                    sc.sortingIndex = scIdx + 1;
                    var categories = sc.subcategories
                        .Select(name => hydratedCategories.FirstOrDefault(c => c.Name == name))
                        .Where(c => !c.Equals(default(Category)))
                        .OrderBy(c => c.sortingIndex)
                        .Select((c, idx) =>
                        {
                            c.sortingIndex = idx + 1;
                            c.Parents = c.Parents.Append(sc.Name);
                            return c;
                        });

                    var categoriesWithUnlockedState = GetCategoriesWithLockingState(
                        categories, this.EnableLevelLocking, this.UnlockedAtStart, this.MustBeAvailableAtLeast);
                    sc.isComplete = !categoriesWithUnlockedState.Any(c => !c.isComplete);
                    sc.isUnlocked = !this.EnableSubcategoryLocking;
                    sc.Categories = categoriesWithUnlockedState
                        .Select(c =>
                        {
                            c.Levels = c.Levels
                                .Select(l =>
                                {
                                    l.Parents = l.Parents.Prepend(sc.Name);
                                    return l;
                                })
                                .ToArray();
                            c.currentLevel.Parents = c.currentLevel.Parents.Prepend(sc.Name);
                            c.previousLevel.Parents = c.previousLevel.Parents.Prepend(sc.Name);
                            return c;
                        })
                        .OrderBy(c => !c.isUnlocked)
                        .ToList();
                    return sc;
                });

            if (this.EnableSubcategoryLocking)
            {
                subCategories = subCategories
                    .Select((sc, idx) =>
                    {
                        sc.isUnlocked = sc.sortingIndex <= UnlockedSubCategoriesAtStart || sc.Categories.Any(c => !c.savedData.isNewRecord);
                        return sc;
                    });
                int currentlyAvailable = subCategories.Count(sc => sc.isUnlocked && !sc.isComplete);
                int lockedCount = subCategories.Count(sc => !sc.isUnlocked);
                if (lockedCount > 0 && currentlyAvailable < this.SubCategoriesMustBeAvailableAtLeast)
                {
                    int shouldUnlock = this.SubCategoriesMustBeAvailableAtLeast - currentlyAvailable;
                    subCategories = subCategories
                        .Select(sc =>
                        {
                            if (shouldUnlock == 0) return sc;

                            if (!sc.isUnlocked)
                            {
                                shouldUnlock--;
                                sc.isUnlocked = true;
                            }
                            return sc;
                        });
                }
            }
            this.SubCategories = subCategories.OrderBy(sc => !sc.isUnlocked).ToList();
            this.Categories = subCategories.SelectMany(sc => sc.Categories).ToList();
            var arr2 = this.Categories.First().Parents.ToArray();
            var arr4 = this.Categories.ToArray();
            var arr = this.Categories.SelectMany(c => c.Levels).Select(l => l.Parents.ToArray()).ToArray();
            var ar3 = this.SubCategories.First().Categories.First().Levels.First().Parents.ToArray();
        }
        else
        {
            this.Categories = GetCategoriesWithLockingState(
                hydratedCategories, this.EnableLevelLocking, this.UnlockedAtStart, this.MustBeAvailableAtLeast
            ).ToList();
        }

        if (CurrentCategory.sortingIndex > 0)
        {
            Category cat = Categories.FirstOrDefault((c) => c.folderName == CurrentCategory.folderName);
            if (cat.sortingIndex > 0)
            {
                CurrentCategory = cat;
            }
        }
    }

    private IEnumerable<Category> GetCategoriesWithLockingState(
        IEnumerable<Category> categories, bool shouldLock, int unlockedAtStart, int mustBeAvailableAtLeast)
    {
        if (!shouldLock) return categories.Select(c => { c.isUnlocked = true; return c; });

        var categoriesWithLockingState = categories
            .Select((c, idx) =>
            {
                c.isUnlocked = c.sortingIndex <= unlockedAtStart || !c.savedData.isNewRecord;
                return c;
            });

        int currentlyAvailable = categoriesWithLockingState.Count(c => c.isUnlocked && !c.isComplete);
        int lockedCount = categoriesWithLockingState.Count(c => !c.isUnlocked);

        if (lockedCount > 0 && currentlyAvailable < mustBeAvailableAtLeast)
        {
            int shouldUnlock = mustBeAvailableAtLeast - currentlyAvailable;
            categoriesWithLockingState = categoriesWithLockingState
                .Select(c =>
                {
                    if (shouldUnlock == 0) return c;

                    if (!c.isUnlocked)
                    {
                        shouldUnlock--;
                        c.isUnlocked = true;
                    }
                    return c;
                });
        }

        return categoriesWithLockingState;
    }

    public override bool Equals(object obj)
    {
        return obj is GameController manager &&
               base.Equals(obj);
    }

    public override int GetHashCode()
    {
        int hashCode = -1982140399;
        hashCode = hashCode * -1521134295 + base.GetHashCode();
        return hashCode;
    }
}