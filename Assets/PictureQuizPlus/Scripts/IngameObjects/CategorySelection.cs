using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using UnityEngine.Events;

public class CategorySelection : MonoBehaviour //Component class for directorys popup
{
    private GameObject categorySelectorPrefab; //Directory prefab reference
    private GameObject galleryPrefab;
    private string[] openedDirs;
    private LevelInfo currentInfo; //Information about the directory from the saved data
    private IEnumerable<CategorySelector> categorySelectors;
    public Button back; //Reference to the Back button setted from the Inspector
    public Sprite defaultPicture; //Default directory image
    public Transform buttonsWrapper; //Default directory image
    public Text title;

    public static event UnityAction<bool> OnRootLevelReached;

    private void Awake()
    {
        categorySelectorPrefab = Utils.CreateFromPrefab("CategoryButton");
        galleryPrefab = Utils.CreateFromPrefab("Gallery");
    }

    public void OnEnable()
    {
        StartCoroutine(LoadCategories()); //Spawn directories buttons
    }

    //Create categories buttons
    private IEnumerator LoadCategories()
    {
        yield return new WaitUntil(() => GameController.Instance.IsDataReady); //Wait for the GameController is ready to share the data
        back.gameObject.SetActive(true);
        title.gameObject.SetActive(true);
        if (GameController.Instance.EnableSubcategories)
        {
            categorySelectors = GameController.Instance.SubCategories.Select(sc => new CategorySelector(sc));
        }
        else
        {
            categorySelectors = GameController.Instance.Categories.Select(c => new CategorySelector(c));
        }
        DrawSelectors(categorySelectors);

    }

    private void DrawSelectors(IEnumerable<CategorySelector> list, int level = 0)
    {
        foreach (Transform child in buttonsWrapper)
        {
            Destroy(child.gameObject);
        }

        CategorySelector[] arr = list.ToArray();

        Func<IEnumerable<CategorySelector>, int, List<CategorySelector>, List<CategorySelector>> getSelectors = null;

        getSelectors = (IEnumerable<CategorySelector> current, int currentLevel, List<CategorySelector> acc) =>
        {
            if (current == null || current.Count() < 1)
            {
                return acc;
            }
            foreach (CategorySelector categorySelector in current)
            {
                if (categorySelector.level == currentLevel)
                {
                    acc.Add(categorySelector);
                }
                else if (categorySelector.level < currentLevel)
                {
                    acc = getSelectors(categorySelector.childrenSelectors, currentLevel, acc);
                }
            }
            return acc;

        };

        List<CategorySelector> selectors = getSelectors(list, level, new List<CategorySelector>());

        foreach (CategorySelector categorySelector in selectors)
        {
            InstantiateSelectorButton(categorySelector);
        }
        back.onClick.RemoveAllListeners();
        back.onClick.AddListener(() => SoundsController.instance.PlaySound("blup"));
        if (level > 0)
        {
            //@todo will not work for deeper than 1 level
            back.onClick.AddListener(() => {
                title.text = GameController.Instance.GetLocalizedValue(LocalizationItemType.select_category);
                DrawSelectors(categorySelectors, level - 1);
            });
        }
        else
        {
            back.onClick.AddListener(() =>
            {
                OnRootLevelReached(true);
                back.gameObject.SetActive(false);
                title.gameObject.SetActive(false);
            });
        }
    }

    private void InstantiateSelectorButton(CategorySelector categorySelector)
    {
        ICategory category = categorySelector.category;

        GameObject go = Instantiate(categorySelectorPrefab, buttonsWrapper);
        go.transform.Find("Name/Text").GetComponent<Text>().text = category.LocalizedName;

        Sprite sprite =  GameController.Instance.ResourcesManager.Get<ContentIconResource, Sprite>(category).GetFirst();
        bool isCategory = category is Category;
        bool isSubCategory = category is SubCategory;
        if (isCategory)
        {
            Category cat = (Category)category;
            if (!cat.HasNoImage && cat.previousLevel.index != 0 && !cat.previousLevel.noImage)
            {
                if (cat.previousLevel.gameType == GameMode.FourImages)
                {
                    var sprites = GameController.Instance.ResourcesManager.Get<LevelImagesResource, Sprite>(cat.previousLevel).GetAll();
                    if (sprites != null)
                    {
                        if (sprites.Count() == 4)
                        {
                            sprite = Utils.CombineFourPictures(sprites.ToArray());
                        }
                        else
                        {
                            sprite = sprites.First();
                        }
                    }
                }
                else
                {
                    sprite = GameController.Instance.ResourcesManager.Get<LevelImagesResource, Sprite>(cat.previousLevel).GetFirst();
                }
            }
        }

        if (sprite == null)
        {
            sprite = defaultPicture;
        }

        go.transform.Find("Icon/Image").GetComponent<Image>().sprite = sprite;

        if (!category.IsUnlocked)
        {
            go.transform.Find("Bar/Progress/Text").GetComponent<Text>().text = GameController.Instance.GetLocalizedValue(LocalizationItemType.locked_text).ToUpper();
            SpriteColor color = go.GetComponent<SpriteColor>();
            color.OnColorApplied = () => go.transform.GetComponent<Image>().color -= new Color(0, 0, 0, 0.4f);
        }

        else
        {
            go.transform.Find("Bar/Progress/Text").GetComponent<Text>().text = category.CompletedLength + "\u002f" + category.Length;
            if (category is SubCategory)
            {
                go.transform.Find("Bar/Progress/Text/Folder").gameObject.SetActive(true);
            }
            RectTransform bar = go.transform.Find("Bar/ProgressBar").GetComponent<RectTransform>();
            float barPercent = (float)category.CompletedLength / (float)category.Length;
            bar.anchoredPosition += new Vector2((bar.sizeDelta.x * barPercent), 0);
        }

        go.GetComponent<Button>().onClick.AddListener(() =>
        {
            if (!category.IsUnlocked)
            {
                SoundsController.instance.PlaySound("wrong");
            }
            else if (categorySelector.childrenSelectors.Count() > 0)
            {
                SoundsController.instance.PlaySound("blup");
                DrawSelectors(categorySelector.childrenSelectors, categorySelector.level + 1);
                title.text = categorySelector.category.LocalizedName;
            }
            else if (isCategory)
            {
                Category cat = (Category)category;
                if (category.IsComplete)
                {
                    GameObject gallery = Instantiate(galleryPrefab, Utils.getRootTransform());
                    gallery.GetComponent<Gallery>().OnStart(cat);
                }
                else
                {
                    GameController.Instance.CurrentCategory = cat;
                    Utils.LoadScene(1);
                }
            }
        });
        go.SetActive(true);

    }

    private void OnDisable()
    {
        back.gameObject.SetActive(true);
        title.gameObject.SetActive(true);
    }
}

public class CategorySelector
{
    public CategorySelector(ICategory category, int level = 0)
    {
        this.category = category;
        this.level = level;
        this.childrenSelectors = this.category.Children.Select(child => new CategorySelector(child, level + 1));
    }
    public ICategory category;
    public int level;
    public IEnumerable<CategorySelector> childrenSelectors;
}