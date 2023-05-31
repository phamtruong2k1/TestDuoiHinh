using UnityEngine;
using UnityEngine.UI;

public class ErrorPopup : MonoBehaviour, IGenericPopup
{
    public LocalizationItemType title => LocalizationItemType.unable_to_continue;
    public Button reload, quit;
    public Text message;

    public void OnClose()
    {
    }

    public void OnOpen()
    {
        message.text = GameController.Instance.errorMessage;
        reload.transform.Find("Text").GetComponent<Text>().text = GameController.Instance.LoadingSettings.getErrorPopupReload(GameController.Instance.CurrentLocalization.filename);
        quit.transform.Find("Text").GetComponent<Text>().text = GameController.Instance.LoadingSettings.getErrorPopupQuit(GameController.Instance.CurrentLocalization.filename);
        reload.onClick.AddListener(() => GameController.Instance.Reload());
        quit.onClick.AddListener(() =>
        {
#if UNITY_EDITOR
            GameController.Instance.Reload();
#else
            Application.Quit();
#endif
        });
    }
}