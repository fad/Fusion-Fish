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
    [Networked] public bool NetworkedPermanentHealth { get; set; }
    public string causeOfDeath = "You got eaten";

    private SpawnGibsOnDestroy _gibsSpawner;

    private void Start()
    {
        playerManager = GetComponent<PlayerManager>();
        _gibsSpawner = GetComponent<SpawnGibsOnDestroy>();
        hitVignette = GameObject.Find("DamagePostProcessing").GetComponent<Volume>();
        showVignette = true;
    }

    public void Die()
    {
        PlayerDeath();
    }
    
    private void PlayerDeath()
    {
        playerManager.satietyManager.Death();
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        playerManager.healthManager.PlayParticles(Color.red, 30);
        _gibsSpawner.SpawnMeatObjects(Runner);
        SetPlayerMeshRpc(false);
        HudUI.Instance.OnDeathPanel(causeOfDeath);
    }
    
    public void PlayerRestart()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        HudUI.Instance.OffDeathPanel();
        
        playerManager.levelUp.Restart();

        playerManager.playerAttack.suckInDamage = playerManager.levelUp.startingSuckPower;
        playerManager.playerAttack.attackDamage = playerManager.levelUp.startingAttackDamage;
        playerManager.thirdPersonController.cameraDistance = playerManager.levelUp.startingCameraDistance;
        playerManager.thirdPersonController.defaultSwimSpeed = playerManager.levelUp.startingDefaultSwimSpeed;
        playerManager.thirdPersonController.boostSwimSpeed = playerManager.levelUp.startingBoostSwimSpeed;
        playerManager.playerAttack.attackRange = playerManager.levelUp.startingAttackRange;
        playerManager.healthManager.maxHealth = playerManager.levelUp.startingHealth;

        playerManager.satietyManager.Restart();
        playerManager.healthManager.Restart();
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
