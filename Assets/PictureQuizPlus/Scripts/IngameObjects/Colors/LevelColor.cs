using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class LevelColor : MonoBehaviour
{
    public float applyTransparency = -1;
    public float applyLight = -1;
    [HideInInspector]
    public ThemeColorEnum transitionTarget;
    [HideInInspector]
    public float transitionProgress = 0;
    [HideInInspector]
    public Image Image;

    private Color memoColor;
    private void Start()
    {
        Image = GetComponent<Image>();
        if (applyTransparency >= 0)
        {
            memoColor = Image.color = new Color(LevelFrontendController.levelColor.r, LevelFrontendController.levelColor.g, LevelFrontendController.levelColor.b, applyTransparency);
        }
        else
        {
            memoColor = Image.color = LevelFrontendController.levelColor;

        }
        if (applyLight > 0)
        {
            memoColor = Image.color += new Color(applyLight, applyLight, applyLight, 0);
        }
    }


    public void Update()
    {
        if (transitionProgress > 0)
        {
            if (transitionTarget >= 0)
            {
                try
                {
                    Color transitionColor = GameController.Instance.GetColor(transitionTarget);
                    if (applyTransparency >= 0)
                    {
                        transitionColor.a = applyTransparency;
                    }
                    if (Image.color != transitionColor)
                    {
                        Image.color = Color.Lerp(Image.color, transitionColor, transitionProgress);
                    }
                }
                catch (System.Exception)
                {
                    Debug.LogWarning($"There is no such theme color: {nameof(transitionTarget)}");
                }
            }
            else
            {
                if (Image.color != memoColor)
                {
                    Image.color = Color.Lerp(Image.color, memoColor, transitionProgress);
                }
            }
        }

    }
}
