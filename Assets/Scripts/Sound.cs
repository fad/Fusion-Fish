using UnityEngine;
using UnityEngine.Audio;

[System.Serializable]
public class Sound
{
    //This class is for the array in audio manager which holds everything that I need to play a sound effect
    
    public string name;

    public AudioClip clip;
    public AudioMixerGroup audioMixer;

    [Range(0f, 1f)] 
    public float volume;
    
    [HideInInspector] public float pitch;
    
    public bool loop;
    
    [HideInInspector] public AudioSource audioSource;
}