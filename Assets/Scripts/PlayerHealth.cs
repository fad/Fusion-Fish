using System.Collections;
using UnityEngine;
using Fusion;
using StarterAssets;
using UnityEngine.Rendering;

public class PlayerHealth : NetworkBehaviour
{
    private PlayerManager playerManager;
    
    [Header("Death")]
    [HideInInspector] public bool isDead;
    
    private Volume hitVignette;
    [HideInInspector] public bool showVignette;
    [SerializeField] private float showDamageVignetteTime;
    [SerializeField] private float hideDamageVignetteTime;

    private void Start()
    {
        playerManager = GetComponent<PlayerManager>();
        hitVignette = GameObject.Find("PostProcessingDamage").GetComponent<Volume>();
        showVignette = true;
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
    
    public IEnumerator ShowDamageVignette()
    {
        showVignette = false;

        hitVignette.priority = 2;
        var elapsedTime = 0f;

        while (elapsedTime < showDamageVignetteTime)
        {
            hitVignette.weight = Mathf.Lerp(hitVignette.weight, 1, elapsedTime / showDamageVignetteTime);
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        elapsedTime = 0;
        
        while (elapsedTime < hideDamageVignetteTime)
        {
            hitVignette.weight = Mathf.Lerp(hitVignette.weight, 0, elapsedTime / hideDamageVignetteTime);
        
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        hitVignette.priority = 0;
        showVignette = true;
    }

    [Rpc(RpcSources.All, RpcTargets.All, InvokeLocal = true)]
    private void SetPlayerMeshRpc(bool setActive)
    {
        playerManager.thirdPersonController.playerVisual.SetActive(setActive);
        playerManager.thirdPersonController.capsuleCollider.enabled = setActive;
        isDead = !setActive;
    }
}