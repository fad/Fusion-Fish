using UnityEngine;
using IngameDebugConsole;
using UnityEngine.UI;
using Button = UnityEngine.UI.Button;

public class InGameDebugConsoleManager : MonoBehaviour
{
    [SerializeField] private KeyCode keyX = KeyCode.X;
    [SerializeField] private KeyCode keyC = KeyCode.C;

    [SerializeField] private GameObject cheats;
    [SerializeField] private Toggle permanentStamina;
    [SerializeField] private Toggle permanentHealth;
    [SerializeField] private Button levelUpButton;
    [HideInInspector] public PlayerManager playerManager;
    private DebugLogManager debugConsole;
    private const float CheckInterval = 0.15f;
    private float nextCheckTime;

    private void Start() => debugConsole = gameObject.GetComponent<DebugLogManager>();

    void Update()
    {
        if (!(Time.time >= nextCheckTime)) 
            return;
        
        var activate = (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) && Input.GetKey(keyX) && Input.GetKey(keyC);

        if (activate)
        {
            if (debugConsole.IsLogWindowVisible)
            {
                debugConsole.HideLogWindow();
                cheats.SetActive(false);
            }
            else
            {
                debugConsole.ShowLogWindow();
                cheats.SetActive(true);
                if (playerManager != null)
                {
                    permanentStamina.interactable = true;
                    permanentHealth.interactable = true;
                    levelUpButton.interactable = true;
                    Cursor.visible = true;
                    Cursor.lockState = CursorLockMode.None;
                }
                else
                {
                    permanentStamina.interactable = false;
                    permanentHealth.interactable = false;
                    levelUpButton.interactable = false;
                    Cursor.visible = false;
                    Cursor.lockState = CursorLockMode.Locked;
                }
            }
        }
        
        nextCheckTime = Time.time + CheckInterval;
    }

    public void TogglePermanentStamina(bool isToggled)
    {
        playerManager.thirdPersonController.permanentStamina = isToggled;
    }
    
    public void TogglePermanentHealth(bool isToggled)
    {
        playerManager.playerHealth.NetworkedPermanentHealth = isToggled;
    }
    
    public void TriggerLevelUp()
    {
        playerManager.levelUp.currentExperience = playerManager.levelUp.experienceUntilUpgrade;
        playerManager.levelUp.CheckLevelUp();
    }
}
