
#if GP_SAVES
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using GooglePlayGames.BasicApi.SavedGame;
using System;
using System.Text;
using UnityEngine;

public class GooglePlaySaves : MonoBehaviour
{
    public static GooglePlaySaves Instance;
    public bool written = false;
    //keep track of saving or loading during callbacks.
    private bool isSaving;
    //Save name
    private static string saveName = "game_save";
    //This is the saved file
    private string saveString = "";
    public event Action<bool> OnAuth;
    //check with GPG (or other*) if user is authenticated. *e.g. GameCenter
    public bool Authenticated
    {
        get
        {
            return Social.Active.localUser.authenticated;
        }
    }

    public bool Loading
    {
        get; private set;
    }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }

    }

    public void Initialize()
    {
        Loading = true;
        // ...
        // Create client configuration with saved games enabled
        PlayGamesClientConfiguration config = new PlayGamesClientConfiguration.Builder()
            .EnableSavedGames()
            .Build();
        PlayGamesPlatform.InitializeInstance(config);
        PlayGamesPlatform.Activate();
        Social.localUser.Authenticate((success, info) =>
        {
            Loading = false;
            if(OnAuth != null) {
                OnAuth(success);
            }
            if (success)
            {
                Debug.LogWarning("AUTHENTICATED");
                LoadFromCloud();
            }
            else
            {
                Debug.LogWarning("NOT AUTHENTICATED");
                Debug.LogWarning(info);
            }
        });
    }

    //merges loaded bytearray with old save
    private void WriteCloudDataToSaveString(byte[] cloudData)
    {
        if (cloudData != null)
        {
            // Debug.Log("Decoding cloud data from bytes.");
            string progress = FromBytes(cloudData);
            // Debug.Log("Merging with existing game progress.");
            ToSaveString(progress);
        }
    }
    //load save from cloud
    public void LoadFromCloud()
    {
        // Debug.Log("Loading game progress from the cloud.");
        isSaving = false;
        written = false;
        ((PlayGamesPlatform)Social.Active).SavedGame.OpenWithAutomaticConflictResolution(
            saveName, //name of file.
            DataSource.ReadCacheOrNetwork,
            ConflictResolutionStrategy.UseLongestPlaytime,
            SavedGameOpened);
    }

    //overwrites old file or saves a new one
    public void SaveToCloud()
    {
        if (Authenticated)
        {
            // Debug.Log("Saving progress to the cloud... filename: " + saveName);
            isSaving = true;
            written = false;
            //save to named file
            ((PlayGamesPlatform)Social.Active).SavedGame.OpenWithAutomaticConflictResolution(
                saveName, //name of file. If save doesn't exist it will be created with this name
                DataSource.ReadCacheOrNetwork,
                ConflictResolutionStrategy.UseLongestPlaytime,
                SavedGameOpened);
        }
        else
        {
            // Debug.LogWarning("NOT AUTHENTICATED");
        }
    }
    //save is opened, either save or load it.
    private void SavedGameOpened(SavedGameRequestStatus status, ISavedGameMetadata game)
    {
        //check success
        if (status == SavedGameRequestStatus.Success)
        {
            //saving
            if (isSaving)
            {
                //read bytes from save
                byte[] data = ToBytes();
                //create builder. here you can add play time, time created etc for UI.
                SavedGameMetadataUpdate.Builder builder = new SavedGameMetadataUpdate.Builder();
                SavedGameMetadataUpdate updatedMetadata = builder.Build();
                //saving to cloud
                Debug.LogWarning("SAVING TO GooglePlaySavesS IN PROCESS");
                ((PlayGamesPlatform)Social.Active).SavedGame.CommitUpdate(game, updatedMetadata, data, SavedGameWritten);
                //loading
            }
            else
            {
                ((PlayGamesPlatform)Social.Active).SavedGame.ReadBinaryData(game, SavedGameLoaded);
            }
            //error
        }
        else
        {
            //Debug.LogWarning("Error opening game: " + status);
        }
    }
    //callback from SavedGameOpened. Check if loading result was successful or not.
    private void SavedGameLoaded(SavedGameRequestStatus status, byte[] data)
    {
        if (status == SavedGameRequestStatus.Success)
        {
            //Debug.LogWarning("SaveGameLoaded, success = " + status);
            WriteCloudDataToSaveString(data);
            GameController.Instance.CheckIsPopupNeeded(saveString);
        }
        else
        {
            //Debug.LogWarning("Error reading game: " + status);
        }
    }
    //callback from SavedGameOpened. Check if saving result was successful or not.
    private void SavedGameWritten(SavedGameRequestStatus status, ISavedGameMetadata game)
    {
        if (status == SavedGameRequestStatus.Success)
        {
            written = true;
            isSaving = false;
            Debug.LogWarning("GAME " + game.Description + " WRITTEN");
        }
        else
        {
            // Debug.LogWarning("Error saving game: " + status);
        }
    }
    //merge local save with cloud save. Here is where you change the merging betweeen cloud and local save for your setup.
    public void ToSaveString(string other)
    {
        if (!string.IsNullOrEmpty(other))
        {
            saveString = other;
        }
    }
    //return saveString as bytes
    private byte[] ToBytes()
    {
        byte[] bytes = Encoding.UTF8.GetBytes(saveString);
        return bytes;
    }
    //take bytes as arg and return string
    private string FromBytes(byte[] bytes)
    {
        string decodedString = Encoding.UTF8.GetString(bytes);
        return decodedString;
    }
}
#endif
