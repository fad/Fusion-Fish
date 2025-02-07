using UnityEngine;
using UnityEngine.UIElements;

[UxmlElement("AudioSlider")]
public partial class AudioSlider : VisualElement
{
    private readonly Slider _slider;
    private readonly Label _valueLabel;

    private const string SliderName = "Slider";
    private const string ValueLabelName = "ValueLabel";

    [UxmlAttribute]
    public string SliderText
    {
        get => _sliderText;
        set
        {
            _sliderText = value;
            _slider.label = value;
        }
    }

    [UxmlAttribute]
    public float LowValue
    {
        get => _lowValue;
        set
        {
            _lowValue = value;
            _slider.lowValue = value;
        }
    }

    [UxmlAttribute]
    public float HighValue
    {
        get => _highValue;
        set
        {
            _highValue = value;
            _slider.highValue = value;
        }
    }

    [UxmlAttribute]
    public float Value
    {
        get => _value;
        set
        {
            _value = value;
            _slider.value = value;
            _valueLabel.text = value.ToString("F0");
        }
    }
    
    private string _sliderText;
    private float _lowValue;
    private float _highValue;
    private float _value;

    public AudioSlider()
    {
        _slider = new Slider();
        _slider.name = SliderName;
        
        _valueLabel = new Label();
        _valueLabel.name = ValueLabelName;

        _slider.RegisterValueChangedCallback(evt => UpdateLabel(evt.newValue));
        
        Add(_slider); // adds a slider to the visual element
        Add(_valueLabel); // adds a label to the visual element
        InitValues();
    }

    private void InitValues()
    {
        _slider.label = SliderText;
        
        _slider.value = Value;
        _slider.lowValue = LowValue;
        _slider.highValue = HighValue;
        
        
        _valueLabel.text = Value.ToString();
    }

    private void UpdateLabel(float newValue)
    {
        Value = newValue;
        _valueLabel.text = newValue.ToString("F0");
    }
}
