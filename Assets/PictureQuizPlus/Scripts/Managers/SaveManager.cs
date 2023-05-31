using System;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

public class SaveManager //Handle save data interactions
{
    DataToSave saveData; //Main structure that stores all loaded or created game data and ready to be saved

    string path;
    //Load saved data
    public SaveManager()
    {
        path = System.IO.Path.Combine(Application.persistentDataPath, "saves.json");
        saveData = LoadDataFromJson();
    }
    //Check the save file exists
    public bool Check(string path)
    {
        return File.Exists(path);
    }

    //Save prepared data
    public void Save()
    {
        SaveDataToJson();
    }

    private DataToSave LoadDataFromJson()
    {
        if (Check(path))
        {
            string raw = File.ReadAllText(path);
            if (!GameController.Instance.DebugButton)
            {
                try
                {
                    return DecryptJson(raw, path);
                }
                catch (System.Exception)
                {
                    try
                    {
                        EncryptJson(raw, path);
                        return LoadDataFromJson();
                    }
                    catch (System.Exception)
                    {
                        return new DataToSave(GameController.Instance.StartCoins);
                    }

                }

            }
            else
            {
                try
                {
                    return JsonUtility.FromJson<DataToSave>(raw);
                }
                catch (System.Exception)
                {
                    try
                    {
                        return DecryptJson(raw, path);
                    }
                    catch (System.Exception)
                    {
                        return new DataToSave(GameController.Instance.StartCoins);
                    }
                }
            }
        }
        else
        {
            return new DataToSave(GameController.Instance.StartCoins);
        }
    }


    private void EncryptJson(string json, string path)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(json);
        string encrypt = BitConverter.ToString(bytes);
        StreamWriter sw = File.CreateText(path);
        sw.Close();
        File.WriteAllText(path, encrypt.Replace("-", ""));
    }

    private DataToSave DecryptJson(string raw, string path)
    {
        int chars = raw.Length;
        byte[] bytes = new byte[chars / 2];
        for (int i = 0; i < chars; i += 2)
        {
            bytes[i / 2] = Convert.ToByte(raw.Substring(i, 2), 16);
        }
        string json = Encoding.UTF8.GetString(bytes);
        DataToSave result = JsonUtility.FromJson<DataToSave>(json);
        return result;
    }

    private void SaveDataToJson()
    {
        string json = JsonUtility.ToJson(saveData, true);
        if (!GameController.Instance.DebugButton)
        {
            EncryptJson(json, path);
        }
        else
        {
            StreamWriter sw = File.CreateText(path);
            sw.Close();
            File.WriteAllText(path, json);
        }
#if GP_SAVES
        if (GooglePlaySaves.Instance != null && GooglePlaySaves.Instance.Authenticated)
        {
            GooglePlaySaves.Instance.ToSaveString(json);
            GooglePlaySaves.Instance.SaveToCloud();
        }
#endif
    }

    //Write fresh level states to the data structure
    public void SetDirectoryState(LevelInfo current)
    {
        saveData.levelsInfo.RemoveAll(t => t.directoryName == current.directoryName); //Replace old data with new
        saveData.levelsInfo.Add(current);
    }
    //Returns the requested directory state
    public LevelInfo GetDirectoryState(string dirName)
    {
        if (saveData.levelsInfo.Count != 0)
        {
            foreach (var item in saveData.levelsInfo)
            {
                if (item.directoryName == dirName)
                {
                    return item;
                }
            }
        }
        return new LevelInfo() { currentLevel = 1, directoryName = dirName, isNewRecord = true };
    }

    //Handle coins changes
    public void SetCoinsData(int coins)
    {
        saveData.coinsCount = coins;
    }

    public int GetCoins()
    {
        return saveData.coinsCount;
    }

    public string[] GetUnlockedDirectories()
    {
        var request = from x in saveData.levelsInfo
                      select x.directoryName;
        return request.ToArray();
    }

    public bool IsCategorySaved(string name)
    {
        return saveData.levelsInfo.Any(l => l.directoryName == name);
    }

    public void ResetLetters()
    {
        saveData.levelsInfo = saveData.levelsInfo
            .Select(l => { l.lettersOppened = 0; return l; }).ToList();

    }

    public void SetAds(bool value)
    {
        saveData.ads = value;
    }

    public bool GetAds()
    {
        return saveData.ads;
    }

    internal bool Compare(string saveString)
    {
        DataToSave dataFromGP = StringToStruct(saveString);
        return saveData.Equals(dataFromGP);
    }

    public DataToSave StringToStruct(string str)
    {
        return JsonUtility.FromJson<DataToSave>(str);
    }

}
