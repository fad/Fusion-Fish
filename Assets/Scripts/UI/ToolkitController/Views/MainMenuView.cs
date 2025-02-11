using System.Collections;
using System.Collections.Generic;
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
    
    URLData _discordURL;
    URLData _feedbackFormURL;
    
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
    
    public MainMenuView(VisualElement topElement, URLData discordURL, URLData feedbackFormURL) : base(topElement)
    {
        _discordURL = discordURL;
        _feedbackFormURL = feedbackFormURL;
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
        
        _quitButton.RegisterCallback<ClickEvent>(OnQuitButtonClicked);
    }

    public override void Dispose()
    {
        base.Dispose();
        
        _feedbackButton.UnregisterCallback<ClickEvent>(OnFeedbackButtonClicked);
        _discordButton.UnregisterCallback<ClickEvent>(OnDiscordButtonClicked);
        
        _quitButton.UnregisterCallback<ClickEvent>(OnQuitButtonClicked);
    }

    private void OnQuitButtonClicked(ClickEvent evt)
    {
        Application.Quit();
    }

    private void OnDiscordButtonClicked(ClickEvent evt)
    {
        OpenURL(_discordURL?.url);
    }

    private void OnFeedbackButtonClicked(ClickEvent evt)
    {
        OpenURL(_feedbackFormURL?.url);
    }

    /// <summary>
    /// Checks whether the URL is set and opens it.
    /// </summary>
    /// <param name="url">The URL to open.</param>
    private void OpenURL(string url)
    {
        if (!string.IsNullOrEmpty(url))
        {
            Application.OpenURL(url);
        }
        else
        {
            Debug.LogWarning("URL not set!");
        }
    }

}
