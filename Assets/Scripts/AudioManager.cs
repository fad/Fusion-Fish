using UnityEngine;
using System;
using System.Collections;
using Random = UnityEngine.Random;

public class AudioManager : MonoBehaviour
{
    //A variable for every sound that is for the game
    public Sound[] sounds;

    public static AudioManager Instance;
    
    //I make this a singleton class and delete it if already existing, then I apply every option for the sound that is 
    //adjustable in the inspector
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        foreach (var sound in sounds)
        {
            sound.audioSource = gameObject.AddComponent<AudioSource>();
            sound.audioSource.clip = sound.clip;

            sound.audioSource.volume = sound.volume;

            sound.audioSource.loop = sound.loop;
            
            sound.audioSource.pitch = sound.pitch;
            
            sound.audioSource.outputAudioMixerGroup = sound.audioMixer;
        }
    }

    //Here I Search for a sound in the sound array that has the according string as a name with lambda method, then I play the sound
    public void Play(string soundName)
    {
        var s = Array.Find(sounds, sound => sound.name == soundName);
        if (s == null)
        {
            Debug.LogWarning("Sound:" + soundName + "not found!");
            return;
        }
        
        s.audioSource.Play();
    }
    
    //Here I Search for a sound in the sound array that has the according string as a name with lambda method, then I play the sound
    public void PlaySoundAtPosition(string soundName, Vector3 audioPosition)
    {
        var s = Array.Find(sounds, sound => sound.name == soundName);
        if (s == null)
        {
            Debug.LogWarning("Sound:" + soundName + "not found!");
            return;
        }
        
        AudioSource.PlayClipAtPoint(s.clip, audioPosition);
    }
    
    //Here I Search for a sound in the sound array that has the according string as a name with lambda method, then I play the sound
    public void PlaySoundWithRandomPitchAtPosition(string soundName, Vector3 audioPosition)
    {
        var s = Array.Find(sounds, sound => sound.name == soundName);
        if (s == null)
        {
            Debug.LogWarning("Sound:" + soundName + "not found!");
            return;
        }

        s.pitch = Random.Range(0.15f, 0.9f);
        
        AudioSource.PlayClipAtPoint(s.clip, audioPosition);
    }
    
    //Here the same as play but with stop
    public void Stop(string soundName)
    {
        var s = Array.Find(sounds, sound => sound.name == soundName);
        if (s == null)
        {
            Debug.LogWarning("Sound:" + soundName + "not found!");
            return;
        }
        s.audioSource.Stop();
    }
    
    //I am asking with a string if a sound is playing right now
    public bool IsPlaying(string soundName)
    {
        var s = Array.Find(sounds, sound => sound.name == soundName);
        if (s == null)
        {
            Debug.LogWarning("Sound:" + soundName + "not found!");
            return false;
        }
        return s.audioSource.isPlaying;
    }

    #region VolumeFadeOut

    public void FadeOut(string soundEffect)
    {
        StartCoroutine(VolumeFadeOut(soundEffect));
    }
    
    //Fades the sound out with lerp of volume
    private IEnumerator VolumeFadeOut(string soundName)
    {
        var s = Array.Find(sounds, sound => sound.name == soundName);
        if (s == null)
        {
            Debug.LogWarning("Sound:" + soundName + "not found!");
            yield break;        
        }

        while (s.audioSource.volume > 0.01f)
        {
            s.audioSource.volume = Mathf.Lerp(s.audioSource.volume, 0, Time.deltaTime * 2);
            yield return null;
        }

        s.audioSource.volume = 0;
    }

    #endregion

    #region VolumeFadeIn

    public void FadeIn(string soundEffect)
    {
        StartCoroutine(VolumeFadeIn(soundEffect));
    }
    
    //Fades the sound in with lerp of volume
    private IEnumerator VolumeFadeIn(string soundName)
    {
        var s = Array.Find(sounds, sound => sound.name == soundName);
        if (s == null)
        {
            Debug.LogWarning("Sound:" + soundName + "not found!");
            yield break;        
        }

        while (s.audioSource.volume < .9f)
        {
            s.audioSource.volume = Mathf.Lerp(s.audioSource.volume, 1, Time.deltaTime / 2);
            yield return null;
        }
    }

    #endregion
}
