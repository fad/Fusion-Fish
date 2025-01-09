using System.Collections;
using Fusion;
using IngameDebugConsole;
using StarterAssets;
using UnityEngine;

public class PlayerManager : NetworkBehaviour
{
    public ThirdPersonController thirdPersonController;
    [HideInInspector] public HealthManager healthManager;
    [HideInInspector] public LevelUp levelUp;
    [HideInInspector] public PlayerHealth playerHealth;
    [HideInInspector] public SatietyManager satietyManager;
    [HideInInspector] public SlowDownManager slowDownManager;
    [SerializeField] public PlayerAttack playerAttack;
    [HideInInspector] public NetworkRunner hostPlayerRunner;
    [HideInInspector] public EntityDataContainer entityDataContainer;

    private void Awake()
    {
        thirdPersonController = GetComponent<ThirdPersonController>();
        healthManager = GetComponent<HealthManager>();
        levelUp = GetComponent<LevelUp>();
        playerHealth = GetComponent<PlayerHealth>();
        satietyManager = GetComponent<SatietyManager>();
        slowDownManager = GetComponent<SlowDownManager>();
        entityDataContainer = GetComponent<EntityDataContainer>();
    }
    
    private IEnumerator Start()
    {
        foreach (var unused in Runner.ActivePlayers)
        {
            if (GetComponent<NetworkObject>().HasStateAuthority)
            {
                hostPlayerRunner = Runner;
            }
        }
        
        yield return new WaitUntil(() => HudUI.Instance != null);
        
        if (HasStateAuthority)
        {
            HudUI.Instance.playerManager = this;
            DebugLogManager.Instance.GetComponent<InGameDebugConsoleManager>().playerManager = this;
        }
    }
}
