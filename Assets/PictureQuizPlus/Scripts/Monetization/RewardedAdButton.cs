using System;
using UnityEngine;
using UnityEngine.UI;

public class RewardedAdButton : MonoBehaviour
{
    public Text timeLabel, textLabel; //Text component of countdown
    public GameObject icon, noConnection, timer;
    public bool isFullLabel;
    private Button adButton;
    private string timeBtwnADs; //Time between ADs

    //Variables that are needed for logic
    private bool isADseen;
    private double tcounter;
    private DateTime currentTime;
    private TimeSpan _remainingTime;
    private DateTime endTime;
    private string Timeformat;
    private bool timerSet;
    private bool countIsReady;
    //private string placementId = "rewardedVideo";
    private bool loading;
    public static bool doubleCoins;

    void Awake()
    {
        timeBtwnADs = GameController.Instance.DelayBetweenAds;
        adButton = GetComponent<Button>();
        adButton.onClick.AddListener(ShowAd);//Add OnClick method to the AD button
        endTime = DateTime.Parse(PlayerPrefs.GetString("endTime", currentTime.ToString()));

        CheckAd();
    }

    private void OnEnable()
    {
        countIsReady = false;
        isADseen = false;
        SetLoading(false);
        GameController.Instance.ads.PrepareRewarded();
        string get = GameController.Instance.GetLocalizedValue(LocalizationItemType.get);
        string coins = GameController.Instance.GetLocalizedValue(LocalizationItemType.coins).ToLower();
        if (isFullLabel)
        {
            textLabel.text = $"{get} {GameController.Instance.ads.RewardAmount} {coins}";
        }
        else
        {
            textLabel.text = $"{GameController.Instance.ads.RewardAmount} {coins}";
        }
        CheckAd();
    }

    void CheckAd() //Is AD ready to be shown
    {
        UpdateTime();

        if (currentTime < endTime && (endTime.Subtract(currentTime) <= TimeSpan.Parse(timeBtwnADs)))
        {
            isADseen = true;
        }
        else EnableButton();
    }

    public void StartTimer() //Start countdown after watching AD
    {
        UpdateTime();
        endTime = currentTime + TimeSpan.Parse(timeBtwnADs);
        PlayerPrefs.SetString("endTime", endTime.ToString());
        isADseen = true;
    }

    private void UpdateTime()
    {
        currentTime = DateTime.Now;
        timerSet = true;
    }

    void Update()
    {

        if (isADseen && timerSet) // Check is AD already seen
        {
            if (currentTime < endTime) //Start countdown if there is remaining time
            {
                _remainingTime = endTime.Subtract(currentTime);
                tcounter = _remainingTime.TotalMilliseconds;
                countIsReady = true;
            }
            else
            {
                isADseen = false;
            }
        }
        else if (!loading)
        {
            EnableButton(); //Activate button after countdown
        }
        if (countIsReady)
        {
            StartCountdown(); //Start countdown if there is remaining time
        }
    }

    public string GetRemainingTime(double x)
    {
        TimeSpan tempB = TimeSpan.FromMilliseconds(x);
        Timeformat = string.Format("{0:D2}:{1:D2}", tempB.Minutes, tempB.Seconds);
        return Timeformat;
    }

    private void  StartCountdown() //Logic to display countdown on the AD button
    {
        timerSet = false;
        tcounter -= Time.deltaTime * 1000;
        DisableButton(GetRemainingTime(tcounter));
        if (tcounter <= 0)
        {
            countIsReady = false;
            UpdateTime();
        }
    }

    private void EnableButton()
    {
        adButton.interactable = true;
        timer.SetActive(false);
        if (icon != null)
        {
            icon.SetActive(true);
        }
    }

    private void DisableButton(string x)
    {
        adButton.interactable = false;
        timer.SetActive(true);
        timeLabel.text = x;
        if (icon != null)
        {
            icon.SetActive(false);
        }
    }

    public void ShowAd()
    {
        SetLoading(true);
        GameController.Instance.ads.ShowRewarded((rewardedStatus) =>
        {
            SetLoading(false);
            if (rewardedStatus.isWatched)
            {
                GameController.Instance.EarnCoins(rewardedStatus.rewardAmount, true); //Add coins when AD successfully watched
                StartTimer();
            }
            GameController.Instance.ads.PrepareRewarded();
        });
    }

    private void StartLoading()
    {
        timerSet = false;
        SetLoading(true);
        timer.SetActive(false);
    }

    public void SetLoading(bool value)
    {
        noConnection.SetActive(value);
        if (icon)
        {
            icon.SetActive(!value);
        }
        loading = value;
    }
}