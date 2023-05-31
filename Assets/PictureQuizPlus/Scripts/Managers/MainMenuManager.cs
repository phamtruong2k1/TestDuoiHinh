using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

//Handle main menu interactions
public class MainMenuManager : MonoBehaviour
{
    public Transform mainTransform, categories, buttons, languagePopup;
    public Button play, about, exit, back, sounds, settings, debug, moregames; //Buttons references

    public void Awake()
    {
        StartCoroutine(Initialize());
        CategorySelection.OnRootLevelReached += (bool isReached) =>
        {
            if (categories == null) return;
            categories.gameObject.SetActive(false);
            buttons.gameObject.SetActive(true);
        };
        if (GameController.Instance.IsSettingsReady)
        {
            moregames.gameObject.SetActive(GameController.Instance.IsMoreGamesEnabled);
        }
    }

    private IEnumerator Initialize()
    {
        //Make sure that SoundsController is loaded and data is ready
        yield return new WaitUntil(() => GameController.Instance.IsSettingsReady);

        categories.gameObject.SetActive(false);

        if (GameController.Instance.DebugButton)
        {
            debug.gameObject.SetActive(true);
            debug.gameObject.GetComponent<DebugButton>().Subscribe(0);
        }
        if (!GameController.Instance.AboutButton)
        {
            about.gameObject.SetActive(false);
        }
        else
        {
            about.onClick.AddListener(() =>
        {
            SoundsController.instance.PlaySound("blup");
            // GameController.Instance.popup.Open<AboutPopup>(ThemeColorEnum.Primary);
            GameController.Instance.popup.Open<AboutPopup>(ThemeColorEnum.Primary);
        });
        }
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

        yield return new WaitUntil(() => SoundsController.instance != null && GameController.Instance.IsDataReady);

        bool langNotChosen = PlayerPrefs.GetInt("lang_chosen", 0) == 0;

        if (!SoundsController.instance.isMusic && !SoundsController.instance.isSounds)
        {
            sounds.GetComponent<Image>().color -= new Color(0, 0, 0, 0.5f);
        }
        //Add OnClick methods to all buttons
        play.onClick.AddListener(PlayHandler);
        exit.onClick.AddListener(() => Application.Quit());
        sounds.onClick.AddListener(() => Utils.ToggleSoundsAndMusic(sounds));
        moregames.onClick.AddListener(() =>
        {
            SoundsController.instance.PlaySound("blup");
            GameController.Instance.popup.Open<MoreGamesPopup>(ThemeColorEnum.Light);

        });
        if (GameController.Instance.IsCategoryCompleted)
        {
            GameController.Instance.IsCategoryCompleted = false;
            PlayHandler();
        }
        GameController.Instance.ads?.KillBanner();

        yield return new WaitUntil(() => GameController.Instance.LoadingScreenClosed);

        if (GameController.Instance.GDPRconsent && PlayerPrefs.GetInt("confirmed", 0) == 0 && GameController.Instance.ConsentOnStart)
        {
            GdprConfirmPopup gdprConfirmPopup = GameController.Instance.popup.Open<GdprConfirmPopup>(ThemeColorEnum.Positive);
            if (langNotChosen)
            {
                GameController.Instance.popup.EnqueuePopup<LanguagesPopup>(ThemeColorEnum.Normal);
            }
        }
        else
        {
            if (langNotChosen)
            {
                GameController.Instance.popup.Open<LanguagesPopup>(ThemeColorEnum.Normal);
            }
        }
#if GP_SAVES
        if (PlayerPrefs.GetInt("gpgames", 0) == 1 || GameController.Instance.InstaLoginGpGames)
        {
            GameController.Instance.gpSaves.Initialize();
        }
#endif  

    }

    void PlayHandler() //"Play button" logic
    {
        SoundsController.instance.PlaySound("blup");
        buttons.gameObject.SetActive(false);
        categories.gameObject.SetActive(true);
    }

    public void ClearSavedData() //Use it if you want to create clearing saves button in game
    {
        string path = System.IO.Path.Combine(Application.persistentDataPath, "saves.json");
        if (System.IO.File.Exists(path))
        {
            System.IO.File.Delete(path);
        }
    }
}
