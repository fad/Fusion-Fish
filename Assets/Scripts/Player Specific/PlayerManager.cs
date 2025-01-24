using System.Collections;
using Fusion;
using IngameDebugConsole;
using StarterAssets;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerManager : NetworkBehaviour
{
    public ThirdPersonController thirdPersonController;

    [HideInInspector]
    public HealthManager healthManager;

    [HideInInspector]
    public LevelUp levelUp;

    [HideInInspector]
    public PlayerHealth playerHealth;

    [HideInInspector]
    public SatietyManager satietyManager;

    [HideInInspector]
    public SlowDownManager slowDownManager;

    [SerializeField]
    public PlayerAttack playerAttack;

    [HideInInspector]
    public NetworkRunner hostPlayerRunner;

    [HideInInspector]
    public EntityDataContainer entityDataContainer;

    [HideInInspector]
    public SpawnGibsOnDestroy spawnGibsOnDestroy;

    [SerializeField,
     Tooltip(
         "The objects that should be set to the player layer when the player is spawned.\nUsed for overriding the materials.")]
    private GameObject[] playerRender;

    private const string StateAuthorityPlayerLayer = "StateAuthorityPlayer";

    private void Awake()
    {
        thirdPersonController = GetComponent<ThirdPersonController>();
        healthManager = GetComponent<HealthManager>();
        levelUp = GetComponent<LevelUp>();
        playerHealth = GetComponent<PlayerHealth>();
        satietyManager = GetComponent<SatietyManager>();
        slowDownManager = GetComponent<SlowDownManager>();
        entityDataContainer = GetComponent<EntityDataContainer>();
        spawnGibsOnDestroy = GetComponent<SpawnGibsOnDestroy>();
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

    public override void Spawned()
    {
        if (!HasStateAuthority) return;

        gameObject.layer = LayerMask.NameToLayer(StateAuthorityPlayerLayer);

        if (playerRender is null || playerRender.Length <= 0) return;

        foreach (GameObject renderObject in playerRender)
        {
            renderObject.layer = LayerMask.NameToLayer(StateAuthorityPlayerLayer);
        }
    }
}
