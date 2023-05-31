using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GdprConfirmPopup : MonoBehaviour, IGenericPopup
{
    public LocalizationItemType title => LocalizationItemType.privacy_policy;
    public Button confirm, more;
    public void Awake()
    {

    }

    public void OnClose()
    {
      
    }

    public void OnOpen()
    {
        GameController.Instance.popup.CanBeClosed = PlayerPrefs.GetInt("confirmed", 0) > 0;
        confirm.onClick.AddListener(() =>
        {
            SoundsController.instance.PlaySound("menus");
            PlayerPrefs.SetInt("confirmed", 1);
            GameController.Instance.popup.CanBeClosed = true;
            if (GameController.Instance.AnyAds)
            {
                GdprAdsPopup popup = GameController.Instance.popup.OpenNextPage<GdprAdsPopup>();
                popup.closeOnClick = true;
            }
            else
            {
                GameController.Instance.popup.Close();
            }
        });
        more.onClick.AddListener(() =>
        {
            SoundsController.instance.PlaySound("menus");
            Application.OpenURL(GameController.Instance.PolicyLink);
        });
    }
}
