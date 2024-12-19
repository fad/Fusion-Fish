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
    [SerializeField]
    private AudioSource audioSourcePrefab;

    [SerializeField]
    private int initialPoolSize = 10;

    [SerializeField]
    private int maxPoolSize = 256;

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
            createFunc: OnCreateAudioSource,
            actionOnGet: OnGetSource,
            actionOnRelease: OnReleaseSource,
            actionOnDestroy: OnDestroySource,
            collectionCheck: false,
            defaultCapacity: initialPoolSize,
            maxSize: maxPoolSize
        );
    }


    /// <summary>
    /// Creates a new AudioSource instance from the prefab and sets its parent to the AudioManager.
    /// </summary>
    /// <returns>A new AudioSource instance.</returns>
    private AudioSource OnCreateAudioSource()
    {
        return Instantiate(audioSourcePrefab, transform);
    }

    /// <summary>
    /// Activates the AudioSource when it is retrieved from the pool.
    /// </summary>
    /// <param name="src">The AudioSource to activate.</param>
    private void OnGetSource(AudioSource src)
    {
        src.gameObject.SetActive(true);
    }

    /// <summary>
    /// Stops the AudioSource and deactivates it when it is released back to the pool.
    /// </summary>
    /// <param name="src">The AudioSource to deactivate.</param>
    private void OnReleaseSource(AudioSource src)
    {
        src.Stop();
        src.gameObject.SetActive(false);
    }

    /// <summary>
    /// Destroys the AudioSource when it is removed from the pool.
    /// </summary>
    /// <param name="src">The AudioSource to destroy.</param>
    private void OnDestroySource(AudioSource src)
    {
        Destroy(src.gameObject);
    }

    /// <summary>
    /// Returns the AudioSource to the pool after it finishes playing.
    /// </summary>
    /// <param name="src">The AudioSource to return to the pool.</param>
    /// <returns>An IEnumerator for the coroutine.</returns>
    private IEnumerator ReturnToPoolAfterPlay(AudioSource src)
    {
        yield return new WaitForSeconds(src.clip.length);
        _audioSourcePool.Release(src);
    }

    /// <summary>
    /// Plays an AudioClip at a specified position with a specified volume.
    /// </summary>
    /// <param name="clip">The AudioClip to play.</param>
    /// <param name="position">The position to play the AudioClip at.</param>
    /// <param name="volume">The volume to play the AudioClip at. Default is 1f.</param>
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
