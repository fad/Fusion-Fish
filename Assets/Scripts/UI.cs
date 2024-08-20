using System.Collections;
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
    
    private IEnumerator Start()
    {
        yield return new WaitUntil(() => neededExperienceText != null);
        
        neededExperienceText.text = playerManager.experience.experienceUntilUpgrade.ToString();
    }

    //Here I update the boost slider where I divide the current boostCount and the max to get the value for the slider which goes up to 1.
    private void Update()
    {
        if (healthUI == null) 
            return;
        
        healthUI.fillAmount = playerManager.health.currentHealth / playerManager.health.maxHealth;

        boostUI.fillAmount = playerManager.thirdPersonController.currentBoostCount / playerManager.thirdPersonController.maxBoostCount;

        experienceText.text = playerManager.experience.currentExperience.ToString();
    }

    public void Restart()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        playerManager.health.deathPanel.SetActive(false);
        playerManager.health.isDead = false;
        playerManager.health.currentHealth = playerManager.health.maxHealth;
        
        playerManager.experience.currentExperience = 0;

        var playerTransform = playerManager.thirdPersonController.transform;
        playerTransform.position = new Vector3(0, 0, 0);
        playerTransform.localScale = new Vector3(1, 1, 1);
        playerManager.thirdPersonController.currentBoostCount = 0;
        playerManager.thirdPersonController.boostState = ThirdPersonController.BoostState.BoostReload;
        playerManager.thirdPersonController.playerMesh.SetActive(true);
    }
}
