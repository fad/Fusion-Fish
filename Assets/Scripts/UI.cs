using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI : MonoBehaviour
{
    [Header("Scripts")] 
    [HideInInspector] public PlayerManager playerManager;
    
    [Header("Boost")]
    [SerializeField] private Image boostUI;
    
    [Header("Health")]
    [SerializeField] private Image healthUI;

    [Header("Death")] 
    public GameObject deathPanel;

    [Header("XP")]
    [SerializeField] public TextMeshProUGUI experienceText;
    [SerializeField] public TextMeshProUGUI neededExperienceText;

    public static UI Instance;
    
    private void Awake()
    {
        Instance = this;
    }

    private IEnumerator Start()
    {
        yield return new WaitUntil(() => playerManager != null);
        
        neededExperienceText.text = playerManager.experience.experienceUntilUpgrade.ToString();
    }

    //Here I update the boost slider where I divide the current boostCount and the max to get the value for the slider which goes up to 1.
    private void Update()
    {
        if (healthUI == null || playerManager == null) 
            return;
        
        healthUI.fillAmount = playerManager.health.NetworkedHealth / playerManager.health.maxHealth;

        boostUI.fillAmount = playerManager.thirdPersonController.currentBoostCount / playerManager.thirdPersonController.maxBoostCount;

        experienceText.text = playerManager.experience.currentExperience.ToString();
    }

    public void Restart()
    {
        playerManager.health.Restart();
    }
}
