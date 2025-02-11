using UnityEngine;

public class MainMenuPresenter : MonoBehaviour
{
    [SerializeField]
    private URLData discordURL;
    
    [SerializeField]
    private URLData feedbackURL;

    private void OnEnable()
    {
        MainMenuEvents.OnDiscordButtonClicked += OpenDiscord;
        MainMenuEvents.OnFeedbackButtonClicked += OpenFeedback;
        MainMenuEvents.OnQuitButtonClicked += CloseApp;
        
    }

    private void OnDisable()
    {
        MainMenuEvents.OnDiscordButtonClicked -= OpenDiscord;
        MainMenuEvents.OnFeedbackButtonClicked -= OpenFeedback;
        MainMenuEvents.OnQuitButtonClicked -= CloseApp;
    }
    
    private void OpenFeedback()
    {
        OpenURL(feedbackURL?.url);
    }

    private void OpenDiscord()
    {
        OpenURL(discordURL?.url);
    }

    private static void CloseApp()
    {
        Application.Quit();
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
