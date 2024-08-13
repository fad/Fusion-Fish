using StarterAssets;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI : MonoBehaviour
{
    [Header("Scripts")] 
    [SerializeField] private PlayerManager playerManager;
    
    [Header("Boost")]
    [SerializeField] private Image boostUI;
    
    [Header("Health")]
    [SerializeField] private Image healthUI;

    [Header("XP")]
    [SerializeField] public TextMeshProUGUI experienceText;
    [SerializeField] public TextMeshProUGUI neededExperienceText;
    
    private void Start()
    {
        neededExperienceText.text = playerManager.experience.experienceUntilUpgrade.ToString();
    }

    //Here I update the boost slider where I divide the current boostCount and the max to get the value for the slider which goes up to 1.
    private void Update()
    {
        boostUI.fillAmount = playerManager.thirdPersonController.currentBoostCount / playerManager.thirdPersonController.maxBoostCount;
        healthUI.fillAmount = playerManager.health.currentHealth / playerManager.health.maxHealth;

        experienceText.text = playerManager.experience.currentExperience.ToString();
    }
}
