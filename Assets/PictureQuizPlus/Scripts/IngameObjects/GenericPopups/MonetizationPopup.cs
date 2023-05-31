using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MonetizationPopup : MonoBehaviour, IGenericPopup
{
    public LocalizationItemType title => LocalizationItemType.get_coins;

    public Button watchAd;

    public GameObject iapBtn;

    private IAPController iapInstance;

    public void OnClose()
    {

    }

    public void OnOpen()
    {
        iapInstance = FindObjectOfType<IAPController>();

        if (GameController.Instance.AnyAds)
        {
            watchAd.gameObject.SetActive(true);
        }

        if (iapInstance != null)
        {
            foreach (var product in GameController.Instance.InAppProducts)
            {
                GameObject btn = Instantiate(iapBtn, transform);
                if (!string.IsNullOrEmpty(product.localPrice))
                {
                    btn.transform.Find("description/buy").GetComponent<Text>().text = GameController.Instance.GetLocalizedValue(LocalizationItemType.buy_for) + " " + product.localPrice;
                }
                else
                {
                    btn.transform.Find("description/buy").GetComponent<Text>().text = GameController.Instance.GetLocalizedValue(LocalizationItemType.buy);
                }
                string description = Utils.RenderMustache(product.buttonDescription);
                btn.transform.Find("description").GetComponent<Text>().text = description;
                btn.transform.Find("icon").GetComponent<Image>().sprite = product.icon;
                btn.SetActive(true);
                btn.GetComponent<Button>().onClick.AddListener(() =>
                {
#if UNITY_IAP
                    iapInstance.BuyProductID(product.productId);
#endif
                });
            }
        }
    }
}
