using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlanksManager : MonoBehaviour //Class to manage task with Planks Type
{
    //References from the Inspector
    public Button aimButton;
    public Sprite buttonSprite;

    private GameObject puffPrefab, plankPrefab, aimPrefab;
    private int gridSize;
    private bool disableAiming;
    private Image taskImage;
    private List<GameObject> planks = new List<GameObject>();
    private List<int> planksOppened = new List<int>();
    private Vector2 leftBounds;
    private Vector2 rightBounds;
    private GameObject aimGo;
    private bool isAiming;
    private Button action, actionIcon;
    private BoxCollider2D boundsBox;

    private float sideX, sideY;
    LevelFrontendController frontendController;

    public void OnStart(LevelInfo data)
    {
        puffPrefab = Utils.CreateFromPrefab("Puff");
        plankPrefab = Utils.CreateFromPrefab("Plank");
        aimPrefab = Utils.CreateFromPrefab("Aim");
        disableAiming = GameController.Instance.DisableAiming;
        gridSize = GameController.Instance.GridSize;
        frontendController = GameObject.FindObjectOfType<LevelFrontendController>();

        if (!disableAiming) //Configure hint button
        {
            action = frontendController.gameAction;
            actionIcon = frontendController.gameActionIcon;
            action.onClick.RemoveAllListeners();
            action.onClick.AddListener(HandleAimButton);
            frontendController.actionPrice.text = LevelStateController.GetHintPrice(Hint.plank).ToString();
        }
        //Initialize data
        taskImage = GetComponent<Image>();
        boundsBox = GetComponent<BoxCollider2D>();
        sideX = boundsBox.size.x; //Image size in a local space
        sideY = boundsBox.size.y;

        //Set up planks grid
        GridLayoutGroup grid = GetComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(sideX / gridSize, sideY / gridSize);

        for (int i = 0; i < Mathf.Pow(gridSize, 2); i++)
        {
            int x = i;

            //Instantiate, configure and add to collection each plank
            GameObject tempplank = Instantiate(plankPrefab, taskImage.transform);
            tempplank.AddComponent<BoxCollider2D>().size = grid.cellSize + new Vector2(5f, 5f);
            planks.Add(tempplank);
            Button tempBut = planks[x].GetComponent<Button>();
            tempBut.onClick.AddListener(() => HitPlank(x));

            if (!disableAiming)
            {
                tempplank.GetComponent<Image>().raycastTarget = false;
                tempBut.interactable = false;
            }
        }

        if (data.openedPlanks != null) //Set up planks that are already opened earlier
        {
            foreach (var item in data.openedPlanks)
            {
                planks[item].GetComponent<Image>().enabled = false;
            }
            planksOppened.AddRange(data.openedPlanks);
        }
        StartCoroutine(Init());
    }

    private IEnumerator Init()
    {
        //be sure all animation complete
        yield return new WaitUntil(() => frontendController.IsImageReady);
        leftBounds = boundsBox.transform.TransformPoint(new Vector3(-sideX / 2, -sideY / 2));//Left and bottom sides of the image square
        rightBounds = boundsBox.transform.TransformPoint(new Vector3(sideX / 2, sideY / 2));//Right and upper sides of the image square
        leftBounds += new Vector2(0.05f, 0.05f);
        rightBounds -= new Vector2(0.05f, 0.05f);
        if (!disableAiming)
        {
            LevelStateController.TryToEducate(LocalizationItemType.education_aim_button, actionIcon.transform);
        }
    }

    private void HitPlank(int number) //When the plank is clicked or shooted
    {
        if (Utils.EnoughCoinsForHint(Hint.plank))
        {
            GameObject currentPlank = planks[number];
            if (currentPlank.GetComponent<Image>().enabled)
            {
                currentPlank.GetComponent<Image>().enabled = false;
                planksOppened.Add(number);
                GameObject puff = Instantiate(puffPrefab, currentPlank.transform.position, Quaternion.identity, transform);
                var main = puff.GetComponent<ParticleSystem>().main;
                main.startColor = LevelFrontendController.levelColor;
                Destroy(puff, 2); //Particles
                SoundsController.instance.PlaySound("shot");
            }
            else
            {
                SoundsController.instance.PlaySound("miss");
            }
            LevelFrontendController.HintEvent(Hint.plank, planksOppened.ToArray());
        }
    }

    private void Update()
    {
        if (isAiming && Input.GetKeyDown("escape"))
        {
            StopAiming();
        }
    }

    private void HandleAimButton() //Listener for hint button
    {
        if (
            GameController.Instance.UseBets &&
            LevelStateController.currentLevel.AnswerType == AnswerType.Variants &&
            !ChoseAnAnswerManager.optionsShowed ||
            !frontendController.IsImageReady
        ) return;

        if (!isAiming)
        {
            StartAiming();
        }
        else
        {
            Shoot();
        }
    }

    private void StartAiming()
    {
        SoundsController.instance.PlaySound("startAim");
        isAiming = true;
        Vector3 randomStartPoint = new Vector3(Random.Range(leftBounds.x, rightBounds.x), leftBounds.y, 0);
        aimGo = Instantiate(aimPrefab, gameObject.transform.parent);
        aimGo.transform.position = randomStartPoint;
        aimGo.GetComponent<Aim>().OnStart(leftBounds, rightBounds);
        actionIcon.GetComponent<Animator>().SetBool("isLooping", true);
    }

    private void Shoot()
    {
        isAiming = false;
        Collider2D colliderOnShot = Physics2D.OverlapPoint(aimGo.transform.position, 1);
        colliderOnShot.gameObject.GetComponent<Button>().onClick.Invoke();
        Destroy(aimGo);
        actionIcon.GetComponent<Animator>().SetBool("isLooping", false);
    }

    private void StopAiming()
    {
        isAiming = false;
        Destroy(aimGo);
        actionIcon.GetComponent<Animator>().SetBool("isLooping", false);
    }
}
