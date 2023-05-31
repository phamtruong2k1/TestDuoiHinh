using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SuggetGPGamesPopup : MonoBehaviour, IGenericPopup
{
    public LocalizationItemType title => LocalizationItemType.cloud_save;
    public Button yes, no;

    public void Awake()
    {
    }

    public void OnClose()
    {

    }

    public void OnOpen()
    {
#if GP_SAVES
        yes.onClick.AddListener(() =>
        {
            PlayerPrefs.SetInt("gpgames", 1);
            GameController.Instance.gpSaves.Initialize();
            GameController.Instance.popup.Close();
        });
        no.onClick.AddListener(() =>
        {
            PlayerPrefs.SetInt("gpgames", -1);
            GameController.Instance.popup.Close();
        });
#endif  
    }

}
