using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PixelManager : MonoBehaviour //Class to manage task with Pixel Type
{
    //References from the Inspector
    public Sprite buttonSprite;
    public Material pixelMaterial;

    private int pixelateFirst;
    private int pixelateSecond;
    private int pixelateThird;
    private int finalImage;
    private float animationSpeed;
    private int openedPics;
    private int[] arrayOfValues;
    private Image taskImage;
    private Button action, actionIcon;

    LevelFrontendController frontendController;


    public void OnStart(LevelInfo data) //Initializing method called from the UIMnager at the level start
    {
        //Get values from the GameController
        pixelateFirst = GameController.Instance.PixelateFirst;
        pixelateSecond = GameController.Instance.PixelateSecond;
        pixelateThird = GameController.Instance.PixelateThird;
        finalImage = GameController.Instance.FinalImage;
        animationSpeed = GameController.Instance.AnimationSpeed / 100;
        frontendController = GameObject.FindObjectOfType<LevelFrontendController>();

        //Initialize level data
        openedPics = data.openedPictures;
        arrayOfValues = new int[4] { pixelateFirst, pixelateSecond, pixelateThird, finalImage };
        taskImage = GetComponent<Image>(); //Main task image
        taskImage.material = pixelMaterial;
        SetPixelGrid(arrayOfValues[openedPics]); //Set up first view
        action = frontendController.gameAction;
        actionIcon = frontendController.gameActionIcon;
        action.onClick.RemoveAllListeners();
        action.onClick.AddListener(() =>
        {
            if (
           GameController.Instance.UseBets &&
           LevelStateController.currentLevel.AnswerType == AnswerType.Variants &&
           !ChoseAnAnswerManager.optionsShowed ||
           !frontendController.IsImageReady
            ) return;
            StartCoroutine(Pixelate(openedPics));
        });
        frontendController.actionPrice.text = LevelStateController.GetHintPrice(Hint.pixelate).ToString();
        StartCoroutine(Init());

    }

    private void SetPixelGrid(int grid)
    {
        taskImage.material.SetFloat("_PixelCountU", grid);
        taskImage.material.SetFloat("_PixelCountV", grid);
    }

    private IEnumerator Init()
    {
        if (openedPics == arrayOfValues.Length - 1)
        {
            action.interactable = false; //Disable hint button if all attempts are already wasted
        }
        //be sure all animation complete
        yield return new WaitUntil(() => frontendController.IsImageReady);

        LevelStateController.TryToEducate(LocalizationItemType.education_pixelate_button, actionIcon.transform);
    }

    private IEnumerator Pixelate(int hintCase) //Try to use hint
    {

        if (Utils.EnoughCoinsForHint(Hint.pixelate))
        {
            SoundsController.instance.PlaySound("pixelate");

            float delay = animationSpeed / 0.2f / Mathf.Abs(arrayOfValues[hintCase] - arrayOfValues[hintCase + 1]);
            action.interactable = false;

            for (int i = 1; i < 10; i++) //Animation of decreasing pixel grid
            {
                yield return new WaitForSeconds(animationSpeed);
                SetPixelGrid(arrayOfValues[hintCase] - i);
            }

            for (int i = 1; i < Mathf.Abs(arrayOfValues[hintCase] - arrayOfValues[hintCase + 1]) + 10; i++) //Animation of encreasing pixel grid
            {
                yield return new WaitForSeconds(delay);
                SetPixelGrid(arrayOfValues[hintCase] - 9 + i);
            }

            action.interactable = hintCase == arrayOfValues.Length - 2 ? false : true; //Check is hint button should be disabled
            openedPics++;

            LevelFrontendController.HintEvent(Hint.pixelate, null);
        }
    }
}

