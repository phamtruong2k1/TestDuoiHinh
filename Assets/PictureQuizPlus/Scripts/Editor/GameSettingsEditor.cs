using System;
using System.Collections;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GameSettings))]
public class GameSettingsEditor : Editor //To override AdsIapSettings class instance view in the Inspector
{
    GameSettings targetInstance;
    GUIStyle sectionStyle;
    ThemeColor[] defaultTheme;
    Color[] defaultColors;
    bool isGpSavesOpened;
    public void OnEnable()
    {
        sectionStyle = new GUIStyle();
        sectionStyle.fontSize = 25;
        sectionStyle.fontStyle = FontStyle.Bold;
        sectionStyle.wordWrap = true;
        RectOffset margin = new RectOffset(0, 0, 10, 10);
        sectionStyle.margin = margin;
        sectionStyle.normal.textColor = EditorGUIUtility.isProSkin ? new Color(0.4320488f, 0.6711017f, 0.6886792f) : new Color(0.3411765f, 0.4588235f, 0.5647059f);
        defaultTheme = new ThemeColor[] {
                new ThemeColor(ThemeColorEnum.Primary, new Color(0.972549f,0.5882353f,0.1176471f)),
                new ThemeColor(ThemeColorEnum.DarkText, new Color(0.2509804f,0.2666667f,0.3254902f)),
                new ThemeColor(ThemeColorEnum.Neutral, new Color(0.4941176f,0.509804f,0.5294118f)),
                new ThemeColor(ThemeColorEnum.Secondary, new Color(0.9529412f,0.4470588f,0.172549f)),
                new ThemeColor(ThemeColorEnum.Light, new Color(0.4320488f,0.6711017f,0.6886792f)),
                new ThemeColor(ThemeColorEnum.Normal, new Color(0.3411765f,0.4588235f,0.5647059f)),
                new ThemeColor(ThemeColorEnum.Positive, new Color(0.5647059f,0.7450981f,0.427451f)),
                new ThemeColor(ThemeColorEnum.LightText, new Color(1,1,1)),
                new ThemeColor(ThemeColorEnum.Negative, new Color(0.97647f,0.254902f,0.266666f)),
                new ThemeColor(ThemeColorEnum.Decorative, new Color(0.2627451f,0.6666667f,0.5450981f)),
            };
        defaultColors = new Color[] {
            new Color(0.3411765f,0.4588235f,0.5647059f),
           /*  new Color(0.20784f,0.313725f,0.4392f),
            new Color(109/255f, 89/255f, 122/255f),
            new Color(181/255f, 101/255f, 118/255f),
            new Color(229/255f, 107/255f, 111/255f),
            new Color(234/255f, 172/255f, 139/255f), */
        };
    }

    public bool GetBool(string name)
    {
        return serializedObject.FindProperty(name).boolValue;
    }

    public void SetProperty(string name, bool value)
    {
        serializedObject.FindProperty(name).boolValue = value;
    }

    public SerializedProperty GetProperty(string name)
    {
        return serializedObject.FindProperty(name);
    }

    public bool IsUnityAdsEnabled
    {
        get { return GetBool("unityAds"); }
        set
        {
            bool b = GetBool("unityAds");
            if (b == value)
            {
                return;
            }
            SetProperty("unityAds", value);

            SetScriptingSymbol("UNITY_AD", BuildTargetGroup.Android, value);
            SetScriptingSymbol("UNITY_AD", BuildTargetGroup.iOS, value);
        }
    }

    public bool IsIAPenabled
    {
        get { return GetBool("unityIap"); }
        set
        {
            bool b = GetBool("unityIap");
            if (b == value)
            {
                return;
            }
            SetProperty("unityIap", value);

            SetScriptingSymbol("UNITY_IAP", BuildTargetGroup.Android, value);
            SetScriptingSymbol("UNITY_IAP", BuildTargetGroup.iOS, value);
        }
    }

