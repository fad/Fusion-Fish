using Fusion;
using UnityEngine;

public class HealthManager : NetworkBehaviour
{
    [Header("Health")]
    public float maxHealth;
    [Networked] [OnChangedRender(nameof(CheckDeath))] public float NetworkedHealth { get; set; }
    private ParticleSystem bloodParticleSystem;
    [HideInInspector] public bool spawnGibs;
    [HideInInspector] public float currentHealth;
    public bool notAbleToGetBitten;

    [Header("Experience")] 
    public int experienceValue = 100;
    
    [Header("SlowDown")]
    [SerializeField] public float maxSlowDownSpeedTime = 5;
    [HideInInspector] public float slowDownSpeedTime;
    [HideInInspector] public bool slowDown;

    private void Start() => bloodParticleSystem = GameObject.Find("BloodParticles").GetComponent<ParticleSystem>();

    public override void Spawned()
    {
        NetworkedHealth = maxHealth;
        currentHealth = NetworkedHealth;
        slowDownSpeedTime = maxSlowDownSpeedTime;
    }
    
    private void Update()
    {
        if (slowDown)
        {
            slowDownSpeedTime -= Time.deltaTime;
            if (slowDownSpeedTime <= 0)
            {
                slowDown = false;
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
        if(currentHealth >= NetworkedHealth)
        {
            slowDownSpeedTime = maxSlowDownSpeedTime;
            slowDown = true;
            currentHealth = NetworkedHealth;
        }
        
        if (TryGetComponent<PlayerHealth>(out var playerHealth) && HasStateAuthority && currentHealth >= NetworkedHealth)
        {
            if(playerHealth.showVignette)
                StartCoroutine(playerHealth.ShowDamageVignette());
            playerHealth.PlayerCheckDeath();
        }
        else if(TryGetComponent<NPCHealth>(out var npcHealth))
        {
            npcHealth.NPCCheckDeath();
        }
    }

    public void PlayParticles(Color color, int burstCount)
    {
        var mainModule = bloodParticleSystem.main;
        mainModule.startColor = new ParticleSystem.MinMaxGradient(color);

        var emissionModule = bloodParticleSystem.emission;
        emissionModule.SetBursts(new ParticleSystem.Burst[] { new(0.0f, burstCount) });

        var healthObjectTransform = transform;
        var bloodParticleSystemTransform = bloodParticleSystem.transform;
        //need to safe the current parent before it changes parent to revert that parent change
        var parent = bloodParticleSystemTransform.parent;

        bloodParticleSystemTransform.position = healthObjectTransform.position;
        bloodParticleSystemTransform.SetParent(healthObjectTransform);
        bloodParticleSystemTransform.localScale = healthObjectTransform.localScale.z < 1 ? Vector3.one : healthObjectTransform.localScale;
        bloodParticleSystem.Play();
        bloodParticleSystemTransform.SetParent(parent);
    }
}
