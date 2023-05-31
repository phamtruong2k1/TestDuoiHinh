using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ChoseAnAnswerManager : MonoBehaviour //Class to handle ChoseAnAnswer game type
{
    public BetBox betBox;
    public ChoseAnAnswerBox answerBox;
    private Button rightAnswerButton;
    private Button currentSelection;
    private bool freshStart = true;
    private LevelFrontendController frontendController;

    public static List<Button> wrongAnswers;
    public static event UnityAction<Hint, object> mistakeHappend;
    public static List<Animator> animators;

    public static bool optionsShowed = false;

    void Awake()
    {
        frontendController = FindObjectOfType<LevelFrontendController>();
        animators = new List<Animator>();
        wrongAnswers = new List<Button>();
        optionsShowed = false;
    }

    public void Initialize(LevelInfo data)
    {

        //Spawn Bet buttons on level start
        if (data.levelBet == default && GameController.Instance.UseBets)
        {
            betBox.gameObject.SetActive(true);
            /*  if (shiftForBanner)
             {
                 betBox.transform.localPosition -= ResolutionSolver.shift;
             } */
            //Subscribe anonymous method for each button
            for (int i = 0; i < betBox.buttons.Length; i++)
            {
                Button current = betBox.buttons[i].transform.parent.gameObject.GetComponent<Button>();
                int value = Int32.Parse(betBox.buttons[i].text);

                current.onClick.AddListener(() =>
                {
                    LevelStateController.PrepareSingleChoiceModeState(value);
                    if (currentSelection)
                    {
                        currentSelection.GetComponent<Image>().color = LevelFrontendController.levelColor;
                    }
                    currentSelection = current;
                    currentSelection.GetComponent<Image>().color = GameController.Instance.GetColor(ThemeColorEnum.Positive);
                    SoundsController.instance.PlaySound("delete", 0.2f);
                });

            }
            //Subscribe anonymous method for bet button
            betBox.betBut.onClick.AddListener(() =>
            {
                if (Utils.EnoughCoinsForHint(Hint.bet) && currentSelection)
                {
                    betBox.gameObject.SetActive(false);
                    LevelStateController.TryToEducate(LocalizationItemType.education_hints, frontendController.hintsButton.transform);
                    StartCoroutine(HandleBet(data)); //Spawn answers buttons
                    HintPopup.needRestart = true;
                    SoundsController.instance.PlaySound("coins");
                }
            });
            LevelStateController.TryToEducate(LocalizationItemType.education_bet, betBox.betBut.transform);
            LevelStateController.isPaused = false;
        }
        else if (!GameController.Instance.UseBets)
        {
            freshStart = false;
            LevelStateController.PrepareSingleChoiceModeState();
            StartCoroutine(HandleBet(data)); //Spawn answers buttons
            HintPopup.needRestart = true;
        }
        else
        {
            freshStart = false;
            StartCoroutine(HandleBet(data)); //Spawn answers buttons
        }
    }

    //Spawn answers buttons
    private IEnumerator HandleBet(LevelInfo data)
    {
        answerBox.gameObject.SetActive(true);
        if (freshStart)
        {
            LevelFrontendController.HintEvent(Hint.bet, null);
        }
        List<string> answersList = new List<string>(GameController.Instance.CurrentLevel.wrongAnswers);
        answersList.Add(LevelStateController.rightAnswer);
        Utils.Shuffle(answersList);
        //Set data for each answer button and add listeners
        for (int i = 0; i < answerBox.buttons.Length; i++)
        {
            Button current = answerBox.buttons[i].transform.parent.gameObject.GetComponent<Button>();
            Animator currentAnim = current.GetComponent<Animator>();
            animators.Add(currentAnim); //Collect all the animators for Chanse to mistake hint

            answerBox.buttons[i].text = answersList.First().ToUpper();
            if (answersList.First() == LevelStateController.rightAnswer)
            {
                rightAnswerButton = current;
                current.onClick.AddListener(() => StartCoroutine(AnswerButtonHandler(true, current)));
            }
            else
            {
                wrongAnswers.Add(current);
                current.onClick.AddListener(() => StartCoroutine(AnswerButtonHandler(false, current)));
            }

            answersList.Remove(answersList.First());
            UnityEngine.Random.InitState((int)System.DateTime.Now.Ticks);
            float x = UnityEngine.Random.Range(0.1f, 0.45f); //Random time before spawn each answer button
            yield return new WaitForSeconds(x);
            answerBox.buttons[i].transform.parent.gameObject.SetActive(true);

            //Handle saved data
            if (data.DisclosedAnswers.Count > 0)
            {
                if (LevelStateController.GetCurrentState().chanseUsed)
                {
                    if (answerBox.buttons[i].text == data.DisclosedAnswers.First())
                    {
                        currentAnim.SetBool("WrongOnStart", true);
                    }
                }
                else
                {
                    foreach (var item in data.DisclosedAnswers)
                    {
                        if (answerBox.buttons[i].text == item)
                        {
                            currentAnim.SetBool("WrongHide", true);
                        }
                    }
                }
            }
        }
        //If Chanse to mistake already used but answer still not chosen, start loop animations
        if (LevelStateController.GetCurrentState().chanseUsed && data.DisclosedAnswers.Count == 0)
        {
            Utils.ChanseToMistake(animators, true);
        }
        optionsShowed = true;
        LevelStateController.isPaused = false;
    }

    //Method to handle animations transitions and game logic for the answer button
    public IEnumerator AnswerButtonHandler(bool state, Button button)
    {
        if (LevelStateController.isPaused == false)
        {
            LevelStateController.isPaused = true;
            SoundsController.instance.PlaySound("blup");
            Animator animator = button.GetComponent<Animator>();

            if (state) //If right answer chosen by user
            {
                //Return coins for ChanseToMistake hint
                if (LevelStateController.GetCurrentState().chanseUsed && LevelStateController.GetCurrentState().DisclosedAnswers.Count == 0)
                {
                    LevelStateController.completeLvlCoins += LevelStateController.GetHintPrice(Hint.chance);
                    Utils.ChanseToMistake(animators, false);
                }
                StartCoroutine(frontendController.LevelCompleted(2f)); //WinPopup appear
                animator.SetBool("ClickedRight", true);
                yield return new WaitForSeconds(1f);
                SoundsController.instance.PlaySound("trill", 0.8f);
            }
            else //If wrong answer chosen
            {
                animator.SetBool("ClickedFalse", true);
                if (LevelStateController.GetCurrentState().chanseUsed == false)
                {
                    StartCoroutine(HandleWrongAnswer());
                }
                else
                {
                    if (LevelStateController.GetCurrentState().DisclosedAnswers.Count > 0)
                    {
                        StartCoroutine(HandleWrongAnswer());
                    }
                    else //If ChanseToMistake was used, highlight answer with red and continue the game
                    {
                        Utils.ChanseToMistake(animators, false);
                        mistakeHappend(Hint.one_option, button.GetComponentInChildren<Text>().text);
                        LevelStateController.isPaused = false;
                        yield return new WaitForSeconds(1f);
                        SoundsController.instance.PlaySound("error", 0.8f);
                    }
                }
            }
        }
    }
    //Wrong answer logic and animations transitions
    public IEnumerator HandleWrongAnswer()
    {
        if (GameController.Instance.DeductCoinsWhenWrong && !GameController.Instance.UseBets)
        {
            LevelStateController.completeLvlCoins = -LevelStateController.completeLvlCoins;
        }
        else
        {
            LevelStateController.completeLvlCoins = 0;
        }
        StartCoroutine(frontendController.LevelCompleted(2f));
        yield return new WaitForSeconds(1f);
        SoundsController.instance.PlaySound("error", 0.8f);
        yield return new WaitForSeconds(0.2f);
        rightAnswerButton.GetComponent<Animator>().SetBool("Right", true);
    }
}
