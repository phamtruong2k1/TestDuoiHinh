using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LocalizationData))]
public class LozalizationDataEditor : Editor //To override AdsIapSettings class instance view in the Inspector
{

    public readonly string[] ALLOWED_FILE_EXTENSIONS = { "*.jpg", "*.jpeg", "*.png", "*.JPEG", "*.JPG", "*.PNG" };
    public readonly string ALLOWED_FILE_EXTENSIONS_REGEXP = @"^\d+\.(jpeg|jpg|png|JPEG|JPG|PNG)$";
    LocalizationData targetInstance;

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
        EditorGUILayout.LabelField(label, new GUIStyle() { fontStyle = FontStyle.Bold, margin = new RectOffset(0, 0, 10, 5), wordWrap = true });
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        targetInstance = (LocalizationData)target;

        if (GUILayout.Button("Load from json", GUILayout.Width(305), GUILayout.Height(30)))
        {
            string filePath = EditorUtility.OpenFilePanel("Select localization data file", Path.Combine(Application.dataPath, "PictureQuizPlus"), "json");

            if (!string.IsNullOrEmpty(filePath))
            {
                string dataAsJson = File.ReadAllText(filePath);
                JsonUtility.FromJsonOverwrite(dataAsJson, targetInstance);
            }
            // Application.OpenURL("https://developers.google.com/admob/unity/start");
        }
        if (GUILayout.Button("Refresh", GUILayout.Width(305), GUILayout.Height(30)))
        {
            RecalculateTasksData();
            // Application.OpenURL("https://developers.google.com/admob/unity/start");
        }
        if (GUILayout.Button("Save", GUILayout.Width(305), GUILayout.Height(30)))
        {
            AssetDatabase.Refresh();
            AssetDatabase.SaveAssets();
            // Application.OpenURL("https://developers.google.com/admob/unity/start");
        }
        EditorGUILayout.Space();

        drawProp("tasksData", "Categories", false);
        drawProp("subCategories", "Subcategories", false);
        drawProp("gameItems", "Translations", false);
        drawProp("randomLetters", "The letters to mixin", false);

        if (GUI.changed)
        {
            serializedObject.ApplyModifiedProperties();
            if (targetInstance.gameItems.Length == 0)
            {
                targetInstance.gameItems = ScriptableObject.CreateInstance<LocalizationData>().gameItems;
            }
        }
    }

    private void RecalculateTasksData()
    {
        ConstructedContentData data = ConstructContentData();

        foreach (Category item in data.tasksData)
        {
            if (targetInstance.tasksData != null && targetInstance.tasksData.Any(c => c.Name == item.Name))
            {
                targetInstance.tasksData = targetInstance.tasksData
                    .Select(c =>
                    {
                        if (c.Name == item.Name)
                        {
                            int diff = item.Levels.Length - c.Levels.Length;
                            // Debug.Log(diff);
                            if (diff > 0)
                            {
                                List<Level> toAppend = new List<Level>();
                                for (var i = 0; i < diff; i++)
                                {
                                    Level task = new Level(c.Levels.Length + i + 1);
                                    toAppend.Add(task);
                                }
                                c.Levels = c.Levels.Concat(toAppend).ToArray();
                            }
                        }
                        return c;
                    })
                    .ToArray();
            }
            else
            {
                Category newCategory = item;
                newCategory.sortingIndex = targetInstance.tasksData.Length + 1;
                targetInstance.tasksData = targetInstance.tasksData.Append(newCategory).ToArray();
            }
        }

        foreach (SubCategory subCatItem in data.subCategories)
        {
            if (targetInstance.subCategories.Any(sc => sc.Name == subCatItem.Name))
            {
                targetInstance.subCategories = targetInstance.subCategories
                    .Select(sc =>
                    {
                        if (sc.Name == subCatItem.Name)
                        {
                            var diff = subCatItem.subcategories.Except(sc.subcategories);
                            // Debug.Log(diff);
                            if (diff.Count() > 0)
                            {
                                sc.subcategories = sc.subcategories.Concat(diff).ToArray();
                            }
                        }
                        return sc;
                    })
                    .ToArray();
            }
            else
            {
                SubCategory newSubCategory = subCatItem;
                newSubCategory.sortingIndex = targetInstance.subCategories.Length + 1;
                targetInstance.subCategories = targetInstance.subCategories.Append(newSubCategory).ToArray();
            }

        }
    }

    private ConstructedContentData ConstructContentData()
    {
        ConstructedContentData data = new ConstructedContentData();
        data.tasksData = new List<Category>();
        data.subCategories = new List<SubCategory>();

        string rootPath = Path.Combine(new string[] { Application.dataPath, "PictureQuizPlus", "Resources", "content" });
        string[] directoryNames = CutNames(Directory.GetDirectories(rootPath));
        System.Action<string, string, int> createTasksFromDirectory = (string path, string categoryName, int categoryIndex) =>
        {
            Regex reg = new Regex(ALLOWED_FILE_EXTENSIONS_REGEXP);
            int filesCount = Directory.GetFiles(path).Select(Path.GetFileName).Where(fileName => reg.IsMatch(fileName)).Count();
            data.tasksData = data.tasksData.Append(new Category(categoryName, filesCount, categoryIndex));
        };
        for (int i = 0; i < directoryNames.Length; i++)
        {
            string dirName = directoryNames[i];
            string checkPath = Path.Combine(rootPath, dirName);
            if (IsSubCategory(checkPath))
            {
                string[] subCategoriesNames = CutNames(Directory.GetDirectories(checkPath));
                SubCategory sub = new SubCategory(i + 1, dirName, subCategoriesNames);
                data.subCategories = data.subCategories.Append(sub);
                for (int i2 = 0; i2 < subCategoriesNames.Length; i2++)
                {
                    string subPath = Path.Combine(checkPath, subCategoriesNames[i2]);
                    createTasksFromDirectory(subPath, subCategoriesNames[i2], i2 + 1);
                }
            }
            else
            {
                createTasksFromDirectory(checkPath, dirName, i + 1);
            }

        }
        return data;
    }

    private bool IsSubCategory(string path)
    {
        return Directory.GetDirectories(path).Length > 0;
    }

    private string[] CutNames(string[] array) //Utility method to handle filenames
    {

        string[] cuttedArr = new string[array.Length];
        for (int i = 0; i < array.Length; i++)
        {
            string[] temp = array[i].Split('\\');
            cuttedArr[i] = temp[temp.Length - 1];
        }
        // cuttedArr = cuttedArr.Where(x => x != "~PREFABS").ToArray();
        return cuttedArr;
    }

}


public struct ConstructedContentData
{
    /*   public ConstructedContentData() {
          tasksData = new List<Category>();
          subCategories = new List<SubCategory>();
      } */
    public IEnumerable<Category> tasksData;
    public IEnumerable<SubCategory> subCategories;
}