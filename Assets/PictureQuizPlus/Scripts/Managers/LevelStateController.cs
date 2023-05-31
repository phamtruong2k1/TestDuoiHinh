using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Collections;


public class LevelStateController : MonoBehaviour //The data part of new level constructor
{
    public static string rightAnswer;
    public static string rightAnswerNoSpaces;
    public static string twoLineRightAnswer;
    public static string imageText;
    public static string imageDescription;
    public static int completeLvlCoins;
    public static bool isPaused = true;
    static public List<char> rightAnswerList; //Collection of right answer chars
    static public List<char> fullList; //Collection of all chars
    static Dictionary<Hint, int> hintsCost = new Dictionary<Hint, int>();
    static LevelInfo infoTosave; //Structure that holds current game state. It is ready to be saved in every moment
    public static event Action<LevelInfo> OnDataSave;
    public static Education educ;
    public static bool IsLevelReady { get; set; }
    public static Level currentLevel;
    public LevelFrontendController frontendController;


    private void Awake()
    {
        isPaused = true;
        IsLevelReady = false;
        HintPopup.OnHintPopupClicked += ProvideHintsCost; //Provide hints buttons with related cost
        frontendController = GameObject.FindObjectOfType<LevelFrontendController>();
        LevelFrontendController.OnHintUsed += SetLevelState; //New level state to save
        ChoseAnAnswerManager.mistakeHappend += SetLevelState;
        LevelFrontendController.OnLevelComplete += SetNewGameState; //Set up next level clear state
        Initialize();
    }

    void Initialize() //Levels data constructor
    {
        try
        {
            Category currentCategory = GameController.Instance.CurrentCategory;
            currentLevel = currentCategory.currentLevel;
            twoLineRightAnswer = string.Empty;
            infoTosave = currentCategory.savedData;
            rightAnswer = currentLevel.rightAnswer.ToLower();
            twoLineRightAnswer = GetSecondLine(rightAnswer);
            rightAnswerNoSpaces = rightAnswer.Replace(" ", string.Empty);
            if (GameController.Instance.RightToLeftWords)
            {
                rightAnswerNoSpaces = new string(rightAnswerNoSpaces.Reverse().ToArray());
            }
            imageText = currentLevel.imageText;
            imageDescription = currentLevel.imageDescription;
            completeLvlCoins = currentLevel.reward;
            hintsCost[Hint.complete] = GameController.Instance.SolveTaskCost;
            hintsCost[Hint.pixelate] = GameController.Instance.PixelateCost;
            hintsCost[Hint.plank] = GameController.Instance.PlankCost;
            hintsCost[Hint.erasure] = GameController.Instance.ErasureCost;
            //Calculations to balance hints cost depending on the words length
            if (GameController.Instance.UseBets && infoTosave.levelBet != 0)
            {
                PrepareSingleChoiceModeState(infoTosave.levelBet);
            }
            PrepareInputModeState();
            fullList = CreateListOfLetters(GameController.Instance.RandomLetters);
            Utils.Shuffle(fullList);

        }
        catch (Exception)
        {
            Debug.LogError("Start the game from 'StartMenu' scene. Or some references are broken");
        }
        if (GameController.Instance.EnableEducation)
        {
            educ = gameObject.AddComponent<Education>();
        }
        IsLevelReady = true;

        StartCoroutine(GameController.Instance.prepareNextLevelData());
        StartCoroutine(PrepareAds());

        bool shouldShowRatePopup = GameController.Instance.IsRatePopupNeeded && PlayerPrefs.GetInt("rate", 0) == 0 &&
           (currentLevel.index) % GameController.Instance.AfterEeachLevel == 0;
        bool shouldShowGpPopup = GameController.Instance.GooglePlaySaves && !GameController.Instance.InstaLoginGpGames && PlayerPrefs.GetInt("gpgames", 0) == 0 &&
                currentLevel.index % GameController.Instance.SuggestToLoginToGPGamesAfterLevel == 0;
        if (shouldShowRatePopup)
        {
            GameController.Instance.popup.Open<RatePopup>(ThemeColorEnum.Positive);
            if (shouldShowGpPopup)
            {
                GameController.Instance.popup.EnqueuePopup<SuggetGPGamesPopup>(ThemeColorEnum.Primary);
            }
        }
        else
        {
            if (shouldShowGpPopup)
            {
                GameController.Instance.popup.Open<SuggetGPGamesPopup>(ThemeColorEnum.Primary);
            }
        }
    }

