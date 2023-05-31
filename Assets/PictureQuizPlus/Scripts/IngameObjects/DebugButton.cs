using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class DebugButton : MonoBehaviour
{

    // Use this for initialization
    public void Subscribe(int scene)
    {
        Button but = GetComponent<Button>();
        switch (scene)
        {
            case 0:
                but.onClick.AddListener(ResetScene);
                break;
            case 1:
                but.onClick.AddListener(ResetDirectory);
                break;

            default:
                break;
        }
    }

    // Update is called once per frame
    void ResetScene()
    {
        string path = System.IO.Path.Combine(Application.persistentDataPath, "saves.json");
        if (File.Exists(path))
        {
            File.Delete(path);
        }
        PlayerPrefs.DeleteAll();
        GameController.Instance.Reload();
    }

    void ResetDirectory()
    {
        GameController.Instance.ads?.KillBanner();
        GameController.Instance.ReloadDirectory();
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(1);
    }
}
