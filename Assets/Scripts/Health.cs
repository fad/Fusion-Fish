using AvocadoShark;
using BiggestFish.Gameplay;
using Fusion;
using StarterAssets;
using UnityEngine;

public class Health : NetworkBehaviour, ISpawned
{
    [Header("Health")]
    public float maxHealth;

    [Networked] [OnChangedRender(nameof(CheckDeath))] public float NetworkedHealth { get; private set; } = 5;
    private ParticleSystem bloodParticleSystem;
    private ThirdPersonController thirdPersonController;
    [HideInInspector] public bool isPlayer;
    private bool spawnGibs;

    [Header("Experience")] 
    public int experienceValue = 100;
    
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
        if (NetworkedHealth <= 0)
        {
            if (isPlayer && HasStateAuthority)
            {
                PlayerDeath();
            }
            else if(!isPlayer)
            {
                if (TryGetComponent<SpawnGibsOnDestroy>(out var spawnGibsOnDestroy) && spawnGibs)
                {
                    spawnGibsOnDestroy.spawnGibs = true;
                }
                
                NPCDeathRpc();
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
        NetworkedHealth = maxHealth;
        
        thirdPersonController.playerManager.experience.currentExperience = thirdPersonController.playerManager.experience.startingExperience;
        thirdPersonController.playerManager.experience.experienceUntilUpgrade = thirdPersonController.playerManager.experience.startingExperienceUntilUpgrade;

        thirdPersonController.playerManager.attack.suckPower = thirdPersonController.playerManager.experience.startingSuckPower;
        thirdPersonController.playerManager.attack.attackDamage = thirdPersonController.playerManager.experience.startingAttackDamage;
        thirdPersonController.cameraDistance = thirdPersonController.playerManager.experience.startingCameraDistance;
        thirdPersonController.defaultSwimSpeed = thirdPersonController.playerManager.experience.startingDefaultSwimSpeed;
        thirdPersonController.boostSwimSpeed = thirdPersonController.playerManager.experience.startingBoostSwimSpeed;

        var playerTransform = thirdPersonController.transform;
        GetComponent<PlayerPosResetter>().ResetPlayerPosition();
        playerTransform.localScale = thirdPersonController.playerManager.experience.startingSize;
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
        
        bloodParticleSystem.transform.position = transform.position;
        bloodParticleSystem.Play();
    }
}
