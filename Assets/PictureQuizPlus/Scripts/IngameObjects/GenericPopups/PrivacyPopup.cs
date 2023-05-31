using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if GDPR
using UnityEngine.Analytics;
#endif        
using UnityEngine.UI;

public class PrivacyPopup : MonoBehaviour, IGenericPopup
{
    public LocalizationItemType title => LocalizationItemType.privacy;

    public Button policy, myData, ads;

    private GameObject loadingCircleInstantiated;
    public void OnClose()
    {

    }

    public void OnOpen()
    {
        myData.GetComponent<Button>().onClick.AddListener(() =>
        {
            GameObject loadingCircle = Utils.CreateFromPrefab("LoadingCircle");
            loadingCircleInstantiated = Instantiate(loadingCircle, Utils.getRootTransform());
            SoundsController.instance.PlaySound("menus");
            OpenDataURL();
        });
        policy.onClick.AddListener(() =>
        {
            SoundsController.instance.PlaySound("menus");
            Application.OpenURL(GameController.Instance.PolicyLink);
        });
        ads.onClick.AddListener(() =>
        {
            SoundsController.instance.PlaySound("menus");
            GameController.Instance.popup.OpenNextPage<GdprAdsPopup>();
        });
    }

    void OnFailure(string reason)
    {
        Destroy(loadingCircleInstantiated);
        Debug.LogWarning(string.Format("Failed to get data privacy page URL: {0}", reason));
    }

    void OnURLReceived(string url)
    {
        Destroy(loadingCircleInstantiated);
        Application.OpenURL(url);
    }
    public void OpenDataURL()
    {
#if GDPR
        DataPrivacy.FetchPrivacyUrl(OnURLReceived, OnFailure);
#endif        
    }
}
