using UnityEngine;
using UnityEngine.UI;

public class MultiplyCoinsButton : MonoBehaviour

//This script is attached to the end level button to watch ad and multiply reward 
{
    public Text multiplayer;

    Button but;

    void Start()
    {
        multiplayer.text = GameController.Instance.CoinsMultiplayer + "X";
        but = GetComponent<Button>();
        but.onClick.AddListener(ShowAd);
    }

    public void ShowAd()
    {
        GameController.Instance.ads.ShowRewarded((status) =>
        {
            if (status.isWatched)
            {
                GameController.Instance.shouldShowInterstitial = false;
                int mulutiplier = GameController.Instance.CoinsMultiplayer - 1;
                int add = 0;
                int result = 0;
                if (GameController.Instance.UseBets && LevelStateController.currentLevel.AnswerType == AnswerType.Variants)
                {
                    int bet = LevelStateController.GetHintPrice(Hint.bet);
                    add = bet * mulutiplier;
                    result = bet + add;
                }
                else
                {
                    add = LevelStateController.completeLvlCoins * mulutiplier;
                    result = LevelStateController.completeLvlCoins + add;
                }
                if (GameController.Instance.SimplifiedWinPopup)
                {
                    SimpleWinPopup simpleWinPopup = FindObjectOfType<SimpleWinPopup>();
                    simpleWinPopup.cost.text = "+" + result.ToString();
                    simpleWinPopup.actions.SetActive(simpleWinPopup.description.gameObject.activeSelf);
                }
                else
                {
                    FindObjectOfType<WinPopup>().cost.text = "+" + result.ToString();
                }
                GameController.Instance.EarnCoins(add, true);
                gameObject.SetActive(false);
            }
            else
            {
                but.interactable = false;
                GameController.Instance.ads.PrepareRewarded((isPrepared) => but.interactable = isPrepared);
            }

        });
    }
}
