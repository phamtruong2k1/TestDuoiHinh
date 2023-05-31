using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

//Stretches letters depending on the devices resolution ratio
public class GameSceneCanvasBehavior : MonoBehaviour
{
    const float REFERENCE_RATIO = 1.77f;
    private float letterBtnSizeX, letterFieldSizeX; //Width of the letter buttons
    public static Vector3 shift;

    public Transform topBar, task, lettersFields1Row, lettersFields2Row, letters, betBox, choseAnAnswerBox, hintButtons;

    public GridLayoutGroup lettersGroup;

    RectTransform betBoxrect;
    RectTransform choseBoxRect;

    float ratio, canvasWidth;

    bool init;

    RectTransform mainCanvasRect;
    public bool shifted = false;
    public const float LAYOUT_SHIT_FOR_BANNER = 35;
    LevelFrontendController frontendController;

    void Awake()
    {
        //canvasWidth = GameController.Instance.isThirdRowRequired ? canvasWidth + 650 : canvasWidth;
        ratio = Utils.GetScreenRatio();
        mainCanvasRect = Utils.getRootTransform().GetComponent<RectTransform>();
        /*  float diff = Mathf.Abs(REFERENCE_RATIO - ratio); */
        canvasWidth = mainCanvasRect.sizeDelta.x - 150;
        betBoxrect = betBox.GetComponent<RectTransform>();
        choseBoxRect = choseAnAnswerBox.GetComponent<RectTransform>();
        StartCoroutine("Init");
    }

    public IEnumerator Init()
    {
        yield return new WaitUntil(() => LevelStateController.IsLevelReady);
        frontendController = GameObject.FindObjectOfType<LevelFrontendController>();
        int cellCount = Mathf.Max(GameController.Instance.FullLettersCount, LevelStateController.rightAnswerNoSpaces.ToCharArray().Length);
        int rowsCounts = GameController.Instance.IsThirdRowRequired ? 3 : 2;
        int countPerRow = Mathf.CeilToInt((float)cellCount / (float)rowsCounts);
        letterBtnSizeX = canvasWidth / countPerRow;
        lettersGroup.cellSize = GameController.Instance.IsThirdRowRequired
            ? new Vector2(letterBtnSizeX, 80f)
            : new Vector2(letterBtnSizeX, 130f);
        lettersGroup.constraintCount = rowsCounts;

        if (!string.IsNullOrEmpty(LevelStateController.twoLineRightAnswer))
        {
            int letterPerRow = Mathf.CeilToInt(LevelStateController.twoLineRightAnswer.Length / 2f);
            letterFieldSizeX = canvasWidth / letterPerRow + (50 / letterPerRow);
            lettersFields2Row.GetComponent<GridLayoutGroup>().cellSize =
                new Vector2(Mathf.Min(60, letterFieldSizeX), lettersFields2Row.GetComponent<GridLayoutGroup>().cellSize.y);
        }

        if (ratio >= 1.9f)
        {
#if UNITY_IOS //IphoneX top UI elements shift down
            // IphoneXUIfix();
#endif
        }

        yield return new WaitForEndOfFrame();
        init = true;

    }

    private void IphoneXUIfix()
    {
        GameObject.FindGameObjectWithTag("top_elem").transform.localPosition -= new Vector3(0, 58);
        transform.parent.Find("Popups/HintsPopup/Buttons").localPosition -= new Vector3(0, 58);
        transform.parent.Find("Popups/WinPopup/WinCanvas").localPosition -= new Vector3(0, 58);
    }

    internal void ReduceSizeForBanner()
    {
        StartCoroutine("Shift");
    }

