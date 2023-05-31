using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class MoreGamesPopup : MonoBehaviour, IGenericPopup
{
    public LocalizationItemType title => LocalizationItemType.more_games;
    public GameObject button;

    public void Awake()
    {
        button.SetActive(false);
    }
    public void OnClose()
    {
    }

    public void OnOpen()
    {
        foreach (AnotherPublisherGame game in GameController.Instance.MoreGamesItems)
        {
            GameObject temp = Instantiate(button, transform);
            temp.transform.Find("row/iconBack/icon").GetComponent<Image>().sprite = game.gamePicture;
            temp.transform.Find("row/nameBack/name").GetComponent<Text>().text = game.gameName;
            if (string.IsNullOrEmpty(game.gameDescription))
            {
                temp.transform.Find("descBack").gameObject.SetActive(false);
            }
            else
                temp.transform.Find("descBack/desc").GetComponent<Text>().text = game.gameDescription;
            temp.SetActive(true);
            temp.GetComponent<Button>().onClick.AddListener(() =>
            {
                Application.OpenURL(game.gameLink);
            });
        }
    }
}
