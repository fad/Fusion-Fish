using UnityEngine;
using Fusion;
using StarterAssets;

public class PlayerHealth : NetworkBehaviour
{
    private PlayerManager playerManager;
    
    [Header("Death")]
    [HideInInspector] public bool isDead;

    private void Start()
    {
        playerManager = GetComponent<PlayerManager>();
    }

    public void PlayerCheckDeath()
    {
        if (playerManager.healthManager.NetworkedHealth <= 0)
        {
            PlayerDeath();
        }
    }
    
    private void PlayerDeath()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        playerManager.healthManager.PlayParticles(Color.red, 30);
        GetComponent<SpawnGibsOnDestroy>().SpawnMeatObjects(Runner);
        SetPlayerMeshRpc(false);
        HudUI.Instance.deathPanel.SetActive(true);
    }
    
    public void PlayerRestart()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        HudUI.Instance.deathPanel.SetActive(false);
        
        playerManager.levelUp.currentExperience = playerManager.levelUp.startingExperience;
        playerManager.levelUp.experienceUntilUpgrade = playerManager.levelUp.startingExperienceUntilUpgrade;

        playerManager.playerAttack.suckInDamage = playerManager.levelUp.startingSuckPower;
        playerManager.playerAttack.attackDamage = playerManager.levelUp.startingAttackDamage;
        playerManager.thirdPersonController.cameraDistance = playerManager.levelUp.startingCameraDistance;
        playerManager.thirdPersonController.defaultSwimSpeed = playerManager.levelUp.startingDefaultSwimSpeed;
        playerManager.thirdPersonController.boostSwimSpeed = playerManager.levelUp.startingBoostSwimSpeed;
        playerManager.playerAttack.attackRange = playerManager.levelUp.startingAttackRange;
        playerManager.healthManager.maxHealth = playerManager.levelUp.startingHealth;

        playerManager.healthManager.NetworkedHealth = playerManager.healthManager.maxHealth;
        playerManager.thirdPersonController.currentBoostCount = playerManager.thirdPersonController.maxBoostCount;
        
        var playerTransform = playerManager.thirdPersonController.transform;
        GetComponent<PlayerPositionReset>().ResetPlayerPosition();
        playerTransform.localScale = playerManager.levelUp.startingSize;
        playerManager.thirdPersonController.boostState = ThirdPersonController.BoostState.BoostReload;
        SetPlayerMeshRpc(true);
    }

    [Rpc(RpcSources.All, RpcTargets.All, InvokeLocal = true)]
    private void SetPlayerMeshRpc(bool setActive)
    {
        playerManager.thirdPersonController.playerVisual.SetActive(setActive);
        playerManager.thirdPersonController.capsuleCollider.enabled = setActive;
        isDead = !setActive;
    }
}
