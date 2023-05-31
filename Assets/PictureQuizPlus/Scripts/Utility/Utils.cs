using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.IO;
using System.Linq;
using System.Collections;
using System.Xml.Linq;
using UnityEditor;
using System;
#if UNITY_AD
using UnityEngine.Advertisements;
#endif
#if ENABLE_ADMOB
using GoogleMobileAds.Api;
#endif

//Static methods that handle almost all game functionality
public static class Utils
{

    public static float GetScreenRatio()
    {
        int width = Screen.width;
        int height = Screen.height;
        int portraitWidth = width > height ? width : height; //For horizontal view only tablets
        int portraitHeight = width > height ? height : width;
        return portraitWidth / (float)portraitHeight;
    }

    public static void LoadScene(int index)
    {
        SceneManager.LoadScene(index);
    }

    public static IEnumerator Share() //Handler to Ask friends button
    {
        yield return new WaitForSecondsRealtime(0.1f); //Wait a bit while hint popup closed
        string path = Path.Combine(Application.persistentDataPath, PlayerPrefs.GetString("scr", "defaultScreen.png")); //Delete old screenshot
        if (File.Exists(path))
        {
            File.Delete(path);
        }
        //Create new screenshot with unique filename to prevent apps to use old cashed image
        PlayerPrefs.SetString("scr", "screen" + System.DateTime.Now.Ticks + ".png");
        ScreenCapture.CaptureScreenshot(PlayerPrefs.GetString("scr"));
        path = Path.Combine(Application.persistentDataPath, PlayerPrefs.GetString("scr"));
        yield return new WaitForSeconds(0.6f);

        //Use mobile platform native plugin to create sharing message 
#if SHARING
        new NativeShare().SetTitle(GameController.Instance.GetLocalizedValue(LocalizationItemType.sharing_title))
                         .SetText(GameController.Instance.GetLocalizedValue(LocalizationItemType.sharing_text))
                         .SetUrl(GameController.Instance.AppUrl)
#if !UNITY_EDITOR
                         .AddFile(path)
#endif
                         .Share();
#endif
        LevelStateController.isPaused = false;
    }

    public static string RenderMustache(string testString, LocalizationItemType? key = null)
    {
        while (testString.Contains("{{"))
        {
            int startIndex = testString.IndexOf("{{");
            int endIndex = testString.IndexOf("}}");
            if (endIndex == -1) endIndex = testString.Length - 1;
            int length = endIndex - (startIndex + 2);
            string substr = testString.Substring(startIndex + 2, length).ToLower();
            testString = testString.Remove(startIndex, length + 4);
            string toInsert = "";
            if (substr == "category")
            {
                toInsert = GameController.Instance.CurrentCategory.LocalizedName;
            }
            else 
            {
                bool enumExists = Enum.TryParse(substr, out LocalizationItemType itemType);
                bool itemTypeIsDifferentOrNull = !key.HasValue || itemType != key;
                bool keyExists = GameController.Instance.LocalizedText.ContainsKey(itemType);
                if(enumExists && itemTypeIsDifferentOrNull && keyExists) {
                    toInsert = GameController.Instance.LocalizedText[itemType];
                }
            }
            testString = testString.Insert(startIndex, toInsert);
        }
        return testString;
    }

    public static System.Uri BuildUrl(string[] parts)
    {
        string url = parts
            .Where(part => !string.IsNullOrEmpty(part))
            .Select(part => part.Trim(new char[] { '/', ' ' }))
            .Aggregate((x, y) => x + "/" + y);
        return new System.Uri(url);
    }
    //When the letter clicked
    public static void LetterClick(Letter letter, List<LetterField> list)
    {
        if (!LevelStateController.isPaused)
        {
            foreach (var item in list)
            {
                if (item.isEmpty)
                {
                    letter.GetComponent<Animation>().Play("FadeOut");
                    letter.GetComponent<Button>().interactable = false;
                    item.text.text = letter.textField.text;
                    item.isEmpty = false;
                    item.letterReference = letter;

                    foreach (var item2 in list)
                    {
                        if (item2.isLast)
                        {
                            item2.isLast = false;
                        }
                    }
                    item.isLast = true;
                    LevelStateController.TryToEducate(LocalizationItemType.education_clear, UnityEngine.Object.FindObjectOfType<LevelFrontendController>().lettersFields.transform);

                    SoundsController.instance.PlaySound("blup");
                    break;
                }
            }
        }
    }