    private IEnumerator PrepareAds()
    {
        yield return new WaitUntil(() => frontendController.IsImageReady);

        if (GameController.Instance.AnyAds && GameController.Instance.IsAdsAllowed())
        {
            GameController.Instance.ads.UpdateVendor();
            if (
                GameController.Instance.ShowAdAfterLevel > 0 &&
                ((currentLevel.index) % GameController.Instance.ShowAdAfterLevel) == 0
            )
            {
                GameController.Instance.shouldShowInterstitial = true;
                GameController.Instance.ads.PrepareInterstitial();
            }
            else
            {
                GameController.Instance.shouldShowInterstitial = false;
            }
            if (
                GameController.Instance.IsBanner &&
                GameController.Instance.BannerEachLevel > 0 &&
                ((currentLevel.index) % GameController.Instance.BannerEachLevel) == 0
            )
            {
                GameController.Instance.ads.PrepareBanner((isLoaded) =>
                {
                    if (isLoaded)
                    {
                        GameController.Instance.ads.ShowBanner((isShown) =>
                        {
                            if (isShown)
                            {
                                FindObjectOfType<GameSceneCanvasBehavior>().ReduceSizeForBanner();
                            }
                        }, () =>
                        {

                        });
                    }
                });
            }
            GameController.Instance.ads.PrepareRewarded();
        }
    }

    public static void TryToEducate(LocalizationItemType type, Transform objectTransform)
    {
        if (educ != null)
        {
            educ.Try(type, objectTransform);
        }
    }

    private string GetSecondLine(string rightAnswer)
    {
        string[] words = rightAnswer.Split(char.Parse(" "));
        int count = words.Length;
        string new1 = string.Empty;
        string new2 = string.Empty;
        if (count > 1 && rightAnswer.Count() > GameController.Instance.AddSecondLineAfterXLetters)
        {
            int count2 = 0;

            string overcounted = string.Empty;
            int overcount2 = 0;
            int overcount1 = 0;
            for (int index = count - 1; index >= 0; index--)
            {
                string temp2 = new2;
                new2 = string.IsNullOrEmpty(new2) ? new2.Insert(0, words[index]) : new2.Insert(0, words[index] + "_");
                if (new2.Length > GameController.Instance.AddSecondLineAfterXLetters)
                {
                    overcount2 = new2.Length;
                    overcounted = new2;
                    new2 = temp2;
                    break;
                }
                count2++;
            }
            if (count2 < count)
            {

                for (int index = 0; index < count - count2; index++)
                {
                    new1 += words[index];
                    if (index != (count - count2 - 1))
                    {
                        new1 += "_";
                    }
                }
                overcount1 = new1.Length;

                if (overcount1 <= GameController.Instance.AddSecondLineAfterXLetters || overcount1 < overcount2)
                {
                    if (new2.Length > new1.Length)
                    {
                        int diff = (new2.Length - new1.Length);
                        for (int i = 0; i < diff; i++)
                        {
                            if (i % 2 == 0)
                            {
                                new1 = new1.Insert(0, "_");
                            }
                            else
                            {
                                new1 += "_";
                            }
                        }
                    }
                    else
                    {
                        int diff = (new1.Length - new2.Length);
                        for (int i = 0; i < diff; i++)
                        {
                            if (i % 2 == 0)
                            {
                                new2 = new2.Insert(0, "_");
                            }
                            else
                            {
                                new2 += "_";
                            }
                        }
                    }
                }
                else
                {
                    new2 = overcounted;
                    new1 = string.Empty;
                    for (int index = 0; index < count - count2 - 1; index++)
                    {
                        new1 += words[index];
                        if (index != (count - count2 - 2))
                        {
                            new1 += "_";
                        }
                    }
                    int diff = (new2.Length - new1.Length);
                    for (int i = 0; i < diff; i++)
                    {
                        if (i % 2 == 0)
                        {
                            new1 = new1.Insert(0, "_");
                        }
                        else
                        {
                            new1 += "_";
                        }
                    }
                }
            }
            return new1 + new2;
        }

        return string.Empty;
    }

    void SetLevelState(Hint hint, object data) //New level state prepare to save when hint is used
    {
        switch (hint)
        {
            case Hint.one_letter:
                infoTosave.lettersOppened++;
                break;
            case Hint.excess:
                infoTosave.isLettersRemoved = true;
                break;
            case Hint.pixelate:
                infoTosave.openedPictures++;
                break;
            case Hint.plank:
                infoTosave.openedPlanks = (int[])data;
                break;
            case Hint.erasure:
                infoTosave.maskPath = (string)data;
                break;
            case Hint.one_option:
                infoTosave.DisclosedAnswers.Add((string)data);
                break;
            case Hint.two_options:
                infoTosave.DisclosedAnswers.AddRange((string[])data);
                break;
            case Hint.chance:
                infoTosave.chanseUsed = true;
                break;
            case Hint.bet:
                infoTosave.levelBet = hintsCost[Hint.bet];
                break;
        }
    }

    public static void PrepareInputModeState()
    {
        int letterCount = rightAnswerNoSpaces.ToCharArray().Length;
        if (hintsCost[Hint.complete] >= 100)
        {
            hintsCost[Hint.excess] = Mathf.Min(hintsCost[Hint.complete], Mathf.CeilToInt((hintsCost[Hint.complete] / 1.8f) / 10) * 10);
            hintsCost[Hint.one_letter] = Mathf.Min(hintsCost[Hint.complete], Mathf.CeilToInt((hintsCost[Hint.complete] * 2.5f / letterCount) / 10) * 10);
        }
        else
        {
            hintsCost[Hint.excess] = Mathf.Min(hintsCost[Hint.complete], Mathf.CeilToInt(hintsCost[Hint.complete] / 1.8f));
            hintsCost[Hint.one_letter] = Mathf.Min(hintsCost[Hint.complete], Mathf.CeilToInt(hintsCost[Hint.complete] * 2.5f / letterCount));
        }
    }

