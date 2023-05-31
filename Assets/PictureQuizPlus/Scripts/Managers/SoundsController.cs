using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundsController : MonoBehaviour //Handle music and sounds in the game
{
    public AudioClip[] sounds; //Array of sounds
    public AudioSource musicPlayer; //Source to play music
    public AudioSource soundsPlayer; //Source to play sounds
    public bool isMusic;
    public bool isSounds;
    Dictionary<string, AudioClip> soundsBag;

    public static SoundsController instance { get; private set; }

    void Start()
    {
        instance = this;
        StartCoroutine(Initialize());
    }

    IEnumerator Initialize()
    {
        //string[] newSounds = new string[] {};
        yield return new WaitUntil(() => GameController.Instance != null && GameController.Instance.IsDataReady);

        
        //Get saved values from the registry to set up preferences
        if (PlayerPrefs.GetInt("Music", 1) == 0)
        {
            SetMusic(false);
        }
        if (PlayerPrefs.GetInt("Sounds", 1) == 0)
        {
            SetSound(false);
        }

        soundsBag = new Dictionary<string, AudioClip>(); //Collection of sounds with easy by name access 

        try
        {
            foreach (var item in sounds)
            {
                soundsBag.Add(item.name.Split('.')[0], item);
            }
        }
        catch (System.Exception)
        {
            Debug.LogError("There are no sounds available");
        }

        //AddNewSounds(newSounds);
        DontDestroyOnLoad(gameObject);
        
        musicPlayer.Play();
    }

    public void SetMusic(bool state)
    {
        if (state)
        {
            PlayerPrefs.SetInt("Music", 1);
        }
        else
        {
            PlayerPrefs.SetInt("Music", 0);
        }
        musicPlayer.mute = !state;
        isMusic = state;
    }

    public void SetSound(bool state)
    {
        if (state)
        {
            PlayerPrefs.SetInt("Sounds", 1);
        }
        else PlayerPrefs.SetInt("Sounds", 0);
        soundsPlayer.mute = !state;
        isSounds = state;
    }

    public void PlaySound(string name) //Main method to play sounds from list
    {
        try
        {
            soundsPlayer.PlayOneShot(soundsBag[name]);
        }
        catch (System.Exception)
        {
            Debug.Log($"no sound {name}");
        }
    }

    public void PlaySound(string name, float volume) //Overload with ability to set volume for the required sound
    {
        try
        {
            soundsPlayer.PlayOneShot(soundsBag[name], volume);
        }
        catch (System.Exception)
        {
            Debug.Log($"no sound {name}");
        }
    }

    public void AddNewSounds(string[] names) //Add sounds from Resources/NewTracks folder
    {
        AudioClip temp;
        foreach (var item in names)
        {
            temp = Resources.Load("NewTracks/" + item) as AudioClip;
            soundsBag.Add(item, temp);
        }
    }

    public void StopAllSounds()
    {
        soundsPlayer.Stop();
    }
}
