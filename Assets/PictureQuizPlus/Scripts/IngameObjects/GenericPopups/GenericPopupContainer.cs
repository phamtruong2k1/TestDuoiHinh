using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public class GenericPopupContainer : MonoBehaviour
{

    public GameObject wrapper, content, scrollView, border;
    public Text title, contentText;
    public Button closeButton, backButton;

    private ContentSizeFitter fitter;
    private RectTransform rect;
    private CanvasGroup canvasGroup;
    private ScrollRect scrollRect;
    private Animator animator;
    private Animation animationComp;

    private VerticalLayoutGroup scrollViewGroup;

    private bool isActivated = false;

    // Start is called before the first frame update

    public void SetTitle(string title)
    {
        this.title.enabled = !string.IsNullOrEmpty(title);
        this.title.text = title;
    }

    public GameObject SetContent(GameObject popupContent)
    {
        return Instantiate(popupContent, this.content.transform);
    }

    public void Hide() {
        animator.SetTrigger("hide");
    }

    public void Show() {
       StartCoroutine("AnimateOpen");
    }

    private IEnumerator AnimateOpen() {
        scrollRect.viewport.sizeDelta = new Vector2(scrollRect.viewport.sizeDelta.x, 699);
        scrollViewGroup.childControlHeight = true;
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        animator.SetTrigger("anotherPage");
    }



    void Awake()
    {
        animator = gameObject.GetComponent<Animator>();
        animationComp = gameObject.GetComponent<Animation>();
        fitter = wrapper.GetComponent<ContentSizeFitter>();
        rect = wrapper.GetComponent<RectTransform>();
        canvasGroup = wrapper.GetComponent<CanvasGroup>();
        scrollRect = scrollView.GetComponent<ScrollRect>();
        scrollViewGroup = scrollView.GetComponent<VerticalLayoutGroup>();
        canvasGroup.alpha = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (scrollRect.viewport.sizeDelta.y >= 820)
        {
            scrollViewGroup.childControlHeight = false;
            scrollRect.viewport.sizeDelta = new Vector2(scrollRect.viewport.sizeDelta.x, 820);
        }
        else if (!scrollViewGroup.childControlHeight)
        {
            scrollViewGroup.childControlHeight = true;
        }
        if (!isActivated)
        {
            canvasGroup.alpha = 1;
            isActivated = true;
        }
    }

    internal void SetColor(ThemeColorEnum color)
    {
        border.GetComponent<SpriteColor>().ChangeColor(color);
        closeButton.gameObject.GetComponent<SpriteColor>().ChangeColor(color);
        backButton.gameObject.GetComponent<SpriteColor>().ChangeColor(color);
    }
}