    //Calculations to balance hints cost depending on the BET
    public static void PrepareSingleChoiceModeState(int reward = 0)
    {
        hintsCost[Hint.bet] = reward;
        if (reward != 0)
        {
            if (GameController.Instance.UseBets)
            {
                completeLvlCoins = reward * 2;
            }
            else
            {
                //fallback
                infoTosave.levelBet = reward;
            }
        }
        else
        {
            reward = completeLvlCoins;
        }

        if (GameController.Instance.UseBets)
        {
            hintsCost[Hint.one_option] = Mathf.RoundToInt(reward / 2f);
            hintsCost[Hint.two_options] = Mathf.RoundToInt(Mathf.RoundToInt(reward / 2f) * 1.8f);
            hintsCost[Hint.chance] = Mathf.RoundToInt(Mathf.RoundToInt(reward / 2f) * 1.2f);
        }
        else
        {
            float coef = GameController.Instance.DeductCoinsWhenWrong ? 1.2f : 1;
            hintsCost[Hint.one_option] = Mathf.RoundToInt(reward * coef / 2.8f);
            hintsCost[Hint.two_options] = Mathf.RoundToInt(Mathf.RoundToInt(reward * coef / 2.8f) * 1.8f);
            hintsCost[Hint.chance] = Mathf.RoundToInt(Mathf.RoundToInt(reward * coef / 2.8f) * 1.2f);
        }


    }

    void ProvideHintsCost(HintPopup popup) //Provide hints buttons with related cost
    {
        for (int i = 0; i < 3; i++)
        {
            if (GameController.Instance.CurrentLevel.AnswerType == AnswerType.Variants && GameController.Instance.UseBets && infoTosave.levelBet == 0)
            {
                Utils.SetUpCost(popup.buttons[i].transform, 0);
            }
            else
            {
                if (GameController.Instance.CurrentLevel.AnswerType == AnswerType.Variants)
                {
                    Utils.SetUpCost(popup.buttons[i].transform, hintsCost[(Hint)(i + 6)]);
                }
                else Utils.SetUpCost(popup.buttons[i].transform, hintsCost[(Hint)i]);
            }
        }
    }

    static private void SetNewGameState() //Set up next level state
    {
        isPaused = true;
        if (completeLvlCoins <= 0)
        {
            if (infoTosave.failedLevels == default(List<FailedLevel>))
            {
                infoTosave.failedLevels = new List<FailedLevel>();
            }
            infoTosave.failedLevels.Add(new FailedLevel(infoTosave.currentLevel, completeLvlCoins));
        }
        infoTosave.currentLevel = infoTosave.currentLevel + 1;
        infoTosave.ResetHints();
        GameController.Instance.EarnCoins(completeLvlCoins, false); //Add coins for victory without sound
        SaveCurrentGameState();
    }

    public static int GetHintPrice(Hint hint)
    {
        return hintsCost[hint];
    }



    public static void ResetDirectory(LevelInfo data)
    {
        infoTosave = data;
    }

    public static void SaveCurrentGameState() //Event is fired when saving data is needed
    {
        OnDataSave(GetCurrentState());
        if (GameController.Instance.CurrentLevel.gameType == GameMode.Erasure)
        {
            ErasureManager.UnSub();
        }
    }

    public static LevelInfo GetCurrentState()
    {
        return infoTosave;
    }

    private void OnApplicationPause(bool pause)
    {
        SaveCurrentGameState();
    }

    private void OnApplicationQuit()
    {
        SaveCurrentGameState();
    }

    private void OnDestroy()
    {
        SaveCurrentGameState();
    }
    //Create the list of letters depending on GameController preferences
    private List<char> CreateListOfLetters(char[] rndList)
    {
        List<char> temp = new List<char>(rndList);

        char[] array = rightAnswerNoSpaces.ToCharArray();
        string answer = string.IsNullOrEmpty(twoLineRightAnswer) ? rightAnswer : twoLineRightAnswer;
        rightAnswerList = GameController.Instance.RightToLeftWords ?
            new List<char>(answer.ToCharArray().Reverse()) : new List<char>(answer.ToCharArray()); //Collection of right answer chars
        int diff = GameController.Instance.FullLettersCount - array.Length;
        int rndLettersCount = diff >= 0 ? diff : 0;
        char[] rndArray = new char[rndLettersCount];
        for (int i = 0; i < rndArray.Length; i++)
        {
            if (temp.Count < 1)
            {
                temp = new List<char>(rndList);
            }
            //Fill the collection with random chars
            int rndIndex = UnityEngine.Random.Range(0, temp.Count);
            rndArray[i] = temp[rndIndex];
            temp.RemoveAt(rndIndex);
        }
        return new List<char>(array.Concat(rndArray)); //Return concatination of collections
    }
}
