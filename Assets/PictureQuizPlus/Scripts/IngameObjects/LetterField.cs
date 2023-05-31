using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LetterField : MonoBehaviour, IDragHandler, IEndDragHandler //Component class that attached to the each letter field
{
    public Text text;

    //States of the field
    public bool isEmpty = true;
    public bool isLocked = false;
    public bool isLast = false;
    public bool isDragged = false;

    public Letter letterReference; //Stores the letter reference when it is clicked

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragged && Mathf.Abs(eventData.pressPosition.x - eventData.position.x) > 200)
        {
            GameObject.FindObjectOfType<LevelFrontendController>().ClearAll();
            isDragged = true;
        }
    }

    public void OnWrongAnswer()
    {
        GetComponent<Animator>().Play("letterWrong", -1, 0f);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDragged = false;
    }

    internal void OnRightAnswer()
    {
        GetComponent<Animator>().Play("letterRight", -1, 0f);
    }
}
