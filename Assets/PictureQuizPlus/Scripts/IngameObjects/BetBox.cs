using UnityEngine;
using UnityEngine.UI;

public class BetBox : MonoBehaviour
{
    public Text[] buttons;
    public Button betBut;
    // Use this for initialization
    void Awake()
    {
        buttons[0].text = GameController.Instance.FirstBet.ToString();
        buttons[1].text = GameController.Instance.SecondBet.ToString();
        buttons[2].text = GameController.Instance.ThirdBet.ToString();
        buttons[3].text = GameController.Instance.FourthBet.ToString();
    }

    
}
