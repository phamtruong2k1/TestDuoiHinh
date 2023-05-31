using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Gallery : MonoBehaviour //Gallery component class
{
    //Ui elements references
    public Button back, arrowRight, arrowLeft;
    public Image image, textArea, fullTextArea;
    public Text directory, task, reward, number;

    public SpriteColor borderColor;
    private int pointer = 1;
    private string dirName;
    private List<FailedLevel> failedLevels;
    private Dictionary<int, Sprite[]> pool = new Dictionary<int, Sprite[]>();
    private ThemeColorEnum defaultBorderColor;
    private Category category;
    private bool isTextareaExpanded;

    void Awake()
    {
        gameObject.GetComponent<CanvasGroup>().alpha = 0;
    }

    public void OnStart(Category category)
    {
        this.category = category;
        defaultBorderColor = borderColor.key;
        StartCoroutine(PrepareResources());
    }

    private IEnumerator PrepareResources()
    {
        GameObject loadingCircle = Utils.CreateFromPrefab("LoadingCircle");
        GameObject loadingCircleInstantiated = Instantiate(loadingCircle, Utils.getRootTransform());
        foreach (Level level in category.Levels.Where(l => !l.noImage))
        {
            LoadingStep<LevelImagesResource> step = new LoadingStep<LevelImagesResource>()
            {
                CreateCoroutineWithData = () => GameController.Instance.ResourcesManager.GetAsync<LevelImagesResource, Sprite>(level),
            };
            step.Start();
            yield return new WaitUntil(() => step.IsReady || step.IsError);

            if (step.IsError)
            {
                break;
            }
            pool.Add(level.index, step.Payload.GetAll().ToArray());
        }

        Destroy(loadingCircleInstantiated);

        if (pool.Count == category.Levels.Where(l => !l.noImage).Count())
        {
            directory.text = category.localizedName.ToUpper();
            failedLevels = category.savedData.failedLevels;
            arrowRight.onClick.AddListener(OnArrowRight);
            arrowLeft.onClick.AddListener(OnArrowLeft);
            gameObject.SetActive(true);
            back.onClick.AddListener(() => Destroy(gameObject));
            SetValues();
            gameObject.GetComponent<CanvasGroup>().alpha = 1;
        }
        else
        {
            GameController.Instance.errorMessage = GameController.Instance.LoadingSettings.getFileNotFoundMessage(GameController.Instance.CurrentLocalization.filename);
            GameController.Instance.popup.Open<ErrorPopup>(
            new PopupSettings()
            {
                color = ThemeColorEnum.Negative,
                title = GameController.Instance.LoadingSettings.getErrorPopupTitle(GameController.Instance.CurrentLocalization.filename),
            });
            Destroy(gameObject);
        }

    }

    private void Update()
    {
        if (Input.GetKeyDown("escape"))
        {
            Destroy(gameObject);
        }
    }

    private void SetValues()
    {
        number.text = pointer.ToString();
        Level currentLevel = category.Levels.First(l => l.index == pointer);
        task.text = currentLevel.rightAnswer.ToUpper();
        if (failedLevels != null && failedLevels.Exists(level => level.orderNumber == pointer))
        {
            reward.text = failedLevels.Find(level => level.orderNumber == pointer).coins.ToString();
            borderColor.ChangeColor(ThemeColorEnum.Negative);
        }
        else
        {
            borderColor.ChangeColor(defaultBorderColor);
            reward.text = currentLevel.reward != 0
              ? "+" + currentLevel.reward.ToString().ToUpper()
              : "+" + GameController.Instance.DefaultWinReward.ToString();
        }
        if (currentLevel.noImage)
        {
            image.sprite = GameController.Instance.BackgroundIfWithoutImage ? GameController.Instance.BackgroundIfWithoutImage :
                Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));

        }
        else if (pool[pointer].Length == 4)
        {
            image.sprite = Utils.CombineFourPictures(pool[pointer]);
        }
        else
        {
            image.sprite = pool[pointer].First();
        }

        fullTextArea.gameObject.SetActive(!string.IsNullOrEmpty(currentLevel.imageText) && currentLevel.noImage);
        fullTextArea.gameObject.GetComponentInChildren<Text>().text = currentLevel.imageText;
        textArea.gameObject.SetActive(!string.IsNullOrEmpty(currentLevel.imageText) && !currentLevel.noImage);
        textArea.gameObject.GetComponentInChildren<Text>().text = currentLevel.imageText;
    }

    private void OnArrowRight()
    {
        if (pointer < category.Levels.Length)
        {
            ++pointer;
            SetValues();
            SoundsController.instance.PlaySound("blup");
        }
    }

    private void OnArrowLeft()
    {
        if (pointer > 1)
        {
            --pointer;
            SetValues();
            SoundsController.instance.PlaySound("blup");
        }
    }
}
