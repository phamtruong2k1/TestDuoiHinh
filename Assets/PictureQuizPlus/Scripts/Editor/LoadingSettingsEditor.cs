using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LoadingSettings))]
public class LoadingSettingsEditor : Editor //To override AdsIapSettings class instance view in the Inspector
{
    LoadingSettings targetInstance;

    public bool GetBool(string name)
    {
        return serializedObject.FindProperty(name).boolValue;
    }

    public void SetProperty(string name, bool value)
    {
        serializedObject.FindProperty(name).boolValue = value;
    }

    public SerializedProperty GetProperty(string name)
    {
        return serializedObject.FindProperty(name);
    }

    void drawProp(string propName, string label, bool isExplisitLabel = true)
    {
        GUIStyle labelStyle = new GUIStyle() { wordWrap = true, padding = new RectOffset(0, 10, 0, 0) };
        if (EditorGUIUtility.isProSkin)
        {
            labelStyle.normal.textColor = new Color(0.8f, 0.8f, 0.8f);
        }
        SerializedProperty prop = GetProperty(propName);
        if (isExplisitLabel)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(label, labelStyle);
            GUILayout.FlexibleSpace();
            EditorGUILayout.PropertyField(prop, GUIContent.none, true);
            EditorGUILayout.EndHorizontal();
        }
        else
        {
            EditorGUILayout.PropertyField(prop, new GUIContent(label), true);
        }
        EditorGUILayout.Space();
    }

    void drawSectionLabel(string label)
    {
        GUIStyle style = new GUIStyle() { fontStyle = FontStyle.Bold, margin = new RectOffset(0, 0, 10, 5), wordWrap = true };
        if (EditorGUIUtility.isProSkin)
        {
            style.normal.textColor = Color.white;
        }
        EditorGUILayout.LabelField(label, style);
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        targetInstance = (LoadingSettings)target;
        if (targetInstance.localizations.Length == 0)
        {
            targetInstance.localizations = new Localization[] { new Localization() }; ;
        }
        drawSectionLabel("Mainstay parameters");
        drawProp("isContentStoredOnWebServer", "Use remote server storage");
        if (GetProperty("isContentStoredOnWebServer").boolValue)
        {
            drawProp("httpStorageUrl", "Remote url");
            drawProp("checkInternetConnectionUrl", "Url to check internet connection");
            drawProp("checkHostConnectionFile", "File to check host connection");
            drawProp("remoteImagesExtension", "File extension of remote images");
        }
        drawProp("localizations", "Localizations", false);

        if (GUI.changed)
        {
            serializedObject.ApplyModifiedProperties();
        }
    }
}
