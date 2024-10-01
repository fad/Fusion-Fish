using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HudUI : MonoBehaviour
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

    public static HudUI Instance;
    
    private void Awake()
    {
        Instance = this;
    }

    //Here I update the boost sliders where I divide the current stats and the max of it, to get the value for the slider which goes up to 1.
    private void Update()
    {
        if (playerManager == null) 
            return;
        
        //healthUI.fillAmount = playerManager.healthManager.NetworkedHealth / playerManager.healthManager.maxHealth;

        boostUI.fillAmount = playerManager.thirdPersonController.currentBoostCount / playerManager.thirdPersonController.maxBoostCount;
        
        xpUI.fillAmount = (float)playerManager.levelUp.currentExperience / playerManager.levelUp.experienceUntilUpgrade;

        experienceText.text = playerManager.levelUp.currentExperience.ToString();
        
        neededExperienceText.text = playerManager.levelUp.experienceUntilUpgrade.ToString();
    }

    //A button to restart when died
    public void Restart()
    {
        playerManager.healthManager.GetComponent<PlayerHealth>().PlayerRestart();
    }
}
