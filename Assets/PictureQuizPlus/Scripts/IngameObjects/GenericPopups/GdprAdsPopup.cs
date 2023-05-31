using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GdprAdsPopup : MonoBehaviour, IGenericPopup
{
    public LocalizationItemType title => LocalizationItemType.ads_settings;
    public Button yes, no;
    public bool closeOnClick = false;

    public void Awake()
    {
    }

    public void OnClose()
    {

    }

    public void OnOpen()
    {
        if (GameController.Instance.NonpersonalizedAds)
        {
            no.GetComponent<SpriteColor>().ChangeColor(ThemeColorEnum.Secondary);
        }
        else
        {
            yes.GetComponent<SpriteColor>().ChangeColor(ThemeColorEnum.Secondary);
        }
        yes.onClick.AddListener(() => {
            GameController.Instance.ads.SetNPA(false);
            yes.GetComponent<SpriteColor>().ChangeColor(ThemeColorEnum.Secondary);
            no.GetComponent<SpriteColor>().ChangeColor(ThemeColorEnum.Primary);
            if(closeOnClick) {
                GameController.Instance.popup.Close();
            }
            else {
                 GameController.Instance.popup.OpenPreviousPage();
            }
        });
        no.onClick.AddListener(() => {
            GameController.Instance.ads.SetNPA(true);
            no.GetComponent<SpriteColor>().ChangeColor(ThemeColorEnum.Secondary);
            yes.GetComponent<SpriteColor>().ChangeColor(ThemeColorEnum.Primary);
            if(closeOnClick) {
                GameController.Instance.popup.Close();
            }
            else {
                 GameController.Instance.popup.OpenPreviousPage();
            }
        });
    }
}
