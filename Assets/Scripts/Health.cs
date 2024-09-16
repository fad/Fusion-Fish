using BiggestFish.Gameplay;
using Fusion;
using StarterAssets;
using UnityEngine;

public class Health : NetworkBehaviour
{
    [Header("Health")]
    public float maxHealth;

    [Networked] public float NetworkedHealth { get; set; } = 5;
    private ParticleSystem bloodParticleSystem;
    private ThirdPersonController thirdPersonController;
    [HideInInspector] public bool isPlayer;

    [Header("Experience")] 
    public int experienceValue = 100;
    
    [Header("Death")]
    [HideInInspector] public bool isDead;
    private bool healthToMaxHealth;

    private void Start()
    {
        if (TryGetComponent(out ThirdPersonController thirdPersonControllerTemp))
        {
            isPlayer = true;
            thirdPersonController = thirdPersonControllerTemp;
        }
        
        bloodParticleSystem = GameObject.Find("BloodParticles").GetComponent<ParticleSystem>();
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
    public void ReceiveDamageRpc(float damage, bool spawnGibs)
    {
        NetworkedHealth -= damage;
        
        if (NetworkedHealth <= 0)
        {
            if (isPlayer)
            {
                PlayParticles(Color.red, 30);
            }
            else
            {
                if (GetComponent<NPC>())
                {
                    FindObjectOfType<NPCSpawner>().SpawnFish();
                }
                else
                {
                    //FindObjectOfType<FoodSpawner>().SpawnFood();
                }
                
                PlayParticles(Color.red, 30);

                if (TryGetComponent<SpawnGibsOnDestroy>(out var spawnGibsOnDestroy) && !spawnGibs)
                {
                    spawnGibsOnDestroy.gibSpawnCount = 0;
                }
                
                Destroy(gameObject);
            }
        }
        else
        {
            PlayParticles(Color.red, 10);
        }
    }

    public void PlayerDeath()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        thirdPersonController.playerMesh.SetActive(false);
        GetComponent<SpawnGibsOnDestroy>().SpawnMeatObjectsRpc(Runner);
        isDead = true;
        UI.Instance.deathPanel.SetActive(true);
    }
    
    public void Restart()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        UI.Instance.deathPanel.SetActive(false);
        isDead = false;
        NetworkedHealth = maxHealth;
        
        thirdPersonController.playerManager.experience.currentExperience = 0;

        var playerTransform = thirdPersonController.transform;
        playerTransform.position = new Vector3(0, 0, 0);
        playerTransform.localScale = new Vector3(.5f, .5f, .5f);
        thirdPersonController.currentBoostCount = 0;
        thirdPersonController.boostState = ThirdPersonController.BoostState.BoostReload;
        thirdPersonController.playerMesh.SetActive(true);
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
