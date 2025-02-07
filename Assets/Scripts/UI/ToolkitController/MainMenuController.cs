using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class MainMenuController : MonoBehaviour
{
    [Header("UI related"), SerializeField]
    private UIDocument ui;
    
    [Header("URL Data"), Space(10), SerializeField]
    private URLData discordURL;
    
    [SerializeField]
    private URLData feedbackURL;
    
    private VisualElement _root;
    
    
    #region Feedback Button Names
    private const string FeedbackButtonName = "FeedbackButton";
    private const string DiscordButtonName = "DiscordButton";
    #endregion
    
    #region Main Button Names
    private const string SingleplayerButtonName = "SingleplayerButton";
    private const string MultiplayerButtonName = "MultiplayerButton";
    private const string SettingsButtonName = "SettingsButton";
    private const string QuitButtonName = "QuitButton";
    #endregion
    
    private void Awake()
    {
        if(!ui) return;
        
        _root = ui.rootVisualElement;
    }
    
    private void OnEnable()
    {
        if(!ui) return;
        
        AddSoundsToButtons();
        AddFeedbackButtonsFunctionality();
        AddMainButtonFunctionality();
    }
    
    /// <summary>
    /// Adds a click sound to every button in the UI Document.
    /// </summary>
    private void AddSoundsToButtons()
    {
        List<Button> buttons = _root.Query<Button>().ToList();
        
        foreach (var button in buttons)
        {
            button.clicked += () => AudioManager.Instance.Play("click");
        }
    }

    /// <summary>
    /// Adds functionality to the feedback buttons.
    /// </summary>
    private void AddFeedbackButtonsFunctionality()
    {
        AddButtonFunctionality(DiscordButtonName, () => OpenURL(discordURL?.url));
        AddButtonFunctionality(FeedbackButtonName, () => OpenURL(feedbackURL?.url));
    }

    /// <summary>
    /// Adds functionality to the main buttons.
    /// </summary>
    private void AddMainButtonFunctionality()
    {
        AddButtonFunctionality(QuitButtonName, Application.Quit);
    }
    
    /// <summary>
    /// Adds functionality on click to a button.
    /// </summary>
    /// <param name="buttonName">The name of the button to give functionality to.</param>
    /// <param name="action">The action to be invoked on clicked.</param>
    private void AddButtonFunctionality(string buttonName, Action action)
    {
        Button button = _root.Q<Button>(buttonName);
        button.clicked += action;
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
