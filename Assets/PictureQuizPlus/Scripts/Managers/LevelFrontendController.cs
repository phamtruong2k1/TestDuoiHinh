using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.IO;
using System.Linq;


//The main class that constructs new level graphics and UI
public class LevelFrontendController : MonoBehaviour
{
    //References to UI gameobjects
    public Image taskImage;
    public Text taskLevel;
    public Transform border, task;
    public Transform main;
    public Transform lettersBoard;
    public Transform lettersFields, lettersFields2Row;
    public Button letterDeleteButton;
    public Button letterClearButton;
    public Button menuButton;
    public Button hintsButton;
    public Button topCoinsButton;
    public Transform hintsPopup;
    public Transform menuPopup;
    public Transform winPopup, simpleWinPopup;
    public GameObject gameActions, inputActions, menuActions, coinsActions, topMenu;
    public Button gameAction, gameActionIcon, menuAction, hintAction, coinsAction;
    public Text actionPrice, balance;
    private GameObject letterPrefab;
    private GameObject letterFieldPrefab;
    //Collections with letters
    private List<Letter> letterList;
    private List<LetterField> letterFieldsList;
    private string currentAnswer;
    public static Color levelColor;

    //This events is for extensions reason. You can easily subscribe some new logic to them
    public static event UnityAction<Hint, object> OnHintUsed;
    public static event UnityAction OnLevelComplete;
    public static bool levelCompleted;

    public Sprite aimBtn, pixelateBtn;
    public GameObject textArea, fullTextArea;
    public static bool isAnswerEntered;
    private bool shouldFireWrongAnswer;

    public const int TASK_SHIFT = 25;

    private Animator imageAnimator;

    public bool IsImageReady => imageAnimator != null &&
        imageAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime > 1 &&
        !imageAnimator.IsInTransition(0);



    private void Awake()
    {
        if (GameController.Instance.UseSimpleMenu)
        {
            ActivateSimpleMenu();
        }
        letterPrefab = Utils.CreateFromPrefab("Letter");
        letterFieldPrefab = Utils.CreateFromPrefab("LetterField");
        levelCompleted = false;
        if (GameController.Instance.LevelColors.Length <= 0)
        {
            throw new System.Exception("LevelColors not specified");
        }
        imageAnimator = taskImage.transform.parent.GetComponent<Animator>();
        levelColor = GameController.Instance.LevelColors[Random.Range(0, GameController.Instance.LevelColors.Length)];
        SubscribeForEvents();

        Utils.SetCoinsText();
        Level currentLevel = GameController.Instance.CurrentLevel;
        taskLevel.text = currentLevel.index.ToString();
        if (GameController.Instance.DisablePictureMoveInWithHand || currentLevel.noImage)
        {
            if (
                currentLevel.gameType == GameMode.Pixel ||
                currentLevel.gameType == GameMode.Erasure ||
                currentLevel.gameType == GameMode.Planks
            )
            {
                imageAnimator.SetBool("withBorder", true);
            }
            else imageAnimator.SetBool("withBorder", true);
        }
        else
        {
            imageAnimator.SetBool("withHand", true);
        }


        StartCoroutine(SpawnTask());
    }

    private void Update()
    {
#if SHARING
        CheckForSharing();
#endif
        CheckForRightAnswer();
    }

    private void CheckForSharing() //Check is sharing required now
    {
        if (GameController.Instance.IsSharing)
        {
            LevelStateController.isPaused = true;
            GameController.Instance.IsSharing = false;
            StartCoroutine(Utils.Share());
        }
    }

    public void ActivateSimpleMenu(bool taskShift = true)
    {
        topMenu.SetActive(false);
        menuActions.SetActive(true);
        coinsActions.SetActive(true);
        Utils.SetCoinsText();
        if (taskShift)
        {
            task.localPosition += new Vector3(0, TASK_SHIFT);
        }
        coinsAction.onClick.RemoveAllListeners();
        coinsAction.onClick.AddListener(() =>
        {
            SoundsController.instance.PlaySound("menus");
            GameController.Instance.popup.Open<MonetizationPopup>(ThemeColorEnum.Decorative);
        });
        // balance.text = GameController.Instance.GetCoinsCount().ToString();
        hintAction.onClick.RemoveAllListeners();
        hintAction.onClick.AddListener(() =>
        {
            if (!LevelStateController.isPaused)
            {
                /*  if (FindObjectOfType<GameSceneCanvasBehavior>().shifted)
                 {
                     FindObjectOfType<GameSceneCanvasBehavior>().DefaultSize();

                 }
                 else
                 {
                     FindObjectOfType<GameSceneCanvasBehavior>().ReduceSizeForBanner();

                 } */
                hintsPopup.gameObject.SetActive(true);
            }
        });
        menuAction.onClick.RemoveAllListeners();
        menuAction.onClick.AddListener(() =>
        {
            if (!LevelStateController.isPaused)
            {
                menuPopup.gameObject.SetActive(true);
            }
        });
    }

