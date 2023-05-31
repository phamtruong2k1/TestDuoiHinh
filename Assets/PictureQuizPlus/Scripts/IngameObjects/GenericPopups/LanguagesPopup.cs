using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LanguagesPopup : MonoBehaviour, IGenericPopup
{
    public LocalizationItemType title => LocalizationItemType.language;
    public GameObject language;
    public void Awake()
    {
        language.SetActive(false);
    }

    public void OnClose()
    {
        PlayerPrefs.SetInt("lang_chosen", 1);
    }

    public void OnOpen()
    {
        foreach (var item in GameController.Instance.Localizations)
        {
            GameObject temp = Instantiate(language, transform);
            temp.transform.Find("Text").GetComponent<Text>().text = item.name;
            temp.transform.Find("Image").GetComponent<Image>().sprite = GameController.Instance.ResourcesManager.Get<LocalizationIconResource, Sprite>(item).GetFirst();
            temp.SetActive(true);
            temp.GetComponent<Button>().onClick.AddListener(() =>
            {
                SoundsController.instance.PlaySound("blup");
                ChangeLanguage(item.filename);
            });
        }
    }

    void ChangeLanguage(string lang)
    {
        PlayerPrefs.SetInt("lang_chosen", 1);
        if (PlayerPrefs.GetString("language") == lang)
        {
            /* GameObject loadingCircle = Utils.CreateFromPrefab("LoadingCircle");
            GameObject loadingCircleInstantiated = Instantiate(loadingCircle, Utils.getRootTransform());
            yield return new WaitForSeconds(1f);
            Destroy(loadingCircleInstantiated); */
            GameController.Instance.popup.Close();
        }
        else
        {
            PlayerPrefs.SetString("language", lang);
            GameController.Instance.ResetHintsOnLanguageChange();
            GameController.Instance.Reload();
        }
    }
}
