using System;
using UnityEngine;
using UnityEngine.UI;

public class SavedProgressPopup : MonoBehaviour, IGenericPopup
{
    public LocalizationItemType title => LocalizationItemType.cloud_save;
    public Text coins, info;
    public Button yes, no;

    public void OnClose()
    {
    }

    public void OnOpen()
    {
        coins.text = GameController.Instance.savedProgress.coinsCount.ToString();
        foreach (var item in GameController.Instance.savedProgress.levelsInfo)
        {
            info.text += item.directoryName + " " + item.currentLevel + "\n";
        }

        no.onClick.AddListener(() => GameController.Instance.popup.Close());
        yes.onClick.AddListener(() => GameController.Instance.SaveFromGP());
    }
}