    private void CheckForRightAnswer() //Check if the users input matches the right answer after last field filled
    {
        if (!LevelStateController.isPaused && GameController.Instance.CurrentLevel.AnswerType != AnswerType.Variants)
        {
            isAnswerEntered = !letterFieldsList.Select(l => l.text.text).Any(s => string.IsNullOrEmpty(s));
            if (isAnswerEntered)
            {
                foreach (var item in letterFieldsList)
                {
                    currentAnswer += item.text.text.ToLower();
                }
                if (currentAnswer == LevelStateController.rightAnswerNoSpaces)
                {
                    StartCoroutine(LevelCompleted(0));
                    foreach (var item in letterFieldsList)
                    {
                        item.OnRightAnswer();
                    }
                }
                else
                {
                    LevelStateController.TryToEducate(LocalizationItemType.education_hints, Object.FindObjectOfType<LevelFrontendController>().hintsButton.transform);
                    currentAnswer = string.Empty;
                    if (shouldFireWrongAnswer)
                    {
                        shouldFireWrongAnswer = false;
                        lettersFields.GetComponent<Animator>().Play("lettersFieldShake", -1, 0f);
                        foreach (var item in letterFieldsList)
                        {
                            item.OnWrongAnswer();
                        }
                    }
                }
            }
            else
            {
                if (!shouldFireWrongAnswer)
                {
                    shouldFireWrongAnswer = true;
                }
            }
        }

    }

    /* public IEnumerator WrongAnswer()
    {

        yield return new WaitUntil(() => {})
    } */
    //Method to show winpopup and fire level complete event to prepare new data
    public IEnumerator LevelCompleted(float seconds)
    {
        levelCompleted = true;
        OnLevelComplete();
        yield return new WaitForSeconds(seconds);
        yield return new WaitForEndOfFrame();
        if (GameController.Instance.WithoutWinPopup)
        {
            SoundsController.instance.StopAllSounds();
            Utils.OnLevelComplete();
        }
        else if (GameController.Instance.SimplifiedWinPopup)
        {
            simpleWinPopup.gameObject.SetActive(true);
        }
        else winPopup.gameObject.SetActive(true);
    }

    private void SubscribeForEvents() //Subscribe for events and add listeners to buttons
    {
        try
        {
            HintPopup.OnHintPopupClicked += AddButtonsListeneres;
            letterDeleteButton.onClick.AddListener(() => Utils.ClearField(letterFieldsList));
            letterClearButton.onClick.AddListener(() => Utils.ClearAll(letterFieldsList));
            hintsButton.onClick.AddListener(() =>
            {
                if (!LevelStateController.isPaused)
                {
                    hintsPopup.gameObject.SetActive(true);
                }
            });
            menuButton.onClick.AddListener(() =>
            {
                if (!LevelStateController.isPaused)
                {
                    menuPopup.gameObject.SetActive(true);
                }
            });
            topCoinsButton.onClick.AddListener(() =>
            {
                SoundsController.instance.PlaySound("menus");
                GameController.Instance.popup.Open<MonetizationPopup>(ThemeColorEnum.Decorative);
            });
            OnHintUsed += GameController.Instance.SpendCoins;
        }
        catch (System.NullReferenceException newEx)
        {
            Debug.LogError("There is no reference to " + newEx.Source);
        }
        catch (System.Exception)
        {
            Debug.LogError("You should start the game from 'StartMenu' scene. Or some references are broken");
        }
    }

    private void AddButtonsListeneres(HintPopup popup) //Subscribe hints handlers
    {
        if (HintPopup.needRestart == false)
        {
            if (GameController.Instance.CurrentLevel.AnswerType == AnswerType.Variants)
            {
                popup.buttons[0].onClick.AddListener(() => UseHint(Hint.one_option));
                popup.buttons[1].onClick.AddListener(() => UseHint(Hint.two_options));
                popup.buttons[2].onClick.AddListener(() => UseHint(Hint.chance));
            }
            else
            {
                popup.buttons[0].onClick.AddListener(() => UseHint(Hint.one_letter));
                popup.buttons[1].onClick.AddListener(() => UseHint(Hint.excess));
                popup.buttons[2].onClick.AddListener(() => UseHint(Hint.complete));
            }
        }
    }

