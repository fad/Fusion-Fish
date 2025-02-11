using UnityEngine.UIElements;

public class ModeSelectionView : UIView
{
    private Button _backButton;
    private Button _nextButton;
    
    private const string BackButtonName = "BackButton";
    private const string NextButtonName = "NextButton";
    
    public ModeSelectionView(VisualElement topElement) : base(topElement)
    {
    }
    
    protected override void SetVisualElements()
    {
        base.SetVisualElements();
        _backButton = TopElement.Q<Button>(BackButtonName);
        _nextButton = TopElement.Q<Button>(NextButtonName);
    }
    
    protected override void RegisterButtonCallbacks()
    {
        _backButton.RegisterCallback<ClickEvent>(OnBackButtonClicked);
        _nextButton.RegisterCallback<ClickEvent>(OnNextButtonClicked);
    }

    private void OnNextButtonClicked(ClickEvent evt)
    {
        ModeSelectionEvents.OnNextButtonClicked?.Invoke();
    }

    private void OnBackButtonClicked(ClickEvent evt)
    {
        ModeSelectionEvents.OnBackButtonClicked?.Invoke();
    }
}
