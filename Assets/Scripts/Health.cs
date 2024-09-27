using System;
using AvocadoShark;
using BiggestFish.Gameplay;
using Fusion;
using StarterAssets;
using UnityEngine;

public class Health : NetworkBehaviour
{
    [Header("Health")]
    public float maxHealth;
    public bool notAbleToGetBitten;
    [Networked] [OnChangedRender(nameof(CheckDeath))] public float NetworkedHealth { get; set; } = 5;
    private ParticleSystem bloodParticleSystem;
    private ThirdPersonController thirdPersonController;
    [HideInInspector] public bool isPlayer;
    private bool spawnGibs;
    private float currentHealth;
    [SerializeField] public bool isShrimp;
    [SerializeField] public bool isStarFish;

    [Header("Experience")] 
    public int experienceValue = 100;
    
    [Header("SlowDown")]
    [HideInInspector] public bool slowPlayerDown;
    [HideInInspector] public bool slowNPCDown;
    private float slowDownSpeedTime = 5;
    [SerializeField] private float maxSlowDownSpeedTime = 5;
    
    [Header("Death")]
    [HideInInspector] public bool isDead;
    private bool healthToMaxHealth;
    [SerializeField] private GameObject playerName;
    [SerializeField] private GameObject playerVisual;

    private void Start()
    {
        if (TryGetComponent(out ThirdPersonController thirdPersonControllerTemp))
        {
            isPlayer = true;
            thirdPersonController = thirdPersonControllerTemp;
        }
        
        bloodParticleSystem = GameObject.Find("BloodParticles").GetComponent<ParticleSystem>();
        slowDownSpeedTime = maxSlowDownSpeedTime;
        currentHealth = maxHealth;
    }

    public override void Spawned()
    {
        NetworkedHealth = maxHealth;
    }

    public override void FixedUpdateNetwork()
    {
        if (!healthToMaxHealth)
        {
            NetworkedHealth = maxHealth;
            healthToMaxHealth = true;
        }
    }

    private void Update()
    {
        if (slowPlayerDown && HasStateAuthority)
        {
            slowDownSpeedTime -= Time.deltaTime;
            if (slowDownSpeedTime <= 0)
            {
                slowPlayerDown = false;
            }
        }
        else if (slowNPCDown)
        {
            slowDownSpeedTime -= Time.deltaTime;
            if (slowDownSpeedTime <= 0)
            {
                slowNPCDown = false;
            }
        }
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void ReceiveDamageRpc(float damage, bool spawnGibsOnDestroy)
    {
        NetworkedHealth -= damage;
        
        spawnGibs = spawnGibsOnDestroy;

        if (NetworkedHealth > 0)
        {
            PlayParticles(Color.red, 10);
        }
    }

    private void CheckDeath()
    {
        if (isPlayer && HasStateAuthority)
        {
            if (NetworkedHealth <= 0)
            {
                PlayerDeath();
            }
            else if(currentHealth >= NetworkedHealth)
            {
                slowDownSpeedTime = maxSlowDownSpeedTime;
                slowPlayerDown = true;
                currentHealth = NetworkedHealth;
            }
        }
        else if(!isPlayer)
        {
            if (NetworkedHealth <= 0)
            {
                if (TryGetComponent<SpawnGibsOnDestroy>(out var spawnGibsOnDestroy) && spawnGibs)
                {
                    spawnGibsOnDestroy.spawnGibs = true;
                }
            
                NPCDeathRpc();
            }
            else if(currentHealth >= NetworkedHealth)
            {
                slowDownSpeedTime = maxSlowDownSpeedTime;
                slowNPCDown = true;
                currentHealth = NetworkedHealth;
            }
        }
    }
    
    [Rpc(RpcSources.All, RpcTargets.All, InvokeLocal = true)]
    private void NPCDeathRpc()
    {
        PlayParticles(Color.red, 30);

        Destroy(gameObject);
    }

    private void PlayerDeath()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        PlayParticles(Color.red, 30);
        GetComponent<SpawnGibsOnDestroy>().SpawnMeatObjects(Runner);
        SetPlayerMeshRpc(false);
        isDead = true;
        HudUI.Instance.deathPanel.SetActive(true);
    }

    [Rpc(RpcSources.All, RpcTargets.All, InvokeLocal = true)]
    private void SetPlayerMeshRpc(bool setActive)
    {
        playerVisual.SetActive(setActive);
        playerName.SetActive(setActive);
        thirdPersonController.capsuleCollider.enabled = setActive;
    }
    
    public void Restart()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        HudUI.Instance.deathPanel.SetActive(false);
        isDead = false;
        
        thirdPersonController.playerManager.levelUp.currentExperience = thirdPersonController.playerManager.levelUp.startingExperience;
        thirdPersonController.playerManager.levelUp.experienceUntilUpgrade = thirdPersonController.playerManager.levelUp.startingExperienceUntilUpgrade;

        thirdPersonController.playerManager.attack.suckPower = thirdPersonController.playerManager.levelUp.startingSuckPower;
        thirdPersonController.playerManager.attack.attackDamage = thirdPersonController.playerManager.levelUp.startingAttackDamage;
        thirdPersonController.cameraDistance = thirdPersonController.playerManager.levelUp.startingCameraDistance;
        thirdPersonController.defaultSwimSpeed = thirdPersonController.playerManager.levelUp.startingDefaultSwimSpeed;
        thirdPersonController.boostSwimSpeed = thirdPersonController.playerManager.levelUp.startingBoostSwimSpeed;
        thirdPersonController.playerManager.attack.attackRange = thirdPersonController.playerManager.levelUp.startingAttackRange;
        thirdPersonController.playerManager.health.maxHealth = thirdPersonController.playerManager.levelUp.startingHealth;

        NetworkedHealth = maxHealth;
        thirdPersonController.currentBoostCount = thirdPersonController.maxBoostCount;
        
        var playerTransform = thirdPersonController.transform;
        GetComponent<PlayerPosResetter>().ResetPlayerPosition();
        playerTransform.localScale = thirdPersonController.playerManager.levelUp.startingSize;
        thirdPersonController.currentBoostCount = 0;
        thirdPersonController.boostState = ThirdPersonController.BoostState.BoostReload;
        SetPlayerMeshRpc(true);
    }

    //Here I change burst count and color when needed
    private void PlayParticles(Color color, int burstCount)
    {
        var mainModule = bloodParticleSystem.main;
        mainModule.startColor = new ParticleSystem.MinMaxGradient(color);

        var emissionModule = bloodParticleSystem.emission;
        emissionModule.SetBursts(new ParticleSystem.Burst[] { new(0.0f, burstCount) });

        var healthObject = transform;
        bloodParticleSystem.transform.position = healthObject.position;
        var parent = bloodParticleSystem.transform.parent;
        bloodParticleSystem.transform.SetParent(healthObject);
        bloodParticleSystem.Play();
        bloodParticleSystem.transform.SetParent(parent);
    }
}
