using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using TMPro;

public class AudioSliderHandler : MonoBehaviour
{
    [Header("Audio Slider Game Objects")]
    [SerializeField]
    private Slider audioSlider;
    
    [SerializeField]
    private TextMeshProUGUI audioValueText;
    
    [Header("Audio Settings")]
    [SerializeField]
    private AudioMixerGroup audioMixerGroup;

    private void OnEnable()
    {
        audioSlider.onValueChanged.AddListener(ChangeToNewValue);
    }
    
    private void Start()
    {
        SetValues();
    }

    private void OnDisable()
    {
        audioSlider.onValueChanged.RemoveListener(ChangeToNewValue);
    }

    private void SetValues()
    {
        audioSlider.minValue = 0f;
        audioSlider.maxValue = 100f;
        
        float savedValue = PlayerPrefs.GetFloat(audioMixerGroup.name, 80f);
        audioSlider.value = savedValue;
        audioValueText.text = savedValue.ToString("F0") + "%"; // F0 rounds the float to the nearest whole number
    }

    private void ChangeToNewValue(float newValue)
    {
        audioValueText.text = newValue.ToString("F0") + "%";
        PlayerPrefs.SetFloat(audioMixerGroup.name, newValue);
        audioMixerGroup.audioMixer.SetFloat(audioMixerGroup.name, ConvertPercentageToDb(newValue));
        
    }
    
    /// <summary>
    /// Converts the percentage to a db value and returns it
    /// </summary>
    /// <param name="percentage">The percentage value</param>
    /// <returns>The db value for the audio mixer group</returns>
    public static float ConvertPercentageToDb(float percentage)
    {
        if (percentage <= 0)
        {
            return -80;
        }
        
        return Mathf.Log10(percentage / 100) * 20;
    }
}
