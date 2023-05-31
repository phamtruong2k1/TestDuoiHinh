using UnityEngine;
using UnityEngine.UI;

public class SimpleWinPopup : MonoBehaviour
{
    public Text answer;
    public Text cost;
    public Button continueButton;
    public Button description, doubleCoins;

    public GameObject actions, descriptionPopup;

    public bool isDescription, isMultiply;

    private void Awake()
    {
        if (LevelStateController.currentLevel.AnswerType != AnswerType.Variants)
        {
            SoundsController.instance.PlaySound("trill");
        }
        answer.text = LevelStateController.rightAnswer.ToUpper();
        if (GameController.Instance.UseBets && LevelStateController.currentLevel.AnswerType == AnswerType.Variants)
        {
            cost.text = (LevelStateController.completeLvlCoins == 0 ? "-" : "+") + LevelStateController.GetHintPrice(Hint.bet).ToString();
        }
        else
        {
            cost.text = (LevelStateController.completeLvlCoins <= 0 ? "" : "+") + LevelStateController.completeLvlCoins.ToString();
        }
        continueButton.onClick.AddListener(() =>
        {
            SoundsController.instance.StopAllSounds();
            Utils.OnLevelComplete();
        });
        isDescription = !(string.IsNullOrEmpty(LevelStateController.imageDescription));
        isMultiply = GameController.Instance.MultipleCoinsAdReward && LevelStateController.completeLvlCoins > 0;
        actions.SetActive(isDescription || isMultiply);
        if (isDescription)
        {
            description.gameObject.SetActive(true);
            description.onClick.AddListener(() =>
            {
                descriptionPopup.gameObject.SetActive(true);
                descriptionPopup.transform.GetComponentInChildren<Text>().text = LevelStateController.imageDescription;
                SoundsController.instance.PlaySound("blup");
                actions.SetActive(doubleCoins.gameObject.activeSelf);
                description.gameObject.SetActive(false);
            });
            LevelStateController.TryToEducate(LocalizationItemType.education_task_description, description.transform);
        }
        doubleCoins.gameObject.SetActive(isMultiply);
    }
}
