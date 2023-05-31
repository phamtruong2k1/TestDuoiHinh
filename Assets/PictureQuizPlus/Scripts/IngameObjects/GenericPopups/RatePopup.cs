using UnityEngine;
using UnityEngine.UI;

public class RatePopup : MonoBehaviour, IGenericPopup
{
    public LocalizationItemType title => LocalizationItemType.empty;
    public Button later, never, rate;

    private void RateApp()
    {
        PlayerPrefs.SetInt("rate", 1);
        Application.OpenURL(GameController.Instance.AppUrl);
        GameController.Instance.popup.Close();
    }

    public void OnClose()
    {
    }

    public void OnOpen()
    {
        later.onClick.AddListener(() => GameController.Instance.popup.Close());
        never.onClick.AddListener(() => { PlayerPrefs.SetInt("rate", 1); GameController.Instance.popup.Close(); });
        rate.onClick.AddListener(RateApp);
    }
}