    private IEnumerator SpawnTask() //Create a new level scene
    {
        yield return new WaitUntil(() => LevelStateController.IsLevelReady);
        Level currentLevel = LevelStateController.currentLevel;

        if (!string.IsNullOrEmpty(LevelStateController.twoLineRightAnswer))
        {
            lettersFields.gameObject.SetActive(false);
            lettersFields2Row.gameObject.SetActive(true);
            GridLayoutGroup grid = lettersFields2Row.GetComponent<GridLayoutGroup>();
            if (GameController.Instance.RightToLeftWords)
            {
                grid.startCorner = GridLayoutGroup.Corner.UpperRight;
            }
        }

        Category currentCategory = GameController.Instance.CurrentCategory;
        LevelInfo data = currentCategory.savedData;
        GameMode type = currentLevel.gameType;

        if (currentLevel.noImage)
        {
            taskImage.sprite = GameController.Instance.BackgroundIfWithoutImage ? GameController.Instance.BackgroundIfWithoutImage :
                Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));
        }
        else if (type != GameMode.FourImages)
        {
            Sprite sprite = GameController.Instance.ResourcesManager.Get<LevelImagesResource, Sprite>(currentLevel).GetFirst();

            taskImage.sprite = sprite;
        }

        fullTextArea.gameObject.SetActive(!string.IsNullOrEmpty(currentLevel.imageText) && currentLevel.noImage);
        fullTextArea.gameObject.GetComponentInChildren<Text>().text = currentLevel.imageText;
        textArea.gameObject.SetActive(!string.IsNullOrEmpty(currentLevel.imageText) && !currentLevel.noImage);
        textArea.gameObject.GetComponentInChildren<Text>().text = currentLevel.imageText;


        //Provide requested manager with data and start its logic
        InitializeGameType(data, type);

        if (currentLevel.AnswerType != AnswerType.Variants)
        {
            letterList = CreateLetterList(LevelStateController.fullList); //Instantiate letter prefabs and add their scripts to the collection
            letterFieldsList = CreateLetterFields(LevelStateController.rightAnswerList);//Instantiate letter fields and add their scripts to the collection
            StartCoroutine(SpawnLetters(letterList, data)); //Spawn letters 
        }
        else
        {
            inputActions.SetActive(false);
            ChoseAnAnswerManager answerbox = taskImage.gameObject.GetComponent<ChoseAnAnswerManager>();
            answerbox.Initialize(data);
        }
        SoundsController.instance.PlaySound("start");
    }

    private void InitializeGameType(LevelInfo data, GameMode type)
    {
        switch (type)
        {
            case GameMode.Default:
                break;
            case GameMode.Pixel:
                gameActions.SetActive(true);
                gameActionIcon.GetComponent<Image>().sprite = pixelateBtn;
                PixelManager pixel = taskImage.gameObject.GetComponent<PixelManager>();
                pixel.enabled = true;
                pixel.OnStart(data);
                break;

            case GameMode.Erasure:
                ErasureManager erasure = taskImage.gameObject.GetComponent<ErasureManager>();
                erasure.enabled = true;
                erasure.OnStart(data);
                break;

            case GameMode.Planks:
                PlanksManager planks = taskImage.gameObject.GetComponent<PlanksManager>();
                if (!GameController.Instance.DisableAiming)
                {
                    gameActions.SetActive(true);
                    gameActionIcon.GetComponent<Image>().sprite = aimBtn;
                }
                planks.OnStart(data);
                break;

            case GameMode.FourImages:
                LevelColor lc = taskImage.gameObject.AddComponent<LevelColor>();
                lc.applyTransparency = 0.6f;
                taskImage.GetComponent<GridLayoutGroup>().enabled = false;
                FourImagesManager script = Instantiate(Utils.CreateFromPrefab("FourPictures"), taskImage.transform).GetComponent<FourImagesManager>();
                var sprites = GameController.Instance.ResourcesManager.Get<LevelImagesResource, Sprite>(GameController.Instance.CurrentCategory.currentLevel).GetAll();
                script.OnStart(sprites.ToArray());
                break;

            default:
                break;
        }
    }

    public void ClearAll()
    {
        Utils.ClearAll(letterFieldsList);
    }

    private IEnumerator SpawnLetters(List<Letter> letterList, LevelInfo data)
    {
        foreach (var item in letterFieldsList)
        {
            item.gameObject.GetComponent<Animator>().Play("fade");
        }

        foreach (var item in letterList)
        {
            item.gameObject.GetComponent<Image>().enabled = true;
            item.gameObject.GetComponentInChildren<Text>().enabled = true;
            item.gameObject.GetComponent<Animation>().Play("FadeIn");
            yield return new WaitForSeconds(0.03f); //Wait a bit to make a dominoes effect for letters
        }

        yield return new WaitForSeconds(0.4f);
        //Handle if some hints already been used earlier
        if (data.lettersOppened != 0)
        {
            int i = data.lettersOppened;
            do
            {
                Utils.RevealLetter(letterFieldsList, letterList, LevelStateController.rightAnswerList);
                i--;
            } while (i != 0);
        }
        if (data.isLettersRemoved)
        {
            Utils.RemoveWrongLetters(letterFieldsList, letterList, LevelStateController.rightAnswerList);
        }

        LevelStateController.isPaused = false;
    }

    List<Letter> CreateLetterList(List<char> arr) //Instantiate letter prefabs and add their scripts to collection
    {
        List<Letter> letterList = new List<Letter>();
        foreach (char item in arr)
        {
            GameObject go = Instantiate(letterPrefab.gameObject, lettersBoard);
            string letter = item.ToString().ToUpper();
            Letter script = go.GetComponent<Letter>();
            script.textField.text = letter;
            script.Clicked += x => Utils.LetterClick(x, letterFieldsList);
            letterList.Add(script);
        }
        return letterList;
    }

    List<LetterField> CreateLetterFields(List<char> arr) //Instantiate letter fields and add their scripts to collection
    {
        List<LetterField> letterArr = new List<LetterField>();
        List<GameObject> full = new List<GameObject>();
        foreach (char item in arr)
        {
            GameObject go = Instantiate(letterFieldPrefab.gameObject,
                string.IsNullOrEmpty(LevelStateController.twoLineRightAnswer) ? lettersFields : lettersFields2Row);
            if (item != ' ' && item != '_')
            {
                LetterField _this = go.GetComponent<LetterField>();
                letterArr.Add(_this);
                Button temp = go.AddComponent<Button>();
                temp.transition = Selectable.Transition.None;
                temp.onClick.AddListener(() => Utils.Clear(_this));
            }
            else
            {
                go.GetComponent<Image>().enabled = false;
            }
            full.Add(go);
        }

        if (GameController.Instance.RightToLeftWords && arr.Count <= GameController.Instance.AddSecondLineAfterXLetters)
        {
            for (int i = 0; i < full.Count; i++)
            {
                full[i].transform.SetSiblingIndex(full.Count - i - 1);
            }

        }
        return letterArr;
    }

    //Hints handler
    private void UseHint(Hint hint)
    {
        if (Utils.EnoughCoinsForHint(hint))
        {
            object data = null;
            switch (hint)
            {
                case Hint.one_letter:
                    Utils.RevealOneLetter(letterFieldsList, letterList, LevelStateController.rightAnswerList);
                    break;
                case Hint.excess:
                    Utils.RemoveWrongLetters(letterFieldsList, letterList, LevelStateController.rightAnswerList);
                    break;
                case Hint.complete:
                    Utils.SolveTask(letterFieldsList, letterList, LevelStateController.rightAnswerList);
                    break;
                case Hint.one_option:
                    data = Utils.RemoveOneWrongAnswer(ChoseAnAnswerManager.wrongAnswers);
                    break;
                case Hint.two_options:
                    data = Utils.RemoveTwoWrongAnswers(ChoseAnAnswerManager.wrongAnswers);
                    break;
                case Hint.chance:
                    Utils.ChanseToMistake(ChoseAnAnswerManager.animators, true);
                    SoundsController.instance.PlaySound("delete");
                    break;
            }
            HintEvent(hint, data);
            hintsPopup.gameObject.SetActive(false);
        }

    }
    public static void HintEvent(Hint hint, object obj)
    {
        OnHintUsed(hint, obj);
    }

    private void OnDestroy()
    {
        OnHintUsed = null;
        OnLevelComplete = null;
    }
}
