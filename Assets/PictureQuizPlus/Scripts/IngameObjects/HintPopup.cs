using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class HintPopup : MonoBehaviour //Component class for hints popup
{
    public Button[] buttons; //Array of buttons references
    public static event UnityAction<HintPopup> OnHintPopupClicked; //Event is fired when popup created
    public static bool needRestart = false;
    public Button share, monetization, watchAd;
    private void Awake()
    {
        if (LevelStateController.currentLevel.AnswerType == AnswerType.Variants) //Change hints buttons localized text keys when ChoseAnAnswer type
        {
            ChangeButtonText(buttons[0], LocalizationItemType.remove_one_option);
            ChangeButtonText(buttons[1], LocalizationItemType.remove_two_options);
            ChangeButtonText(buttons[2], LocalizationItemType.chanse_to_mistake);
        }

        if (GameController.Instance.Sharing)
        {
            share.gameObject.SetActive(true);
            share.onClick.AddListener(() =>
            {
                GameController.Instance.IsSharing = true;
                gameObject.SetActive(false);
            });
        }
        if (GameController.Instance.UnityIap && GameController.Instance.InAppProducts.Length > 0)
        {
            monetization.gameObject.SetActive(true);
            monetization.onClick.AddListener(() =>
            {
                GameController.Instance.popup.Open<MonetizationPopup>(ThemeColorEnum.Decorative);
            });
        }
        else if (GameController.Instance.AnyAds)
        {
            watchAd.gameObject.SetActive(true);
        }
    }

    public void ChangeButtonText(Button but, LocalizationItemType key)
    {
        but.transform.Find("Text").GetComponent<LocalizedText>().key = key;
    }

    private void OnEnable()
    {
        // GameController.Instance.ads.HideBanner();
        if (needRestart == true) //Restart all hint handlers when BET made
        {
            OnHintPopupClicked(this);
            needRestart = false;
        }

        if (GameController.Instance.DisableCharacters) //Disable hint guy
        {
            try
            {
                transform.Find("HintMan").gameObject.SetActive(false);

            }
            catch (System.Exception)
            {
                Debug.LogWarning("Character not founded");
            }
        }

        //Is 'Remove letters' button should be disabled
        if (LevelStateController.GetCurrentState().isLettersRemoved && buttons[1].interactable == true)
        {
            Utils.DisableButton(buttons[1]);
        }

        //When BET is not made yet disable all hint buttons
        if (LevelStateController.currentLevel.AnswerType == AnswerType.Variants && GameController.Instance.UseBets && LevelStateController.GetCurrentState().levelBet == 0)
        {
            AllDisable();
        }
        //Handle should be buttons disabled or enabled depending on different situations
        else if (LevelStateController.currentLevel.AnswerType == AnswerType.Variants && LevelStateController.GetCurrentState().levelBet != 0)
        {
            if (LevelStateController.GetCurrentState().chanseUsed == true)
            {
                AllDisable();
            }
            else
            {
                switch (LevelStateController.GetCurrentState().DisclosedAnswers.Count)
                {
                    case 0:
                        Utils.EnableButton(buttons[0]);
                        Utils.EnableButton(buttons[1]);
                        Utils.EnableButton(buttons[2]);
                        LevelStateController.TryToEducate(LocalizationItemType.education_chance_to_mistake, buttons[2].transform);
                        break;
                    case 1:
                        Utils.EnableButton(buttons[0]);
                        Utils.DisableButton(buttons[1]);
                        Utils.DisableButton(buttons[2]);
                        break;
                    case 2:
                        AllDisable();
                        break;
                    default:
                        AllDisable();
                        break;
                }
            }
        }
        LevelStateController.isPaused = true;
        SoundsController.instance.PlaySound("menus");
    }

    private void AllDisable()
    {
        Utils.DisableButton(buttons[0]);
        Utils.DisableButton(buttons[1]);
        Utils.DisableButton(buttons[2]);
    }

    private void OnDisable()
    {
        LevelStateController.isPaused = false;
    }

    void Start()
    {
        OnHintPopupClicked(this); //Event is fired when popup created
        GetComponent<Button>().onClick.AddListener(() => gameObject.SetActive(false)); //Disable popup wnen background image clicked

    }

    void Update()
    {
        if (Input.GetKeyDown("escape")) //Is Escape or Back pressed
        {
            gameObject.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        OnHintPopupClicked = null;
    }
}
