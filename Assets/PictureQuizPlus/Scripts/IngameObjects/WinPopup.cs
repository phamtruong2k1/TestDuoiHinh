using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;


public class WinPopup : MonoBehaviour //Component class for a win popup
{
    //UI elements references
    public Image image;
    public Sprite loseman, scaledCanvas;
    public Button continueButton, multiplyCoins, descriptionButton;
    public Text answer;
    public Text cost;
    public Transform mainTransform, wincanvas, descriptionPopup, noImageBack;
    public Transform fireworksCanvas;


    private GameObject firework;

    private void Awake()
    {
        firework = Utils.CreateFromPrefab("Firework");
    }

    void Start()
    {
        Initialize();

        //Spawn effects on start
        if (!GameController.Instance.DisableFireworks)
        {
            if (LevelStateController.completeLvlCoins <= 0)
            {
                StartCoroutine(Utils.SpawnResourcesObject("Prefabs/RainDrop", 1, "rain", mainTransform));
            }
            else StartCoroutine(SpawnFireWorks(1f, 0.3f));
        }
    }

    private void Initialize()
    {
        GameController.Instance.ads?.KillBanner();
        if (GameController.Instance.DisableCharacters) //Handle the guy in the win popup
        {
            try
            {
                transform.Find("WinMan").gameObject.SetActive(false);
            }
            catch (System.Exception)
            {
                Debug.LogWarning("Character not founded");
            }
        }
        else
        {
            if (LevelStateController.completeLvlCoins <= 0) //When ChoseAnAnswer type and player failed, change guy`s sprite
            {
                transform.Find("WinMan").GetComponent<Image>().sprite = loseman;
            }
        }
        bool shouldShowAdBtn = GameController.Instance.AnyAds && GameController.Instance.MultipleCoinsAdReward && LevelStateController.completeLvlCoins > 0;
        multiplyCoins.gameObject.SetActive(shouldShowAdBtn);
        if (shouldShowAdBtn)
        {
            GameController.Instance.ads.PrepareRewarded();
        }
        if (!string.IsNullOrEmpty(LevelStateController.imageDescription))
        {
            descriptionButton.gameObject.SetActive(true);
            descriptionButton.onClick.AddListener(() =>
            {
                descriptionPopup.gameObject.SetActive(true);
                descriptionPopup.transform.GetComponentInChildren<Text>().text = LevelStateController.imageDescription;
                SoundsController.instance.PlaySound("blup");
                descriptionButton.gameObject.SetActive(false);
            });
            LevelStateController.TryToEducate(LocalizationItemType.education_task_description, descriptionButton.transform);
        }

        if (LevelStateController.currentLevel.noImage) //Two diferent animations for the appearance of the popup
        {
            wincanvas.GetComponent<Image>().enabled = false;
            noImageBack.gameObject.SetActive(true);
            GetComponent<Animator>().SetBool("winPopup2", true);
        }
        else GetComponent<Animator>().SetBool("winPopup1", true);

        if (LevelStateController.currentLevel.gameType == GameMode.FourImages)
        {
            FourImagesManager script = Instantiate(Utils.CreateFromPrefab("FourPictures"), image.transform).GetComponent<FourImagesManager>();
            script.gameObject.GetComponent<GridLayoutGroup>().cellSize = new Vector2(160, 160);
            script.gameObject.GetComponent<GridLayoutGroup>().spacing = new Vector2(5, 5);
            var sprites = GameController.Instance.ResourcesManager.Get<LevelImagesResource, Sprite>(LevelStateController.currentLevel).GetAll();
            script.OnStart(sprites.ToArray());
        }
        else
        {
            image.sprite = GameController.Instance.ResourcesManager.Get<LevelImagesResource, Sprite>(LevelStateController.currentLevel).GetFirst();
        }
        //Set up the victory event data
        answer.text = LevelStateController.rightAnswer.ToUpper();
        if (GameController.Instance.UseBets && LevelStateController.currentLevel.AnswerType == AnswerType.Variants)
        {
            cost.text = (LevelStateController.completeLvlCoins == 0 ? "-" : "+") + LevelStateController.GetHintPrice(Hint.bet).ToString();
        }
        else
        {
            cost.text = (LevelStateController.completeLvlCoins <= 0 ? "" : "+") + LevelStateController.completeLvlCoins.ToString();
        }

        //Add anonymous method for Continue button
        continueButton.onClick.AddListener(() =>
        {
            SoundsController.instance.StopAllSounds();
            Utils.OnLevelComplete();
        });

        GameObject.FindGameObjectWithTag("coinText").transform.parent.GetComponent<Canvas>().sortingOrder = 1;

        SoundsController.instance.PlaySound("finish");
    }

    IEnumerator SpawnFireWorks(float delay, float shortDelay) //Method to spawn fireworks, obsolete
    {
        while (LevelStateController.isPaused)
        {
            yield return new WaitForSeconds(delay);
            foreach (Transform transform in fireworksCanvas)
            {
                Fire(transform);
                yield return new WaitForSeconds(shortDelay);
            }
        }
    }

    private void Fire(Transform parent) //Fireworks spawning with random colors
    {
        GameObject boom = Instantiate(firework, parent);
        boom.transform.localScale = new Vector3(0.5f, 0.5f, 0);

        Color newColor = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
        ParticleSystem[] particles = boom.GetComponentsInChildren<ParticleSystem>();

        //Paint over all particles with new random color
        for (int i = 0; i < particles.Length; i++)
        {
            var module = particles[i].main;
            module.startColor = newColor;
        }
        SoundsController.instance.PlaySound("firework");
        Destroy(boom, 2f); //Destroy the prefab after 2 seconds
    }
}
