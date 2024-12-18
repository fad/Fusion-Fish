using UnityEngine;
using System;
using System.Collections;
using UnityEngine.Pool;
using Random = UnityEngine.Random;

public class AudioManager : MonoBehaviour
{
    [Header("Sounds")]
    //A variable for every sound that is in the game
    public Sound[] sounds;
    
    [Header("Object Pooling")]
    [SerializeField] private AudioSource audioSourcePrefab;
    [SerializeField] private int initialPoolSize = 10;
    [SerializeField] private int maxPoolSize = 256;

    public static AudioManager Instance;

    private ObjectPool<AudioSource> _audioSourcePool;
    
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
        
        InitializePool();
    }

    private void Start()
    {
        Play("underwaterAmbience");
        Play("OutOfWaterLake");
        PauseSound("OutOfWaterLake");
        Play("OutOfWaterOcean");
        PauseSound("OutOfWaterOcean");
    }

    private void InitializePool()
    {
        _audioSourcePool = new ObjectPool<AudioSource>(
            createFunc: CreateAudioSource,
            actionOnGet: OnGetSource,
            actionOnRelease: ReturnToPool,
            actionOnDestroy: DestroySource,
            collectionCheck: false,
            defaultCapacity: initialPoolSize,
            maxSize: maxPoolSize
        );
    }

    private AudioSource CreateAudioSource()
    {
        return Instantiate(audioSourcePrefab, transform);
    }

    private void OnGetSource(AudioSource src)
    {
        src.gameObject.SetActive(true);
    }

    private void ReturnToPool(AudioSource src)
    {
        src.Stop();
        src.gameObject.SetActive(false);
    }

    private void DestroySource(AudioSource src)
    {
        Destroy(src.gameObject);
    }

    private IEnumerator ReturnToPoolAfterPlay(AudioSource src)
    {
        yield return new WaitForSeconds(src.clip.length);
        _audioSourcePool.Release(src);
    }

    private void PlayClipAtPosition(AudioClip clip, Vector3 position, float volume = 1f)
    {
        AudioSource src = _audioSourcePool.Get();
        src.transform.position = position;
        src.volume = volume;
        src.clip = clip;
       
        src.Play();
        StartCoroutine(ReturnToPoolAfterPlay(src));
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
        
        PlayClipAtPosition(s.clip, audioPosition);
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
        
        PlayClipAtPosition(s.clip, audioPosition);
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