    //Clears letters field
    public static void Clear(LetterField item)
    {
        if (item.isEmpty == false && item.letterReference)
        {
            item.letterReference.GetComponent<Animation>().Play("FadeIn");
            item.text.text = null;
            item.isEmpty = true;
            item.letterReference.GetComponent<Button>().interactable = true;
            item.letterReference = null;
            item.isLast = false;
        }
    }
    //Clears all letters fields
    public static void ClearAll(List<LetterField> list)
    {
        foreach (var item in list)
        {
            if (item.letterReference && !item.isLocked)
            {
                Clear(item);
            }
        }
        SoundsController.instance.PlaySound("delete");
    }
    //Clear one letter field
    public static void ClearField(List<LetterField> list)
    {
        foreach (var item in list)
        {
            if (item.isLast && item.letterReference)
            {
                SoundsController.instance.PlaySound("delete");
                Clear(item);
                if (list.IndexOf(item) >= 1)
                {
                    list[list.IndexOf(item) - 1].isLast = true;
                }

            }
        }

    }

    public static void RevealOneLetter(List<LetterField> fields, List<Letter> letters, List<char> rightLetters)
    {
        ClearAll(fields);
        RevealLetter(fields, letters, rightLetters);
    }

    //'Reveal a letter' hint handler 
    public static void RevealLetter(List<LetterField> fields, List<Letter> letters, List<char> rightLetters)
    {
        foreach (var item in letters)
        {
            if (rightLetters.Count > 0 && (rightLetters[0] == ' ' || rightLetters[0] == '_'))
            {
                rightLetters.Remove(rightLetters[0]);
            }
            if (rightLetters.Count > 0 && item.textField.text.ToLower() == rightLetters[0].ToString())
            {
                rightLetters.Remove(rightLetters[0]);
                item.GetComponent<Animation>().Play("FadeOut");
                item.GetComponent<Button>().interactable = false;
                foreach (var field in fields)
                {
                    if (field.isEmpty)
                    {
                        field.text.text = item.textField.text;
                        field.isEmpty = false;
                        field.isLocked = true;
                        item.textField.text = null;
                        break;
                    }
                }
                break;
            }
        }
    }

    //"Remove letters" hint handler
    public static void RemoveWrongLetters(List<LetterField> fields, List<Letter> letters, List<char> rightLetters)
    {
        ClearAll(fields);
        List<char> newList = new List<char>(rightLetters);
        int counter = 0;
        foreach (var item in letters)
        {
            if (item.GetComponent<Button>().interactable)
            {
                for (int i = 0; i < newList.Count; i++)
                {
                    if (item.GetComponentInChildren<Text>().text.ToLower() == newList[i].ToString())
                    {
                        newList.Remove(newList[i]);
                        goto Next;
                    }
                }
                //Next instructions are to chose randomly wrong letters that should stay on board depending on words length
                counter++;
                if (rightLetters.Count <= 3)
                {
                    if (counter > 3)
                    {
                        DisableButton(item.GetComponent<Button>());
                    }
                    else
                    {
                        if (UnityEngine.Random.Range(0, 3) > 0)
                        {
                            DisableButton(item.GetComponent<Button>());
                            counter--;
                        }
                        goto Next;
                    }
                }
                else
                {
                    if ((rightLetters.Count < 8 && counter > 2) || (rightLetters.Count > 7 && counter > 1))
                    {
                        DisableButton(item.GetComponent<Button>());
                    }
                    else
                    {
                        if (UnityEngine.Random.Range(0, 3) > 0)
                        {
                            DisableButton(item.GetComponent<Button>());
                            counter--;
                        }
                        goto Next;
                    }
                }
            }
        Next:;
        }
    }

