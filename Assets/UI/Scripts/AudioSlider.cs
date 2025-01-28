using UnityEngine;
using UnityEngine.UIElements;

[UxmlElement("AudioSlider")]
public partial class AudioSlider : VisualElement
{
    private readonly Slider _slider;
    private readonly Label _valueLabel;
    
    [UxmlAttribute]
    public string SliderText { get; set; }
    
    [UxmlAttribute]
    public float LowValue { get; set; }
    
    [UxmlAttribute]
    public float HighValue { get; set; }
    
    [UxmlAttribute]
    public float Value { get; set; }

    public AudioSlider()
    {
        _slider = new Slider();
        Add(_slider); // adds a slider to the visual element
        
        _valueLabel = new Label();
        Add(_valueLabel); // adds a label to the visual element

        RegisterCallback<ChangeEvent<float>>(OnSliderValueChanged);
        RegisterCallback<ChangeEvent<float>>(OnHighValueChanged);
        RegisterCallback<ChangeEvent<float>>(OnLowValueChanged);
        RegisterCallback<ChangeEvent<string>>(OnSliderTextValueChanged);
        
        Update();
    }

    private void Update()
    {
        _slider.label = SliderText;
        
        _slider.value = Value;
        _slider.lowValue = LowValue;
        _slider.highValue = HighValue;
        
        
        _valueLabel.text = Value.ToString();
    }
    
    private void OnSliderValueChanged(ChangeEvent<float> evt)
    {
        Value = evt.newValue;
        _valueLabel.text = Value.ToString();
    }
    
    public void OnSliderTextValueChanged(ChangeEvent<string> evt)
    {
        SliderText = evt.newValue;
        _slider.label = SliderText;
    }
    
    public void OnLowValueChanged(ChangeEvent<float> evt)
    {
        LowValue = evt.newValue;
        _slider.lowValue = LowValue;
    }
    
    public void OnHighValueChanged(ChangeEvent<float> evt)
    {
        HighValue = evt.newValue;
        _slider.highValue = HighValue;
    }
}
