using UnityEngine.UIElements;

public class MultiplayerMenuView : UIView
{
    private Button _backButton;
    private Button _hostGameButton;
    private Button _findGameButton;
    
    private const string BackButtonName = "BackButton";
    private const string HostGameButtonName = "HostButton";
    private const string FindGameButtonName = "FindButton";
    
    public MultiplayerMenuView(VisualElement topElement) : base(topElement)
    {
    }

    protected override void SetVisualElements()
    {
        base.SetVisualElements();
        _backButton = TopElement.Q<Button>(BackButtonName);
        _hostGameButton = TopElement.Q<Button>(HostGameButtonName);
        _findGameButton = TopElement.Q<Button>(FindGameButtonName);
    }

    protected override void RegisterButtonCallbacks()
    {
        _backButton.RegisterCallback<ClickEvent>(OnBackButtonClicked);
        _hostGameButton.RegisterCallback<ClickEvent>(OnHostGameButtonClicked);
        _findGameButton.RegisterCallback<ClickEvent>(OnFindGameButtonClicked);
    }

    private void OnBackButtonClicked(ClickEvent evt)
    {
        MultiplayerMenuEvents.OnBackButtonClicked?.Invoke();
    }

    private void OnHostGameButtonClicked(ClickEvent evt)
    {
        MultiplayerMenuEvents.OnHostGameButtonClicked?.Invoke();
    }

    private void OnFindGameButtonClicked(ClickEvent evt)
    {
        MultiplayerMenuEvents.OnFindGameButtonClicked?.Invoke();
    }
}
