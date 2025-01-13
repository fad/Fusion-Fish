using UnityEngine;
using UnityEngine.Audio;

public class SetAudioMixerVolumes : MonoBehaviour
{
    [SerializeField]
    private AudioMixer audioMixer;
    
    private void Start()
    {
        SetValues();
    }

    /// <summary>
    /// Sets all the audio mixer values to the saved values at the start of the game
    /// </summary>
    private void SetValues()
    {
        foreach(AudioMixerGroup audioMixerGroup in audioMixer.FindMatchingGroups(""))
        {
            float savedValue = PlayerPrefs.GetFloat(audioMixerGroup.name, 80f);
            audioMixer.SetFloat(audioMixerGroup.name, ConvertPercentageToDb(savedValue));
        }
    }
    
    /// <summary>
    /// Converts the percentage to a db value and returns it
    /// </summary>
    /// <param name="percentage">The percentage value</param>
    /// <returns>The db value for the audio mixer group</returns>
    public static float ConvertPercentageToDb(float percentage)
    {
        return Mathf.Log10(percentage / 100) * 20;
    }
}