    //"Get an answer"
    public static void SolveTask(List<LetterField> fields, List<Letter> letters, List<char> rightLetters)
    {
        ClearAll(fields);
        do { RevealLetter(fields, letters, rightLetters); }
        while (rightLetters.Count > 0);
    }

    internal static string RemoveOneWrongAnswer(List<Button> items)
    {
        Shuffle(items);
        string answer = items.First().GetComponentInChildren<Text>().text;
        items.First().GetComponent<Animator>().SetBool("WrongHide", true);
        items.Remove(items.First());
        SoundsController.instance.PlaySound("delete");
        return answer;
    }

    internal static string[] RemoveTwoWrongAnswers(List<Button> items)
    {
        Shuffle(items);
        var x = items.Take(2).ToArray();
        foreach (var item in x)
        {
            item.GetComponent<Animator>().SetBool("WrongHide", true);
        }
        SoundsController.instance.PlaySound("delete");
        return x.Select(y => y.GetComponentInChildren<Text>().text).ToArray();
    }

    internal static void ChanseToMistake(List<Animator> animators, bool value)
    {

        foreach (var item in animators)
        {
            item.SetBool("isLoop", value);
        }
    }

    public static void SetUpCost(Transform go, int value)
    {
        go.Find("Image/cost").GetComponent<Text>().text = value.ToString();
    }

    public static void DisableButton(Button button) //Disabling buttons logic
    {
        if (button.GetComponent<Animation>())
        {
            button.GetComponent<Animation>().Play("FadeOut");
        }
        button.interactable = false;

        if (button.GetComponent<Image>().color.a > 0.9f)
        {
            button.GetComponent<Image>().color -= new Color(0, 0, 0, 0.5f);

        }

        SpriteColor spriteColor = button.GetComponent<SpriteColor>();
        if (spriteColor != null)
        {
            spriteColor.OnColorApplied += () =>
            {
                if (button.GetComponent<Image>().color.a > 0.9f)
                {
                    button.GetComponent<Image>().color -= new Color(0, 0, 0, 0.5f);

                }
            };
        }
    }

    public static void EnableButton(Button button) //Enabling buttons logic
    {
        if (button.GetComponent<Animation>())
        {
            button.GetComponent<Animation>().Play("FadeIn");
        }
        button.interactable = true;
        if (button.GetComponent<Image>().color.a <= 0.5f)
        {
            button.GetComponent<Image>().color += new Color(0, 0, 0, 0.5f);

        }
        SpriteColor spriteColor = button.GetComponent<SpriteColor>();
        if (spriteColor != null)
        {
            spriteColor.OnColorApplied += () =>
            {
                if (button.GetComponent<Image>().color.a <= 0.5f)
                {
                    button.GetComponent<Image>().color += new Color(0, 0, 0, 0.5f);

                }
            };
        }
    }

    //Handlers for sound buttons
    public static void ToggleMusic(Button button)
    {
        ToggleMusic(button, !SoundsController.instance.isMusic);
    }

    public static void ToggleMusic(Button button, bool state)
    {
        if (!state)
        {
            button.GetComponent<Image>().color -= new Color(0, 0, 0, 0.5f);
            SoundsController.instance.SetMusic(false);
        }
        else
        {
            button.GetComponent<Image>().color += new Color(0, 0, 0, 0.5f);
            SoundsController.instance.SetMusic(true);
        }
    }

    public static void ToggleSoundsAndMusic(Button button)
    {
        if (!SoundsController.instance.isSounds && !SoundsController.instance.isMusic)
        {
            button.GetComponent<Image>().color += new Color(0, 0, 0, 0.5f);
            SoundsController.instance.SetSound(true);
            SoundsController.instance.SetMusic(true);

        }
        else
        {
            button.GetComponent<Image>().color -= new Color(0, 0, 0, 0.5f);
            SoundsController.instance.SetSound(false);
            SoundsController.instance.SetMusic(false);
        }

    }

    public static void ToggleSounds(Button button)
    {
        ToggleSounds(button, !SoundsController.instance.isSounds);
    }


