using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public class Welcome : EditorWindow //Welcome screen
{
    private const float width = 500;
    private const float height = 200;
    private const string connectUrl = "https://connect.unity.com/u/5a8539f032b30600171a79c2";
    private const string ShowAtStartUP = "ShowAtStartUP";
    private static bool showAtStartup;
    private static bool interfaceInitialized;
    private static Texture buttonIcon;
    private static Welcome inst;
    private static GameController inst2;

    [MenuItem("Assets/PictureQuiz/Welcome Screen")]
    public static void OpenWelcomeWindowFromEditor()
    {
        GetWindow<Welcome>(true);
    }

    public static void OpenWelcomeWindow()
    {
        showAtStartup = EditorPrefs.GetInt(ShowAtStartUP, 1) == 1;
        if (showAtStartup)
        {
            GetWindow<Welcome>(true);
        }
        EditorPrefs.SetInt(ShowAtStartUP, 0);
        EditorApplication.update -= OpenWelcomeWindow;
    }

    static Welcome()
    {
        EditorApplication.update += OpenWelcomeWindow;
    }
    
    void OnEnable()
    {
        maxSize = new Vector2(width, height);
        minSize = maxSize;
    }

    public void OnGUI()
    {
        InitInterface();
        GUIStyle myStyle = new GUIStyle();
        myStyle.fontSize = 15;
        myStyle.fontStyle = FontStyle.Bold;
        myStyle.alignment = TextAnchor.MiddleCenter;
        GUI.Box(new Rect(0, 0, width, 60), "PICTURE QUIZ PLUS 3 \n Assets/PictureQuizPlus", myStyle);
        //		GUI.Label( new Rect(width-90,45,200,20),new GUIContent("Version : " +VERSION));
        GUILayoutUtility.GetRect(position.width, 64);
        GUILayout.Space(20);
        GUILayout.BeginVertical();

        if (Button(buttonIcon, "A QUESTION? A SUGGESTION? A REQUEST?", "Email me!"))
        {
            Application.OpenURL("mailto:ifelse3000@gmail.com");
        }
        if (Button(buttonIcon, "DONT FORGET TO CHECK DOCUMENTATION", "To have a fancy start with the asset"))
        {
            string[] guids = AssetDatabase.FindAssets("PQ+ Documentation");
            Selection.activeObject = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guids[0]), typeof(Object));
        }

        GUILayout.EndVertical();

    }

    void InitInterface()
    {

        if (!interfaceInitialized)
        {
            buttonIcon = (Texture)Resources.Load("arrow") as Texture;
            interfaceInitialized = true;
        }
    }

    bool Button(Texture texture, string heading, string body, int space = 10)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Space(24);
        GUILayout.Box(texture, GUIStyle.none, GUILayout.MaxWidth(48), GUILayout.MaxHeight(30));
        GUILayout.Space(10);
        GUILayout.BeginVertical();
        GUILayout.Space(1);
        GUILayout.Label(heading, EditorStyles.boldLabel);
        GUILayout.Label(body);
        GUILayout.EndVertical();
        GUILayout.EndHorizontal();

        var rect = GUILayoutUtility.GetLastRect();
        EditorGUIUtility.AddCursorRect(rect, MouseCursor.Link);

        bool returnValue = false;
        if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
        {
            returnValue = true;
        }

        GUILayout.Space(space);

        return returnValue;
    }
}
