using UnityEngine;
using System;
using System.Collections;
using Random = UnityEngine.Random;

public class AudioManager : MonoBehaviour
{
    //A variable for every sound that is in the game
    public Sound[] sounds;

    public static AudioManager Instance;
    
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

    private void Start()
    {
        Play("underwaterAmbience");
        Play("OutOfWaterLake");
        PauseSound("OutOfWaterLake");
        Play("OutOfWaterOcean");
        PauseSound("OutOfWaterOcean");
    }

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
    
    public void PauseSound(string soundName)
    {
        var s = Array.Find(sounds, sound => sound.name == soundName);
        if (s == null)
        {
            Debug.LogWarning("Sound:" + soundName + "not found!");
            return;
        }
        s.audioSource.Pause();
    }
    
    public void UnPauseSound(string soundName)
    {
        var s = Array.Find(sounds, sound => sound.name == soundName);
        if (s == null)
        {
            Debug.LogWarning("Sound:" + soundName + "not found!");
            return;
        }
        s.audioSource.UnPause();
    }
    
    public void StopSound(string soundName)
    {
        var s = Array.Find(sounds, sound => sound.name == soundName);
        if (s == null)
        {
            Debug.LogWarning("Sound:" + soundName + "not found!");
            return;
        }
        s.audioSource.Stop();
    }
    
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
        StopCoroutine(VolumeFadeIn(soundEffect));
        StopCoroutine(VolumeFadeOut(soundEffect));
        
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

        while (s.audioSource.volume > 0.1f)
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
        StopCoroutine(VolumeFadeIn(soundEffect));
        StopCoroutine(VolumeFadeOut(soundEffect));
        
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
