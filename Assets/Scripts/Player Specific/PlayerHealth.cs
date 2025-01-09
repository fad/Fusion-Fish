using UnityEngine;
using Fusion;
using StarterAssets;

public class PlayerHealth : NetworkBehaviour, IHealthUtility
{
    private PlayerManager _playerManager;
    private SpawnGibsOnDestroy _gibsSpawner;
    
    [Header("Death")]
    [HideInInspector] public bool isDead;
    
    [SerializeField] private float showDamageVignetteTime;
    [SerializeField] private float hideDamageVignetteTime;
    [Networked] public bool NetworkedPermanentHealth { get; set; }
    public string causeOfDeath = "You got eaten";


    private void Start()
    {
        _playerManager = GetComponent<PlayerManager>();
        _gibsSpawner = GetComponent<SpawnGibsOnDestroy>();
    }

    public void Die()
    {
        PlayerDeath();
    }
    
    private void PlayerDeath()
    {
        _playerManager.satietyManager.Death();
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        _playerManager.healthManager.PlayParticles(Color.red, 30);
        SetPlayerMeshRpc(false);
        _gibsSpawner.SpawnGibsRpc();
        HudUI.Instance.OnDeathPanel(causeOfDeath);
    }
    
    public void PlayerRestart()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        HudUI.Instance.OffDeathPanel();
        
        _playerManager.levelUp.Restart();
        _playerManager.satietyManager.Restart();
        _playerManager.healthManager.Restart();
        _playerManager.thirdPersonController.currentBoostCount = _playerManager.thirdPersonController.maxBoostCount;
        
        var playerTransform = _playerManager.thirdPersonController.transform;
        GetComponent<PlayerPositionReset>().ResetPlayerPosition();
        _playerManager.thirdPersonController.boostState = ThirdPersonController.BoostState.BoostReload;
        SetPlayerMeshRpc(true);
    }

    [Rpc(RpcSources.All, RpcTargets.All, InvokeLocal = true)]
    private void SetPlayerMeshRpc(bool setActive)
    {
        _playerManager.thirdPersonController.playerVisual.SetActive(setActive);
        _playerManager.thirdPersonController.capsuleCollider.enabled = setActive;
        isDead = !setActive;
    }
}
