using UnityEngine;
using UnityEngine.UIElements;

public class MainMenuView : UIView
{
    Button _singleplayerButton;
    Button _multiplayerButton;
    Button _settingsButton;
    Button _quitButton;
    
    Button _feedbackButton;
    Button _discordButton;
    
    #region Main Button Names
    private const string SingleplayerButtonName = "SingleplayerButton";
    private const string MultiplayerButtonName = "MultiplayerButton";
    private const string SettingsButtonName = "SettingsButton";
    private const string QuitButtonName = "QuitButton";
    #endregion
    
    #region Feedback Button Names
    private const string FeedbackButtonName = "FeedbackButton";
    private const string DiscordButtonName = "DiscordButton";
    #endregion
    
    public MainMenuView(VisualElement topElement) : base(topElement)
    {

    }

    protected override void SetVisualElements()
    {
        base.SetVisualElements();
        _singleplayerButton = TopElement.Q<Button>(SingleplayerButtonName);
        _multiplayerButton = TopElement.Q<Button>(MultiplayerButtonName);
        _settingsButton = TopElement.Q<Button>(SettingsButtonName);
        _quitButton = TopElement.Q<Button>(QuitButtonName);
        
        _feedbackButton = TopElement.Q<Button>(FeedbackButtonName);
        _discordButton = TopElement.Q<Button>(DiscordButtonName);
    }

    protected override void RegisterButtonCallbacks()
    {
        _feedbackButton.RegisterCallback<ClickEvent>(OnFeedbackButtonClicked);
        _discordButton.RegisterCallback<ClickEvent>(OnDiscordButtonClicked);
        
        _singleplayerButton.RegisterCallback<ClickEvent>(OnSingleplayerButtonClicked);
        _multiplayerButton.RegisterCallback<ClickEvent>(OnMultiplayerButtonClicked);
        _settingsButton.RegisterCallback<ClickEvent>(OnSettingsButtonClicked);
        _quitButton.RegisterCallback<ClickEvent>(OnQuitButtonClicked);
        
    }

    public override void Dispose()
    {
        base.Dispose();
        
        _feedbackButton.UnregisterCallback<ClickEvent>(OnFeedbackButtonClicked);
        _discordButton.UnregisterCallback<ClickEvent>(OnDiscordButtonClicked);
        
        _quitButton.UnregisterCallback<ClickEvent>(OnQuitButtonClicked);
    }
    
    
    private void OnSettingsButtonClicked(ClickEvent evt)
    {
        MainMenuEvents.OnSettingsButtonClicked?.Invoke();
    }

    private void OnMultiplayerButtonClicked(ClickEvent evt)
    {
        MainMenuEvents.OnMultiplayerButtonClicked?.Invoke();
    }

    private void OnSingleplayerButtonClicked(ClickEvent evt)
    {
        MainMenuEvents.OnSingleplayerButtonClicked?.Invoke();
    }

    private void OnQuitButtonClicked(ClickEvent evt)
    {
        MainMenuEvents.OnQuitButtonClicked?.Invoke();
    }

    private void OnDiscordButtonClicked(ClickEvent evt)
    {
        MainMenuEvents.OnDiscordButtonClicked?.Invoke();
    }

    private void OnFeedbackButtonClicked(ClickEvent evt)
    {
        MainMenuEvents.OnFeedbackButtonClicked?.Invoke();
    }

}