    public IEnumerator Shift()
    {
        if (shifted) yield return false;
        yield return new WaitUntil(() => init);
        if (!GameController.Instance.UseSimpleMenu)
        {
            frontendController.ActivateSimpleMenu(false);
        }
        lettersGroup.cellSize = GameController.Instance.IsThirdRowRequired ? new Vector2(letterBtnSizeX, 70f) : new Vector2(letterBtnSizeX, 100f);
        lettersGroup.childAlignment = TextAnchor.UpperCenter;
        betBoxrect.sizeDelta = new Vector2(betBoxrect.sizeDelta.x, betBoxrect.sizeDelta.y - 30);
        choseBoxRect.sizeDelta = new Vector2(choseBoxRect.sizeDelta.x, choseBoxRect.sizeDelta.y - 30);

        if (GameController.Instance.BannerOnTop)
        {
            foreach (var item in new Transform[] { betBox, choseAnAnswerBox, lettersFields1Row, lettersFields2Row, letters, hintButtons })
            {
                item.localPosition -= new Vector3(0, LAYOUT_SHIT_FOR_BANNER);
            }
            if (GameController.Instance.UseSimpleMenu)
            {
                task.localPosition -= new Vector3(0, LevelFrontendController.TASK_SHIFT + LAYOUT_SHIT_FOR_BANNER);
            }
            else
            {
                task.localPosition -= new Vector3(0, LAYOUT_SHIT_FOR_BANNER);
            }
        }
        else
        {
            foreach (var item in new Transform[] { betBox, choseAnAnswerBox, lettersFields1Row, lettersFields2Row, letters })
            {
                item.localPosition += new Vector3(0, LAYOUT_SHIT_FOR_BANNER);
            }
            if (GameController.Instance.UseSimpleMenu)
            {
                task.localPosition += new Vector3(0, LAYOUT_SHIT_FOR_BANNER - LevelFrontendController.TASK_SHIFT);
            }
            else
            {
                task.localPosition += new Vector3(0, LAYOUT_SHIT_FOR_BANNER);
            }
        }
        shifted = true;
    }

    internal void DefaultSize()
    {
        if (!shifted) return;
        lettersGroup.cellSize = GameController.Instance.IsThirdRowRequired ? new Vector2(letterBtnSizeX, 80f) : new Vector2(letterBtnSizeX, 130f);
        lettersGroup.childAlignment = TextAnchor.MiddleCenter;
        betBoxrect.sizeDelta = new Vector2(betBoxrect.sizeDelta.x, betBoxrect.sizeDelta.y + 30);
        choseBoxRect.sizeDelta = new Vector2(choseBoxRect.sizeDelta.x, choseBoxRect.sizeDelta.y + 30);

        if (GameController.Instance.BannerOnTop)
        {
            foreach (var item in new Transform[] { betBox, choseAnAnswerBox, lettersFields1Row, lettersFields2Row, letters, hintButtons })
            {
                item.localPosition += new Vector3(0, LAYOUT_SHIT_FOR_BANNER);
            }
            if (GameController.Instance.UseSimpleMenu)
            {
                task.localPosition += new Vector3(0, LevelFrontendController.TASK_SHIFT + LAYOUT_SHIT_FOR_BANNER);
            }
            else
            {
                task.localPosition += new Vector3(0, LAYOUT_SHIT_FOR_BANNER);
            }
        }
        else
        {
            foreach (var item in new Transform[] { betBox, choseAnAnswerBox, lettersFields1Row, lettersFields2Row, letters })
            {
                item.localPosition -= new Vector3(0, LAYOUT_SHIT_FOR_BANNER);
            }
            if (GameController.Instance.UseSimpleMenu)
            {
                task.localPosition -= new Vector3(0, LAYOUT_SHIT_FOR_BANNER - LevelFrontendController.TASK_SHIFT);
            }
            else
            {
                task.localPosition -= new Vector3(0, LAYOUT_SHIT_FOR_BANNER);
            }
        }
        shifted = false;
    }

    private void ChangeLocalPosition(string[] tags, Vector3 value)
    {
        foreach (var item in tags)
        {
            GameObject.FindGameObjectWithTag(item).transform.localPosition += value;
        }
    }
}

