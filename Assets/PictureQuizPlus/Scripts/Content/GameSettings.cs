using System;
using UnityEngine;

[CreateAssetMenu, Serializable]
public class GameSettings : ScriptableObject //Root ScriptableObject that representes Ads settings instance in the Inspector. Check InspectorGUI class that is in symbiosis with it
{
    public bool isUnityIapSettingOpened = false;
    public bool isAdsSettingOpened = false;
    public bool isUnityAdsSettingOpened = false;
    public bool isUnityAdMobSettingOpened = false;
    public bool isGDPRSettingOpened = false;
    public bool isSharingOptionsOpened = false;
    public bool isGeneralOpened = false;
    public bool isAdsIapOpened = false;
    public bool isUIOpened = false;
    public bool isColorsOpened = false;
    public int defaultWinReward = 60;
    public int solveTaskCost = 200;
    [Min(0)]
    public int startCoins = 1000;
    [Range(8, 30)]
    public int fullLettersCount = 16;
    public bool isThirdRowRequired = false;
    [Min(0)]
    public int addSecondLineAfterXLetters = 12;
    public bool disableCharacters = false;
    public bool disableFireworks = false;
    public bool disablePictureMoveInWithHand = false;
    public bool simplifiedWinPopup = false;
    public bool withoutWinPopup = false;
    public bool enableSubcategories = true;
    public bool imageDescriptionButton = false;
    public bool debugButton = true;
    public bool aboutButton = true;
    public bool rightToLeftWords = false;
    public bool multipleCoinsAdReward = true;
    [Range(2, 10)]
    public int coinsMultiplayer = 3;
    public string gpAppUrl = "https://my_google_play_link";
    public string iosAppUrl = "https://my_app_store_link";
    public bool enableEducation = true;
    [Range(5, 100)]
    public int pixelateFirst = 15;
    [Range(5, 100)]
    public int pixelateSecond = 20;
    [Range(5, 100)]
    public int pixelateThird = 25;
    [Range(5, 100)]
    public int finalImage = 50;
    [Range(1, 10)]
    [Min(0)]
    public float animationSpeed = 4f;
    [Min(0)]
    public int pixelateCost;
    public Texture2D pen;
    [Min(0)]
    public int erasureCost = 5;
    [Range(0.05f, 0.5f)]
    public float penFrequnecy = 0.15f;
    [Range(1, 10)]
    public int huskFrequnecy = 3;
    [Range(2, 10)]
    public int gridSize = 5;
    [Range(2, 10)]
    public float aimSpeed = 6;
    public bool disableAiming = false;
    [Min(0)]
    public int plankCost = 20;
    [Min(0)]
    public int firstBet = 50;
    [Min(0)]
    public int secondBet = 60;
    [Min(0)]
    public int thirdBet = 70;
    [Min(0)]
    public int fourthBet = 80;
    public bool useBets = false;
    public bool deductCoinsWhenWrong = false;
    public Sprite backgroundIfWithoutImage;
    public bool enableLevelLocking = true;
    [Min(1)]
    public int unlockedAtStart = 4;
    [Min(1)]
    public int mustBeAvailableAtLeast = 2;
    public bool enableSubcategoryLocking = true;
    [Min(1)]
    public int unlockedSubCategoriesAtStart = 2;
    [Min(1)]
    public int subCategoriesMustBeAvailableAtLeast = 1;
    public bool isRatePopupNeeded = true;
    public int afterEeachLevel = 10;
    public bool isMoreGamesEnabled = false;
    public AnotherPublisherGame[] moreGamesItems;
    public bool useSimpleMenu = false;
    public ThemeColor[] colors;
    public Color[] levelColors;
    public bool unityAds = false;
    public bool unityIap = false;
    public bool adMob = false;
    public bool isBanner = false;
    public bool bannerOnTop = false;
    public int bannerEachLevel = 3;
    public bool nonpersonalizedAds = false;
    public bool googlePlaySaves = false;
    public bool instaLoginGpGames = false;
    public int suggestToLoginToGPGamesAfterLevel = 10;
    public bool GDPRconsent = false;
    public bool consentOnStart = true;
    public bool gdprButton = true;
    public string policyLink = "https://my_privacy_page";
    public InAppProduct[] inAppProducts;
    public bool disableAdsOnPurchase = false;
    public string unityAdsGooglePlay = "";
    public string unityAdsApple = "";
    public string unityAdsAndroidRewardedID = "Android_Rewarded";
    public string unityAdsAndroidInterstitialID = "Android_Interstitial";
    public string unityAdsAndroidBannerID = "Android_Banner";
    public string unityAdsIosRewardedID = "IOS_Rewarded";
    public string unityAdsIosInterstitialID = "IOS_Interstitial";
    public string unityAdsIosBannerID = "IOS_Banner";
    public int coinsAdReward = 100;
    public int showAdAfterLevel = 5;
    public string delayBetweenAds = "00:00:10";
    public bool sharing = false;
    public bool useRewardSettingsFromConsole = false;
    public string adMobAndroidRewardedID = "ca-app-pub-3940256099942544/5224354917";
    public string adMobAndroidInterstitialID = "ca-app-pub-3940256099942544/1033173712";
    public string adMobAndroidBannerID = "ca-app-pub-3940256099942544/6300978111";
    public string adMobIosRewardedID = "ca-app-pub-3940256099942544/1712485313";
    public string adMobIosInterstitialID = "ca-app-pub-3940256099942544/4411468910";
    public string adMobIosBannerID = "ca-app-pub-3940256099942544/2934735716";


    public override string ToString()
    {
        return JsonUtility.ToJson(this);
    }
}