    public bool IsSharingEnabled
    {
        get { return GetBool("sharing"); }
        set
        {
            bool b = GetBool("sharing");
            if (b == value)
            {
                return;
            }
            SetProperty("sharing", value);

            SetScriptingSymbol("SHARING", BuildTargetGroup.Android, value);
            SetScriptingSymbol("SHARING", BuildTargetGroup.iOS, value);
        }
    }

    public bool EnableAdMob
    {
        get { return GetBool("adMob"); }
        set
        {
            bool b = GetBool("adMob");
            if (b == value)
            {
                return;
            }
            SetProperty("adMob", value);

            SetScriptingSymbol("ENABLE_ADMOB", BuildTargetGroup.Android, value);
            SetScriptingSymbol("ENABLE_ADMOB", BuildTargetGroup.iOS, value);
        }
    }

    public bool EnableGDPRconsent
    {
        get { return GetBool("GDPRconsent"); }
        set
        {
            bool b = GetBool("GDPRconsent");
            if (b == value)
            {
                return;
            }
            SetProperty("GDPRconsent", value);

            SetScriptingSymbol("GDPR", BuildTargetGroup.Android, value);
            SetScriptingSymbol("GDPR", BuildTargetGroup.iOS, value);
        }
    }

    public bool EnableGPSaves
    {
        get { return GetBool("googlePlaySaves"); }
        set
        {
            bool b = GetBool("googlePlaySaves");
            if (b == value)
            {
                return;
            }
            SetProperty("googlePlaySaves", value);

            SetScriptingSymbol("GP_SAVES", BuildTargetGroup.Android, value);
            SetScriptingSymbol("GP_SAVES", BuildTargetGroup.iOS, value);
        }
    }

    void drawProp(string propName, string label, bool isExplisitLabel = true)
    {
        GUIStyle labelStyle = new GUIStyle() { wordWrap = true, padding = new RectOffset(0, 10, 0, 0), };
        if (EditorGUIUtility.isProSkin)
        {
            labelStyle.normal.textColor = new Color(0.8f, 0.8f, 0.8f);
        }
        SerializedProperty prop = GetProperty(propName);
        if (isExplisitLabel)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(label, labelStyle);
            GUILayout.FlexibleSpace();
            EditorGUILayout.PropertyField(prop, GUIContent.none, true);
            EditorGUILayout.EndHorizontal();
        }
        else
        {
            EditorGUILayout.PropertyField(prop, new GUIContent(label), true);
        }
        EditorGUILayout.Space();
    }

    void drawSectionLabel(string label)
    {
        GUIStyle style = new GUIStyle() { fontStyle = FontStyle.Bold, margin = new RectOffset(0, 0, 10, 5), wordWrap = true };
        if (EditorGUIUtility.isProSkin)
        {
            style.normal.textColor = Color.white;
        }
        EditorGUILayout.LabelField(label, style);
    }
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        targetInstance = (GameSettings)target;

#if !UNITY_IOS && !UNITY_ANDROID
			EditorGUILayout.Space();
			EditorGUILayout.Space();
			EditorGUILayout.Space();
			EditorGUILayout.Space();
			EditorGUILayout.Space();

			GUILayout.TextField("SWITCH THE PLATFORM TO IOS OR ANDROID IN THE BUILD SETTINGS");
			EditorGUILayout.Space();
			EditorGUILayout.Space();
			EditorGUILayout.Space();
			EditorGUILayout.Space();
			EditorGUILayout.Space();
			return;
