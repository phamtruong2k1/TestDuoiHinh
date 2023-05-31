using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[ExecuteInEditMode]

public class EditorController : MonoBehaviour
{
    public static GameSettings EditorGamesettings { get; set; }

    // Start is called before the first frame update
    void OnValidate()
    {
#if UNITY_EDITOR
        if (Application.isPlaying) return;
        GameSettings[] some = Utils.GetAllInstances<GameSettings>();
        EditorGamesettings = some.FirstOrDefault(s => s.colors != null && s.colors.Length > 0);
        // Debug.Log(EditorGamesettings);
        /* if(EditorGamesettings != null) {
            GameObject about = GameObject.FindGameObjectWithTag("about");
            about?.SetActive(EditorGamesettings.aboutButton);
            GameObject moregames = GameObject.FindGameObjectWithTag("moregames");
                moregames?.SetActive(EditorGamesettings.isMoreGamesEnabled);
        } */
#endif   
    }
}
