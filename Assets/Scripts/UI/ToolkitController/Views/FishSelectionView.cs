using UnityEngine.UIElements;

public class FishSelectionView : UIView
{
    private Button _backButton;
    private Button _startButton;
    
    private const string BackButtonName = "BackButton";
    private const string StartButtonName = "StartButton";
    
    public FishSelectionView(VisualElement topElement) : base(topElement)
    {
    }

    protected override void SetVisualElements()
    {
        base.SetVisualElements();
        _backButton = TopElement.Q<Button>(BackButtonName);
        _startButton = TopElement.Q<Button>(StartButtonName);
    }

    protected override void RegisterButtonCallbacks()
    {
        base.RegisterButtonCallbacks();
        _backButton.RegisterCallback<ClickEvent>(OnBackButtonClicked);
        _startButton.RegisterCallback<ClickEvent>(OnStartButtonClicked);
    }
    
    private void OnBackButtonClicked(ClickEvent evt)
    {
        FishSelectionEvents.OnBackButtonClicked?.Invoke();
    }
    
    private void OnStartButtonClicked(ClickEvent evt)
    {
        FishSelectionEvents.OnStartButtonClicked?.Invoke();
    }
}
