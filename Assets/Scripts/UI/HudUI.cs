using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class HudUI : MonoBehaviour
{
    [Header("Scripts")] 
    [HideInInspector] public PlayerManager playerManager;
    
    [Header("Boost")]
    [SerializeField] private ProgressBarPro boostUI;

    [Header("Health")]
    [SerializeField] private ProgressBarPro healthUI; 
    
    [Header("Satiety")]
    [SerializeField] private ProgressBarPro satietyUI;
    [SerializeField] private Animator satietyAnim;

    [Header("Death")]
    [SerializeField] private GameObject deathPanel;
    [SerializeField] private TextMeshProUGUI causeOfDeathText;

    [Header("XP")]
  // [SerializeField] public Text experienceText;
  // [SerializeField] public Text neededExperienceText;
    [SerializeField] private ProgressBarPro xpUI;
    [SerializeField] public TextMeshProUGUI levelText;


    [Header("Egg")]
    [SerializeField] private GameObject pressSpaceText;

    [Header("LevelUpText")]
    [SerializeField] private TextMeshProUGUI levelUpText;
    private Vector3 textStartingPosition;
    
    public static HudUI Instance;
    
    private void Awake()
    {
        Instance = this;
    }
    public void OnDeathPanel(string causeOfDeath)
    {
        deathPanel.SetActive(true);
        causeOfDeathText.text = causeOfDeath;
    }
    public void OffDeathPanel()
    {
        deathPanel.SetActive(false);
    }
    private IEnumerator Start()
    {
        yield return new WaitUntil(() => playerManager != null);
        
        playerManager.healthManager.OnHealthChanged += UpdateHealthUI;
        playerManager.levelUp.levelUpEvent += LevelUpTextSpawn;

        textStartingPosition = levelUpText.transform.localPosition;
    }

    private void OnDisable()
    {
        playerManager.healthManager.OnHealthChanged -= UpdateHealthUI;
    }

    //Here I update the boost sliders where I divide the current stats and the max of it, to get the value for the slider which goes up to 1.
    private void Update()
    {
        if (!playerManager) 
            return;

        boostUI.Value = playerManager.thirdPersonController.currentBoostCount / playerManager.thirdPersonController.maxBoostCount;

        float XpPercent = (float)playerManager.levelUp.currentExperience / playerManager.levelUp.experienceUntilUpgrade;
        if (xpUI.Value > XpPercent)
        {
            xpUI.animTime = 100;
        }
        else if(xpUI.Value < XpPercent)
        {
            xpUI.animTime = 0.8f;
        }
        xpUI.Value = XpPercent;

        // experienceText.text = playerManager.levelUp.currentExperience.ToString();
        // neededExperienceText.text = playerManager.levelUp.experienceUntilUpgrade.ToString();

        Satiety();

        levelText.text = "Lvl: "+playerManager.levelUp.currentLevel.ToString();

        if (playerManager.levelUp.isEgg)
            pressSpaceText.SetActive(true);
        else
            pressSpaceText.SetActive(false);
    }

    private void Satiety()
    {
        float Satiety =  playerManager.satietyManager.GetSatiety();
        float MaxSatiety =  playerManager.satietyManager.GetMaxSatiety();

        satietyUI.Value = Satiety / MaxSatiety;
        
        if(Satiety <= (30 * MaxSatiety) / 100f)
            satietyAnim.SetBool("Hunger",true);
        else
            satietyAnim.SetBool("Hunger",false);
    }
    private void UpdateHealthUI(float value)
    {
        healthUI.Value = value / playerManager.healthManager.maxHealth;
    }

    //A button to restart when died
    public void Restart()
    {
        playerManager.healthManager.GetComponent<PlayerHealth>().PlayerRestart();
        UpdateHealthUI(playerManager.healthManager.NetworkedHealth);
    }

    private void LevelUpTextSpawn()
    {
        levelUpText.transform.localPosition = textStartingPosition;
        levelUpText.color = Color.white;
        StartCoroutine(ShowLevelUpText());
    }

    private IEnumerator ShowLevelUpText()
    {
        float alfa = 1;
        while (levelUpText.color.a > 0)
        {
            levelUpText.transform.localPosition += new Vector3(0,1.5f,0);
            levelUpText.color = new Color(1,1,1,alfa);
            alfa -= 0.004f;
            yield return null;
        }
    }
}