    public static void ToggleSounds(Button button, bool state)
    {
        if (!state)
        {
            button.GetComponent<Image>().color -= new Color(0, 0, 0, 0.5f);
            SoundsController.instance.SetSound(false);
        }
        else
        {
            button.GetComponent<Image>().color += new Color(0, 0, 0, 0.5f);
            SoundsController.instance.SetSound(true);
        }
    }

    //What to do after level complete
    public static void OnLevelComplete()
    {
        Action<bool> loadNextLevel = (bool ready) =>
        {
            if (LevelStateController.currentLevel.index == GameController.Instance.CurrentCategory.Levels.Length)
            {
                GameController.Instance.IsCategoryCompleted = true;
                LoadScene(0);
            }
            else
            {
                GameController.Instance.LoadNextCategory();
                Sprite sprite = GameController.Instance.ResourcesManager.Get<LevelImagesResource, Sprite>(GameController.Instance.CurrentLevel).GetFirst();
                if (sprite == null)
                {
                    GameController.Instance.errorMessage = GameController.Instance.LoadingSettings.getFileNotFoundMessage(GameController.Instance.CurrentLocalization.filename);
                    GameController.Instance.popup.Open<ErrorPopup>(
                        new PopupSettings()
                        {
                            color = ThemeColorEnum.Negative,
                            title = GameController.Instance.LoadingSettings.getErrorPopupTitle(GameController.Instance.CurrentLocalization.filename),
                            unableToClose = true
                        }
                    );
                }
                else LoadScene(1);
            }
        };
        if (GameController.Instance.shouldShowInterstitial)
        {
            Debug.Log(GameController.Instance.shouldShowInterstitial);
            GameController.Instance.ads.ShowInterstitial(loadNextLevel);
        }
        else
        {
            loadNextLevel(true);
        }
    }

    public static void ChangeColor(Button but)
    {
        if (but.gameObject.GetComponent<Image>().color == Color.white)
        {
            but.gameObject.GetComponent<Image>().color -= new Color(0, 0.5f, 0.5f, 0);
        }
        else
        {
            but.gameObject.GetComponent<Image>().color += new Color(0, 0.5f, 0.5f, 0);
        }
    }

    public static void Shuffle<T>(List<T> list) //Randomly shuffle a collection
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            T tmp = list[i];
            list[i] = list[j];
            list[j] = tmp;
        }
    }

    public static void SetCoinsText() //Coins UI text handler
    {
        if (LevelFrontendController.levelCompleted == false)
        {
            GameObject coins = GameObject.FindGameObjectWithTag("coinText");
            Animator animator = coins.GetComponent<Animator>();
            if (animator != null)
            {
                animator.Play("coinDrag");
            }
            coins.GetComponent<Text>().text = GameController.Instance.GetCoinsCount().ToString();
        }
    }

    public static bool EnoughCoinsForHint(Hint hint)
    {
        int hintCost = LevelStateController.GetHintPrice(hint);

        if (GameController.Instance.GetCoinsCount() < hintCost)
        {
            SoundsController.instance.PlaySound("wrong");
            SetCoinsText();
            return false;
        }
        else return true;
    }

    public static bool EnoughCoinsForHint(Hint hint, bool isSoundNeeded)
    {
        int hintCost = LevelStateController.GetHintPrice(hint);

        if (GameController.Instance.GetCoinsCount() < hintCost)
        {
            if (isSoundNeeded)
            {
                SoundsController.instance.PlaySound("wrong");
            }
            SetCoinsText();
            return false;
        }
        else return true;
    }

    public static IEnumerator SpawnResourcesObject(string path, float delay, string sound, Transform parent)
    {
        GameObject go = Resources.Load(path) as GameObject;
        yield return new WaitForSeconds(delay);
        if (!string.IsNullOrEmpty(sound))
        {
            SoundsController.instance.PlaySound(sound, 0.6f);
        }
        UnityEngine.Object.Instantiate(go, parent);
    }

    public static GameObject CreateFromPrefab(string prefabName)
    {
        return Resources.Load(Path.Combine("Prefabs", prefabName)) as GameObject;
    }

    public static GameObject CreateFromPrefab(string[] prefabName)
    {
        return Resources.Load(Path.Combine(prefabName.Prepend("Prefabs").ToArray())) as GameObject;
    }

    public static Transform getRootTransform()
    {
        return GameObject.FindGameObjectWithTag("main_canvas").transform;
    }

    /// <summary>
    /// Returns the input string with the first character converted to uppercase, or mutates any nulls passed into string.Empty
    /// </summary>
    public static string Capitalize(this string s)
    {
        if (string.IsNullOrEmpty(s))
            return string.Empty;

        char[] a = s.ToCharArray();
        a[0] = char.ToUpper(a[0]);
        return new string(a);
    }

