using System.IO;
using UnityEngine;
using UnityEditor;

//Extensions for editor. Look at Tools tab
public class EditorExtension : MonoBehaviour
{
    [MenuItem("Assets/PictureQuiz/Clear Saved Data", false, 1)]
    private static void ClearSavedData()
    {
        string path = System.IO.Path.Combine(Application.persistentDataPath, "saves.json");
        if (File.Exists(path))
        {
            File.Delete(path);
        }
        PlayerPrefs.DeleteAll();
    }

    [MenuItem("Assets/PictureQuiz/Open Game Settings", false, 2)]
    public static void Autoselect()
    {
        string[] guids = AssetDatabase.FindAssets("Game", new string[] { "Assets" + Path.DirectorySeparatorChar + "PictureQuizPlus" });
        Selection.activeObject = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guids[0]), typeof(Object));
    }

    [MenuItem("Assets/PictureQuiz/Open Loading Settings", false, 3)]

    public static void AutoselectLoadingSettings()
    {
        string[] guids = AssetDatabase.FindAssets("Loading", new string[] { "Assets" + Path.DirectorySeparatorChar + "PictureQuizPlus" });
        Selection.activeObject = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guids[0]), typeof(Object));
    }


    [MenuItem("Assets/PictureQuiz/Find Documentation file")]

    public static void AutoselectInfo()
    {
        string[] guids = AssetDatabase.FindAssets("PQ+ Documentation");
        Selection.activeObject = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guids[0]), typeof(Object));
    }

    public static void CreateAsset<T>(string name) where T : ScriptableObject
    {
        var asset = ScriptableObject.CreateInstance<T>();
        ProjectWindowUtil.CreateAsset(asset, name + ".asset");
    }

}
