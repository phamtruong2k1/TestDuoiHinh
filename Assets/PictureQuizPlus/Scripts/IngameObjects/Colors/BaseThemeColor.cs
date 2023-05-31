using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEditor;

public class BaseThemeColor : MonoBehaviour //Component class that mast be attached to every game element that should be localized
{
    public ThemeColorEnum key;

    private Color color;
    GameSettings settings;

    public Action OnColorApplied;

    public float applyTransparency = -1;

    private void Start()
    {
        StartCoroutine("Initialize");
    }


    private IEnumerator Initialize()
    {
        if (GameController.Instance == null)
        {
            yield return false;
        }
        else
        {
            yield return new WaitUntil(() => GameController.Instance.IsSettingsReady);
            Color clr = GameController.Instance.GetColor(key);
            if (applyTransparency >= 0)
            {
                Color = new Color(clr.r, clr.g, clr.b, applyTransparency);
            }
            else
            {
                Color = clr;
            }
            if (OnColorApplied != null)
            {
                OnColorApplied();
            }
        }
    }

    public virtual Color Color
    {
        get { return color; }
        set { color = value; }
    }

    public void ChangeColor(ThemeColorEnum key)
    {
        this.key = key;
        if (!gameObject.activeSelf) return;
        StartCoroutine("Initialize");
    }

    public void OnValidate()
    {
        if (Application.isPlaying) return;
        // Debug.Log("OnValidate");
        if (EditorController.EditorGamesettings == null) return;
        Color clr = EditorController.EditorGamesettings.colors.First(c => c.key == key).color;
        if (applyTransparency >= 0)
        {
            Color = new Color(clr.r, clr.g, clr.b, applyTransparency);
        }
        else Color = clr;

    }


}