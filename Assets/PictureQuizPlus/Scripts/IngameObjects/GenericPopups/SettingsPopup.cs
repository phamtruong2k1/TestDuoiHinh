using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingsPopup : MonoBehaviour, IGenericPopup
{
    public LocalizationItemType title => LocalizationItemType.settings;

    public Button language, gdpr, googleplay, music, sounds;
    public event Action OnSoundsChange;

    public GameObject gpError;

    public void OnClose()
    {

    }

    public void OnOpen()
    {
        language.gameObject.SetActive(GameController.Instance.Localizations.Length > 1);
        gdpr.gameObject.SetActive(GameController.Instance.GDPRconsent);
        googleplay.gameObject.SetActive(GameController.Instance.GooglePlaySaves);
        if (GameController.Instance.GooglePlaySaves)
        {
            StartCoroutine("HandleGooglePlayBtn");
        }

        language.onClick.AddListener(() =>
        {
            SoundsController.instance.PlaySound("menus");
            GameController.Instance.popup.OpenNextPage<LanguagesPopup>();
        });

        gdpr.onClick.AddListener(() =>
        {
            SoundsController.instance.PlaySound("menus");
            GameController.Instance.popup.OpenNextPage<PrivacyPopup>();
        });

        StartCoroutine("HandleSounds");

    }


    private IEnumerator HandleGooglePlayBtn()
    {
        yield return new WaitUntil(() =>
       {
           return googleplay.GetComponent<SpriteColor>().Color != default;
       });
#if GP_SAVES
        GooglePlaySaves gpSaves = GameController.Instance.gpSaves;
        if (gpSaves != null)
        {
            googleplay.onClick.AddListener(() =>
            {
                gpError.SetActive(false);
                googleplay.GetComponent<Animation>().Play();
                PlayerPrefs.SetInt("gpgames", 1);
                gpSaves.Initialize();
            });
            if (gpSaves.Loading)
            {
                googleplay.GetComponent<Animation>().Play();
            }
            else gpError.SetActive(!gpSaves.Authenticated);
            gpSaves.OnAuth += (bool success) =>
            {
                if (googleplay == null) return;
                googleplay.GetComponent<Animation>().Stop();
                gpError.SetActive(!success);
            };
        }
#endif
    }

    private IEnumerator HandleSounds()
    {
        yield return new WaitUntil(() =>
        {
            return music.GetComponent<SpriteColor>().Color != default && sounds.GetComponent<SpriteColor>().Color != default;
        });
        if (!SoundsController.instance.isMusic)
        {
            music.GetComponent<Image>().color -= new Color(0, 0, 0, 0.5f);
        }
        if (!SoundsController.instance.isSounds)
        {
            sounds.GetComponent<Image>().color -= new Color(0, 0, 0, 0.5f);
        }
        music.onClick.AddListener(() =>
        {
            Utils.ToggleMusic(music);
            if (OnSoundsChange != null)
            {
                OnSoundsChange();

            }
        });
        sounds.onClick.AddListener(() =>
        {
            if (!SoundsController.instance.isSounds)
            {
                SoundsController.instance.PlaySound("blup");
            }
            Utils.ToggleSounds(sounds);
            if (OnSoundsChange != null)
            {
                OnSoundsChange();

            }
        });
    }
}
