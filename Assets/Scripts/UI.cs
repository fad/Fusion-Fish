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
    [SerializeField] private Image xpUI;

    public static UI Instance;
    
    private void Awake()
    {
        Instance = this;
    }

    //Here I update the boost slider where I divide the current boostCount and the max to get the value for the slider which goes up to 1.
    private void Update()
    {
        if (playerManager == null) 
            return;
        
        healthUI.fillAmount = playerManager.health.NetworkedHealth / playerManager.health.maxHealth;

        boostUI.fillAmount = playerManager.thirdPersonController.currentBoostCount / playerManager.thirdPersonController.maxBoostCount;
        
        xpUI.fillAmount = (float)playerManager.experience.currentExperience / playerManager.experience.experienceUntilUpgrade;

        experienceText.text = playerManager.experience.currentExperience.ToString();
        
        neededExperienceText.text = playerManager.experience.experienceUntilUpgrade.ToString();
    }

    public void Restart()
    {
        playerManager.health.Restart();
    }
}
