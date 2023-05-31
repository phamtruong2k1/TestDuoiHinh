using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;


[RequireComponent(typeof(Text))]
public class LocalizedText : MonoBehaviour //Component class that mast be attached to every game element that should be localized
{
    public LocalizationItemType key;
    public bool toUpperCase;
    public bool toLowerCase;

    private Text text;

    private void Awake() {
        text = GetComponent<Text>();
    }

    private void Start()
    {
        if(GameController.Instance && GameController.Instance.IsDataReady) {
            SetValue();
        }
        else {
            StartCoroutine("Initialize");
        }
    }

    private IEnumerator Initialize()
    {
        yield return new WaitUntil(() => GameController.Instance && GameController.Instance.IsDataReady);
        SetValue();
    }

    private void SetValue()
    {
        string value = GameController.Instance.GetLocalizedValue(key);
        if (toUpperCase)
        {
            text.text = value.ToUpper();
        }
        else if (toLowerCase)
        {
            text.text = value.ToLower();
        }
        else text.text = value;
    }

}