#if UNITY_EDITOR
    public static T[] GetAllInstances<T>() where T : ScriptableObject
    {
        string[] guids = AssetDatabase.FindAssets("t:" + typeof(T).Name);  //FindAssets uses tags check documentation for more info
        T[] a = new T[guids.Length];
        // Debug.Log($"guids {guids.Length}");
        for (int i = 0; i < guids.Length; i++)         //probably could get optimized 
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            // Debug.Log($"guids {path}");
            a[i] = AssetDatabase.LoadAssetAtPath<T>(path);
        }

        return a;

    }
#endif

    public static Texture2D DuplicateTexture(Texture2D source)
    {
        RenderTexture renderTex = RenderTexture.GetTemporary(
                    source.width,
                    source.height,
                    0,
                    RenderTextureFormat.Default,
                    RenderTextureReadWrite.Linear);

        Graphics.Blit(source, renderTex);
        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = renderTex;
        Texture2D readableText = new Texture2D(source.width, source.height);
        readableText.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
        readableText.Apply();
        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(renderTex);
        return readableText;
    }
    public static void CheckResolution()//Check for 18:9 ratio
    {
        if (Utils.GetScreenRatio() >= 1.9) //If Yes than expand categories buttons field and scale main canvas
        {
            Utils.getRootTransform().GetComponent<CanvasScaler>().referenceResolution += new Vector2(0, 120f);
        }
    }
    public static Sprite CombineFourPictures(Sprite[] pictures)
    {
        int newSize = 180;
        List<Texture2D> equaledPics = new List<Texture2D>();
        Texture2D result;
        try
        {
            foreach (Sprite item in pictures)
            {
                Sprite temp = item;
                Texture2D texture;
                if (temp == null)
                {
                    temp = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));
                }
                if (temp.texture == null)
                {
                    texture = Texture2D.whiteTexture;
                }
                else
                {
                    texture = Utils.DuplicateTexture(temp.texture);

                }
                if (texture.width != newSize && texture.height != newSize)
                {
                    Texture2D newTex = new Texture2D(texture.width, texture.height, TextureFormat.RGB24, false);
                    newTex.SetPixels(texture.GetPixels());
                    newTex.Apply();
                    TextureScaler.Point(newTex, newSize, newSize);
                    equaledPics.Add(newTex);
                }
                else equaledPics.Add(texture);
            }
            result = new Texture2D(newSize * 2, newSize * 2);

            int maxHeight = result.height;
            int maxWidth = result.width;

            for (int x = 0; x < maxWidth; x++)
            {
                for (int y = 0; y < maxHeight; y++)
                {
                    Color color;
                    if (y < maxHeight / 2)
                    {
                        if (x < maxWidth / 2)
                        {
                            color = equaledPics[0].GetPixel(x, y);
                        }
                        else
                        {
                            color = equaledPics[1].GetPixel(Mathf.Abs(maxWidth / 2 - x), y);
                        }

                    }
                    else
                    {
                        if (x < maxWidth / 2)
                        {

                            color = equaledPics[2].GetPixel(x, Mathf.Abs(maxHeight / 2 - y));
                        }
                        else
                        {
                            color = equaledPics[3].GetPixel(Mathf.Abs(maxWidth / 2 - x), Mathf.Abs(maxHeight / 2 - y));
                        }
                    }
                    result.SetPixel(x, y, color);
                }
            }

            result.Apply();
        }
        catch (UnityException)
        {
            return pictures[0];
        }
        return Sprite.Create(result, new Rect(0, 0, result.width, result.height), new Vector2(0.5f, 0.5f));
    }

}

