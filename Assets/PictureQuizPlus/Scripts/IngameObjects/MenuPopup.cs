using UnityEngine;
using UnityEngine.UI;

public class MenuPopup : MonoBehaviour //Component class for menu popup
{
    //Buttons references
    public Button sounds, debug, settings, backToGame, backToMainMenu, exit;

    private void Start()
    {
        Initialize();
    }

    private void OnEnable()
    {
        if (GameController.Instance.DisableCharacters)
        {
            try
            {
                transform.Find("MenuMan").gameObject.SetActive(false);

            }
            catch (System.Exception)
            {
                Debug.LogWarning("Character not founded");
            }
        }

        LevelStateController.isPaused = true;
        SoundsController.instance.PlaySound("menus");
    }
    private void OnDisable()
    {
        LevelStateController.isPaused = false;
    }

    void Update()
    {
        if (Input.GetKeyDown("escape"))
        {
            gameObject.SetActive(false);
        }
    }

    private void Initialize()
    {
        if (GameController.Instance.DebugButton)
        {
            debug.gameObject.SetActive(true);
            debug.gameObject.GetComponent<DebugButton>().Subscribe(1);
        }
        settings.gameObject.SetActive(GameController.Instance.GDPRconsent || GameController.Instance.Localizations.Length > 1 || GameController.Instance.GooglePlaySaves);

        settings.onClick.AddListener(() =>
        {
            SoundsController.instance.PlaySound("blup");
            SettingsPopup popup = GameController.Instance.popup.Open<SettingsPopup>(ThemeColorEnum.Normal);
            popup.OnSoundsChange += () =>
            {
                if (!SoundsController.instance.isMusic && !SoundsController.instance.isSounds)
                {
                    float currentAlpha = sounds.GetComponent<Image>().color.a;
                    if (currentAlpha == 0.5f) return;
                    sounds.GetComponent<Image>().color -= new Color(0, 0, 0, 0.5f);
                }
                else
                {
                    float currentAlpha = sounds.GetComponent<Image>().color.a;
                    if (currentAlpha == 1f) return;
                    sounds.GetComponent<Image>().color += new Color(0, 0, 0, 0.5f);
                }
            };
        });

        if (!SoundsController.instance.isMusic && !SoundsController.instance.isSounds)
        {
            sounds.GetComponent<SpriteColor>().OnColorApplied += () =>
            {
                sounds.GetComponent<Image>().color -= new Color(0, 0, 0, 0.5f);
            };
        }

        //Attach OnClick methods to menu buttons
        sounds.onClick.AddListener(() => Utils.ToggleSoundsAndMusic(sounds));
        backToGame.onClick.AddListener(() => gameObject.SetActive(false));
        backToMainMenu.onClick.AddListener(() => Utils.LoadScene(0));
        exit.onClick.AddListener(() => Application.Quit());
    }
}