#endif
        EditorGUILayout.LabelField("GENERAL", sectionStyle);
        targetInstance.isGeneralOpened = EditorGUILayout.Foldout(targetInstance.isGeneralOpened, "General Game Settings", true);
        EditorGUILayout.Space();

        if (targetInstance.isGeneralOpened)
        {
            if (targetInstance.isUIOpened)
            {
                targetInstance.isUIOpened = false;
            }
            if (targetInstance.isAdsIapOpened)
            {
                targetInstance.isAdsIapOpened = false;
            }
            drawProp("startCoins", "Initial coins supply");
            drawProp("defaultWinReward", "Default reward for a passed level. Is used when \"Reward\" field of level is empty.");
            drawProp("solveTaskCost", "Price for \"Get the answer\" hint. The price for another hints is automatically calculated depending on this value.");
            drawProp("gpAppUrl", "App URL in the Goole play store. Is used for social sharing and \"Rate the game\" popup.");
            drawProp("iosAppUrl", "App URL in the AppStore");

            drawSectionLabel("\"Pixelate\" mode");
            drawProp("pixelateFirst", "Pixel grid 1");
            drawProp("pixelateSecond", "Pixel grid 2");
            drawProp("pixelateThird", "Pixel grid 3");
            drawProp("finalImage", "Final grid");
            drawProp("animationSpeed", "Speed of animation");
            drawProp("pixelateCost", "Pixelation step price");


            drawSectionLabel("\"Erasure\" mode");
            drawProp("pen", "Brush texture");
            drawProp("huskFrequnecy", "Husk spawn frequency");
            drawProp("penFrequnecy", "Brush tick frequency");
            drawProp("erasureCost", "Erasure step price");

            drawSectionLabel("\"Planks\" mode");
            drawProp("gridSize", "Grid size");
            drawProp("aimSpeed", "Aim speed");
            drawProp("disableAiming", "Tap directly instead of aiming");
            drawProp("plankCost", "Removing plank price");

            drawSectionLabel("Single choice answer type");
            drawProp("useBets", "Make a bet before revealing options");
            drawProp("deductCoinsWhenWrong", "Deduct reward when the wrong answer is given. Ignored when using bets. Balance can turn negative. ");
            drawProp("firstBet", "1st Bet");
            drawProp("secondBet", "2nd Bet");
            drawProp("thirdBet", "3rd Bet");
            drawProp("fourthBet", "4th Bet");

            drawSectionLabel("Categories locking");
            drawProp("enableLevelLocking", "Enable category locking");
            drawProp("unlockedAtStart", "Initially unlocked");
            drawProp("mustBeAvailableAtLeast", "Must be available at least");

            drawSectionLabel("Subcategories");
            drawProp("enableSubcategories", "Enable Subcategories");
            drawProp("enableSubcategoryLocking", "Enable subcategory locking");
            drawProp("unlockedSubCategoriesAtStart", "Initially unlocked subcategories");
            drawProp("subCategoriesMustBeAvailableAtLeast", "Subcategories must be available at least");

            drawSectionLabel("Misc");
            drawProp("isRatePopupNeeded", "\"Rate the game\" popup");
            drawProp("afterEeachLevel", "Show \"Rate the game\" popup after each level");
            drawProp("isMoreGamesEnabled", "\"More games\" button in the main menu");
            drawProp("moreGamesItems", "\"More games\" elements", false);
        }

        EditorGUILayout.LabelField("UI", sectionStyle);
        targetInstance.isUIOpened = EditorGUILayout.Foldout(targetInstance.isUIOpened, "UI settings", true);
        EditorGUILayout.Space();

        if (targetInstance.isUIOpened)
        {
            if (targetInstance.isAdsIapOpened)
            {
                targetInstance.isAdsIapOpened = false;
            }
            if (targetInstance.isGeneralOpened)
            {
                targetInstance.isGeneralOpened = false;
            }
            drawProp("enableEducation", "In-game tips to educate players");
            drawProp("useSimpleMenu", "Use simplified top menu");
            drawProp("debugButton", "\"Reset the scene\" button. Use for debug. Turn it off for the production release!");
            drawProp("aboutButton", "\"About\" button");
            drawProp("fullLettersCount", "The finite amount of buttons for letters");
            drawProp("isThirdRowRequired", "Three rows of buttons for letters");
            drawProp("addSecondLineAfterXLetters", "The number of letters in the answer to expand the answer input to the second line");
            drawProp("rightToLeftWords", "Enable RTL");
            drawProp("disableCharacters", "Remove cartoon characters");
            drawProp("disableFireworks", "Disable victory fireworks");
            drawProp("disablePictureMoveInWithHand", "Disable animation of putting a picture on the stage with the hand");
            drawProp("simplifiedWinPopup", "Simplified popup after passing a level");
            drawProp("withoutWinPopup", "Skip victory screen completely");
            drawProp("backgroundIfWithoutImage", "Sprite to be used for background in the tasks without an image");
            drawProp("colors", "Theme colors", false);
            drawProp("levelColors", "Level colors", false);
        }



        EditorGUILayout.LabelField("ADS, IAP, GDPR", sectionStyle);
        targetInstance.isAdsIapOpened = EditorGUILayout.Foldout(targetInstance.isAdsIapOpened, "Advertisement and In App Purchase Settings", true);
        EditorGUILayout.Space();

        if (targetInstance.isAdsIapOpened)
        {
            if (targetInstance.isUIOpened)
            {
                targetInstance.isUIOpened = false;
            }
            if (targetInstance.isGeneralOpened)
            {
                targetInstance.isGeneralOpened = false;
            }
            if (GUILayout.Button("AdMob installation guide", GUILayout.Width(305), GUILayout.Height(30)))
            {
                Application.OpenURL("https://developers.google.com/admob/unity/start");
            }
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("For Google cloud progress saving:");
            if (GUILayout.Button("Download Google Play SDK", GUILayout.Width(305), GUILayout.Height(30)))
            {
                Application.OpenURL("https://github.com/playgameservices/play-games-plugin-for-unity/releases/");
            }
            if (GUILayout.Button("Follow the Configure Your Game section", GUILayout.Width(305), GUILayout.Height(30)))
            {
                Application.OpenURL("https://github.com/playgameservices/play-games-plugin-for-unity#configure-your-game");
            }
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Social sharing (for Unity 2020+ only):");
            if (GUILayout.Button("Install plugin", GUILayout.Width(305), GUILayout.Height(30)))
            {
                Application.OpenURL("https://github.com/yasirkula/UnityNativeShare#installation");
            }
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            GUIStyle style = new GUIStyle() { fontSize = 13, fontStyle = FontStyle.Bold, wordWrap = true };
            if (EditorGUIUtility.isProSkin)
            {
                style.normal.textColor = new Color(0.97647f, 0.554902f, 0.566666f);
            }
            else
            {
                style.normal.textColor = new Color(0.47647f, 0.154902f, 0.166666f);
            }
            //myStyle.alignment = TextAnchor.MiddleCenter;
            EditorGUILayout.LabelField("IMPORT REQUIRED PLUGINS BEFORE YOU ENABLE ANY OF THE OPTIONS BELOW", style);
            EditorGUILayout.LabelField("Applying new settings usually takes about 10 sec");
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            IsIAPenabled = EditorGUILayout.BeginToggleGroup(new GUIContent("Enable UnityIAP"), IsIAPenabled);
            EditorGUILayout.EndToggleGroup();

            EnableAdMob = EditorGUILayout.BeginToggleGroup(new GUIContent("Enable AdMob"), EnableAdMob);
            EditorGUILayout.EndToggleGroup();

            IsUnityAdsEnabled = EditorGUILayout.BeginToggleGroup(new GUIContent("Enable UnityAds"), IsUnityAdsEnabled);
            EditorGUILayout.EndToggleGroup();

            EnableGDPRconsent = EditorGUILayout.BeginToggleGroup(new GUIContent("Enable GDPR consent"), EnableGDPRconsent);
            EditorGUILayout.EndToggleGroup();

            EnableGPSaves = EditorGUILayout.BeginToggleGroup(new GUIContent("Enable GooglePlay Saves"), EnableGPSaves);
            EditorGUILayout.EndToggleGroup();

            IsSharingEnabled = EditorGUILayout.BeginToggleGroup(new GUIContent("Enable Social Sharing"), IsSharingEnabled);
            EditorGUILayout.EndToggleGroup();
            EditorGUILayout.Space();

#if SHARING
            // GetProperty("isSharingOptionsOpened").boolValue = EditorGUILayout.Foldout(GetProperty("isSharingOptionsOpened").boolValue, "Social Sharing Settings");
            /*  targetInstance.isSharingOptionsOpened = EditorGUILayout.Foldout(targetInstance.isSharingOptionsOpened, "Social Sharing Settings", true);
             EditorGUILayout.Space();

             if (targetInstance.isSharingOptionsOpened)
             {
                 drawProp("uniqueString", "Unique string for your android app");
                 if (GUILayout.Button("Integrate unique string", GUILayout.Width(180), GUILayout.Height(20)))
                 {
                     Utils.IntegrateStringToXML();
                 }
             } */
#endif

#if GP_SAVES
            isGpSavesOpened = EditorGUILayout.Foldout(isGpSavesOpened, "Cloud Saves", true);
            if (isGpSavesOpened)
            {
                drawProp("instaLoginGpGames", "Try to login to GooglePlayGames after first launch");
                drawProp("suggestToLoginToGPGamesAfterLevel", "Suggest to login to GooglePlayGames after each level");
            }
#endif

#if GDPR
            targetInstance.isGDPRSettingOpened = EditorGUILayout.Foldout(targetInstance.isGDPRSettingOpened, "GDPR Settings", true);
            EditorGUILayout.Space();

            if (targetInstance.isGDPRSettingOpened)
            {
                drawProp("consentOnStart", "Show GDPR popup at first launch");
                drawProp("policyLink", "Privacy Policy link");

            }
#endif

#if UNITY_IAP
            targetInstance.isUnityIapSettingOpened = EditorGUILayout.Foldout(targetInstance.isUnityIapSettingOpened, "Unity IAP", true);
            EditorGUILayout.Space();
            if (targetInstance.isUnityIapSettingOpened)
            {
                drawProp("disableAdsOnPurchase", "Disable interstital ADs and banner with any purchase");
                drawProp("inAppProducts", "In App Products", false);
                /*  drawProp("currencySign", "Currency sign");
                 drawProp("takePricesFromConsole", "Use prices from dev console (recommended)");
                 drawSectionLabel("1st product");
                 drawProp("ProductIDConsumable", "ID from store");
                 drawProp("coinsIAPReward", "Reward");
                 drawProp("price1", "Price");

                 drawSectionLabel("2nd product");
                 drawProp("ProductIDConsumable2", "ID from store");
                 drawProp("coinsIAPReward2", "Reward");
                 drawProp("price2", "Price");

                 drawSectionLabel("3rd product");
                 drawProp("ProductIDConsumable3", "ID from store");
                 drawProp("coinsIAPReward3", "Reward");
                 drawProp("price3", "Price"); */
            }
#endif
#if UNITY_AD || ENABLE_ADMOB
            targetInstance.isAdsSettingOpened = EditorGUILayout.Foldout(targetInstance.isAdsSettingOpened, "Ads Settings", true);
            EditorGUILayout.Space();
            if (targetInstance.isAdsSettingOpened)
            {
                drawProp("multipleCoinsAdReward", "\"Multiply reward by watching an AD\" button after passing a level");
                drawProp("coinsMultiplayer", "\"Multiply reward\" multiplier");
                drawProp("coinsAdReward", "Reward for watching an AD");
                drawProp("delayBetweenAds", "Delay between ADs (hh:mm:ss). Must be 5 sec at least to have time to load");
                drawProp("showAdAfterLevel", "Show interstitial AD after each X level");
                drawProp("isBanner", "Show banner");
                drawProp("bannerEachLevel", "Show bannner on each X level");
                drawProp("bannerOnTop", "Banner position on the top");
#if UNITY_AD
                targetInstance.isUnityAdsSettingOpened = EditorGUILayout.Foldout(targetInstance.isUnityAdsSettingOpened, "Unity Ads", true);
                EditorGUILayout.Space();
                if (targetInstance.isUnityAdsSettingOpened)
                {
                    drawSectionLabel("Android settings");
                    drawProp("unityAdsGooglePlay", "Google Play Store Game ID");
                    drawProp("unityAdsAndroidRewardedID", "Rewarded ID");
                    drawProp("unityAdsAndroidInterstitialID", "Interstitial ID");
                    drawProp("unityAdsAndroidBannerID", "Banner ID");
                    drawSectionLabel("IOS settings");
                    drawProp("unityAdsApple", "Apple App Store Game ID");
                    drawProp("unityAdsIosRewardedID", "Rewarded ID");
                    drawProp("unityAdsIosInterstitialID", "Interstitial ID");
                    drawProp("unityAdsIosBannerID", "Banner ID");
                }
#endif
#if ENABLE_ADMOB
                targetInstance.isUnityAdMobSettingOpened = EditorGUILayout.Foldout(targetInstance.isUnityAdMobSettingOpened, "AdMob", true);
                EditorGUILayout.Space();
                if (targetInstance.isUnityAdMobSettingOpened)
                {
                    drawProp("useRewardSettingsFromConsole", "Use reward amount from console AD unit");
                    drawSectionLabel("Android settings");
                    drawProp("adMobAndroidRewardedID", "Rewarded ID");
                    drawProp("adMobAndroidInterstitialID", "Interstitial ID");
                    drawProp("adMobAndroidBannerID", "Banner ID");
                    drawSectionLabel("IOS settings");
                    drawProp("adMobIosRewardedID", "Rewarded ID");
                    drawProp("adMobIosInterstitialID", "Interstitial ID");
                    drawProp("adMobIosBannerID", "Banner ID");
                }
#endif
            }
#endif
        }

        if (GUI.changed)
        {
            serializedObject.ApplyModifiedProperties();

            foreach (var item in defaultTheme)
            {
                if (targetInstance.colors == null || targetInstance.colors.Length < 1)
                {
                    targetInstance.colors = defaultTheme.ToArray();
                }
                else if (!targetInstance.colors.Any(c => c.key == item.key))
                {
                    targetInstance.colors = targetInstance.colors.Append(item).ToArray();
                }
            }
            EditorController.EditorGamesettings = targetInstance;
            foreach (SpriteColor component in FindObjectsOfType<SpriteColor>())
            {
                component.Color = targetInstance.colors.First(c => c.key == component.key).color;
                EditorUtility.SetDirty(component);
            }
            if (targetInstance.levelColors == null || targetInstance.levelColors.Length == 0)
            {
                targetInstance.levelColors = defaultColors;
            }
            MainMenuManager menu = FindObjectOfType<MainMenuManager>();
            if (menu != null)
            {
                menu.about?.gameObject.SetActive(targetInstance.aboutButton);
                menu.moregames?.gameObject.SetActive(targetInstance.isMoreGamesEnabled);
            }
            SceneView.RepaintAll();

        }
    }

    void SetScriptingSymbol(string symbol, BuildTargetGroup target, bool isActivate)
    {
        var s = PlayerSettings.GetScriptingDefineSymbolsForGroup(target);

        s = s.Replace(symbol + ";", "");

        s = s.Replace(symbol, "");

        if (isActivate)
            s = symbol + ";" + s;

        PlayerSettings.SetScriptingDefineSymbolsForGroup(target, s);
    }

}
