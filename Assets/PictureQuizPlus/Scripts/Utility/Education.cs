//using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum Direction
{
    up, down
}

public enum TextShift
{
    center, left, right
}

public class Education : MonoBehaviour //This component class is attached to the scene by Level Manager when education is enabled
{
    static bool showing;
    static bool timerSet;
    GameObject arrow, textField;
    LocalizedText text;
    Transform main;
    float timer;
    float timeToWait = 1;
    Dictionary<LocalizationItemType, EducationParameters> educationParams;


    private void Awake()
    {
        Initialize();
    }

    private void Initialize()
    {
        main = FindObjectOfType<LevelFrontendController>().main;
        arrow = Utils.CreateFromPrefab("EducationArrow");
        textField = Utils.CreateFromPrefab("EducationTextField");
        text = textField.GetComponentInChildren<LocalizedText>();

        //Add related parameters of the arrow and text box depending on education object position on the scene
        educationParams = new Dictionary<LocalizationItemType, EducationParameters>(){
        {LocalizationItemType.education_hints, new EducationParameters(0.7f, 2f, Direction.up, TextShift.right) },
        {LocalizationItemType.education_clear, new EducationParameters(0.7f, 1f, Direction.down, TextShift.center)},
        {LocalizationItemType.education_aim_button, new EducationParameters(0.7f, 1.7f, Direction.down, TextShift.right) },
        {LocalizationItemType.education_aim_image, new EducationParameters(1f, 1.7f, Direction.down, TextShift.center) },
        {LocalizationItemType.education_erasure, new EducationParameters(0.7f, 1f, Direction.down, TextShift.center) },
        {LocalizationItemType.education_task_description, new EducationParameters(0.5f, 2f, Direction.down, TextShift.left, 7) },
        {LocalizationItemType.education_pixelate_button, new EducationParameters(0.7f, 1.7f, Direction.down, TextShift.right) },
        {LocalizationItemType.education_pixelate_image, new EducationParameters(1f, 1.7f, Direction.down, TextShift.center) },
        {LocalizationItemType.education_bet, new EducationParameters(0.6f, 0.5f, Direction.down, TextShift.center, 0, 150) },
        {LocalizationItemType.education_chance_to_mistake, new EducationParameters(0.6f, 1f, Direction.up, TextShift.center, 10, 150) },
        };
    }

    private void Update() //Timer serves to prohibit new education tip to appear for a while
    {
        if (timerSet && timer < timeToWait)
        {
            timer += Time.deltaTime;
        }
        else
        {
            timerSet = false;
            timer = 0;
        }
    }

    //Spawn education arrow and text field with preffered parameters
    private IEnumerator SpawnEducation(LocalizationItemType type, Transform hintObjecttransform, EducationParameters param)
    {
        showing = true;
        yield return new WaitForSeconds(param.delay);
        Vector3 arrowOffset = new Vector3(0, param.direction == Direction.up ? -param.shift : +param.shift);
        Vector3 textOffset = new Vector3(param.textShift == TextShift.left ? -1f : param.textShift == TextShift.right ? +1f : 0,
                                            param.direction == Direction.up ? -0.9f - param.shift : 0.9f + param.shift);
        EducationPopup objScript = Instantiate(arrow, hintObjecttransform.position + arrowOffset,
                    param.direction == Direction.up ? Quaternion.identity : new Quaternion(0, 0, -180f, 0), main).GetComponent<EducationPopup>();
        text.key = type;
        objScript.textField = Instantiate(textField, hintObjecttransform.position + textOffset, Quaternion.identity, main);

        if (param.extend > 0)
        {
            Vector3 size = objScript.textField.GetComponent<RectTransform>().sizeDelta;
            objScript.textField.GetComponent<RectTransform>().sizeDelta = new Vector3(size.x, size.y + param.extend, size.z);
            Vector3 ap = objScript.textField.GetComponent<RectTransform>().anchoredPosition;
            Vector3 newPos = new Vector3(ap.x, ap.y + (param.direction == Direction.up ? -param.extend / 2 : param.extend / 2), ap.z);
            objScript.textField.GetComponent<RectTransform>().localPosition = objScript.textField.GetComponent<RectTransform>().anchoredPosition = newPos;

        }

        //overrideSorting serves to change UI order of the education elements in some popups like Win Popup
        if (param.overrideSorting != 0)
        {
            objScript.GetComponent<Canvas>().overrideSorting = true;
            objScript.textField.GetComponent<Canvas>().overrideSorting = true;
            objScript.GetComponent<Canvas>().sortingOrder = param.overrideSorting;
            objScript.textField.GetComponent<Canvas>().sortingOrder = param.overrideSorting;
        }
        PlayerPrefs.SetInt(type.ToString(), 1);

        //if (!(educationParams.Any(y => PlayerPrefs.GetInt(y.Key.ToString(), 0) == 0)))
        //{
        //    PlayerPrefs.SetInt("education_completed", 1);
        //}

    }

    //If all conditions (registry doesnt have the record, other tips are not showing now, timer after last tip is done) are met - spawn tip
    public void Try(LocalizationItemType type, Transform hintObjecttransform)
    {
        if (PlayerPrefs.GetInt(type.ToString(), 0) == 0 && !showing && timer == 0)
        {
            StartCoroutine(SpawnEducation(type, hintObjecttransform, educationParams[type]));
        }
    }

    public static void StartTimer()
    {
        showing = false;
        timerSet = true;
    }
}

public struct EducationParameters
{
    public EducationParameters(float shift, float delay, Direction direction, TextShift textShift, int overrideSorting, int extend = 0)
    {
        this.delay = delay;
        this.shift = shift;
        this.direction = direction;
        this.textShift = textShift;
        this.overrideSorting = overrideSorting;
        this.extend = extend;
    }
    public EducationParameters(float shift, float delay, Direction direction, TextShift textShift)
    {
        this.delay = delay;
        this.shift = shift;
        this.direction = direction;
        this.textShift = textShift;
        this.overrideSorting = 0;
        this.extend = 0;

    }
    public float shift, delay;
    public Direction direction;
    public TextShift textShift;
    public int overrideSorting, extend;
}
