using System.Collections;
using TMPro;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;
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

    [Header("Experience")]
  // [SerializeField] public Text experienceText;
  // [SerializeField] public Text neededExperienceText;
    [SerializeField] private ProgressBarPro xpUI;
    [SerializeField] public TextMeshProUGUI levelText;
    [SerializeField] private MoveExperience ExperienceTextObj;
    [SerializeField] private Transform SpawnPoint;
    private float currentTimeToSpawn,intervalBetweenSpawn = 0.1f;


    [Header("Egg")]
    [SerializeField] private GameObject pressSpaceText;

    [Header("LevelUpText")]
    [SerializeField] private TextMeshProUGUI levelUpText;
    private Vector3 textStartingPosition;
    private Coroutine showLevelUpText;



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
        playerManager.levelUp.AddExperienceEvent += AddExperience;

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

        ExperienceLogic();
        Satiety();
        LevelLogic();
    }

    
    private void ExperienceLogic()
    {

        currentTimeToSpawn -= Time.deltaTime;

        float XpPercent = (float)playerManager.levelUp.GetExperience() / playerManager.levelUp.experienceUntilUpgrade;
        if (xpUI.Value > XpPercent)
        {
            xpUI.animTime = 100;
        }
        else if(xpUI.Value < XpPercent)
        {
            xpUI.animTime = 0.8f;
        }
        xpUI.Value = XpPercent;
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
    private void LevelLogic()
    {
            levelText.text = "Lvl: "+playerManager.levelUp.currentLevel.ToString();

        if (playerManager.levelUp.isEgg)
            pressSpaceText.SetActive(true);
        else
            pressSpaceText.SetActive(false);
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

        if(showLevelUpText!= null)
            StopCoroutine(showLevelUpText);

        showLevelUpText = StartCoroutine(ShowLevelUpText());
    }

     private void AddExperience(int Experience)
    {
        if(currentTimeToSpawn < 0)
        {
            MoveExperience experienceEntity = Instantiate(ExperienceTextObj,transform);
            experienceEntity.transform.position = SpawnPoint.position + new Vector3(Random.Range(0,50),Random.Range(0,50),0);
            experienceEntity.MoveToPosition(xpUI.transform.localPosition,Experience);
            currentTimeToSpawn = intervalBetweenSpawn;
        }
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
