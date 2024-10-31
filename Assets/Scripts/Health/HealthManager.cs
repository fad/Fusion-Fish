using System;
using System.Collections;
using Fusion;
using Unity.VisualScripting;
using UnityEngine;

public class HealthManager : NetworkBehaviour, IHealthManager
{
    [Header("Health")]
    public float maxHealth;
    [SerializeField] private float recoveryHealthInSecond = 10;
    [SerializeField] private float timeToStartRecoveryHealth = 3;
    private bool _regeneration = false;
    [Networked] private TickTimer regenTimer { get; set; }

    [Networked] [OnChangedRender(nameof(CheckDeath))] public float NetworkedHealth { get; set; }
    private ParticleSystem bloodParticleSystem;
    [HideInInspector] public bool spawnGibs; // TODO: Break this up in own class
    [HideInInspector] public float currentHealth;
    public bool notAbleToGetBitten;

    [Header("Experience")] // TODO: Break this up in own class
    public int experienceValue = 100;
    
    [Header("SlowDown")] // TODO: Break this up in own class
    [SerializeField] public float maxSlowDownSpeedTime = 5;
    [HideInInspector] public float slowDownSpeedTime;
    [HideInInspector] public bool slowDown;
    
    public event Action<float> OnHealthChanged;
    public event Action OnDeath;

    private bool _hasSpawned = false;
    private bool _died = false;
    
    public bool Died => _died;

    private void Start() => bloodParticleSystem = GameObject.Find("BloodParticles").GetComponent<ParticleSystem>();

    public override void Spawned()
    {
        Restart();
    }
    public void Restart()
    {
        NetworkedHealth = maxHealth;
        currentHealth = NetworkedHealth;
        slowDownSpeedTime = maxSlowDownSpeedTime;
        _hasSpawned = true;
    }
    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        _hasSpawned = false;
    }
    
    private void OnDisable()
    {
        _hasSpawned = false;
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

    public override void FixedUpdateNetwork()
    {
        if (_regeneration && regenTimer.ExpiredOrNotRunning(Runner))
        {
            RecoveryHealthRpc(recoveryHealthInSecond);
            regenTimer = TickTimer.CreateFromSeconds(Runner, 1);

            if (NetworkedHealth >= maxHealth)
                _regeneration = false;
        }
    }
    private void startPassiveRecoveryHealth()
    {
        regenTimer = TickTimer.CreateFromSeconds(Runner, timeToStartRecoveryHealth);
        _regeneration = true;
    }
    
    public void Damage(float amount)
    {
        if (notAbleToGetBitten) return;
        
        ReceiveDamageRpc(amount, true);
        CheckDeath();
    }
    
    public void Heal(float amount)
    {
        RecoveryHealthRpc(amount);
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RecoveryHealthRpc(float amountHealthRestored)
    {
        NetworkedHealth += amountHealthRestored;
        NetworkedHealth = Mathf.Min(NetworkedHealth, maxHealth);
        currentHealth = NetworkedHealth;
        OnHealthChanged?.Invoke(NetworkedHealth);
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void ReceiveDamageRpc(float damage, bool spawnGibsOnDestroy)
    {
        NetworkedHealth -= damage;
        
        spawnGibs = spawnGibsOnDestroy;

        if (NetworkedHealth > 0)
        {
            startPassiveRecoveryHealth();
            PlayParticles(Color.red, 10);
        }
    }

    private void CheckDeath()
    {
        if(!_hasSpawned) return;
        
        if (TryGetComponent<PlayerHealth>(out var playerHealth) && HasStateAuthority && currentHealth > NetworkedHealth)
        {
            if(playerHealth.showVignette)
                StartCoroutine(playerHealth.ShowDamageVignette());

            if (NetworkedHealth <= 0)
            {
                playerHealth.Die();
                OnDeath?.Invoke();
            }
        }
        else if(TryGetComponent<NPCHealth>(out var npcHealth))
        {
            if (NetworkedHealth <= 0)
            {
                npcHealth.Die();
                OnDeath?.Invoke();
            }
            
        }

        if (currentHealth > NetworkedHealth)
        {
            slowDownSpeedTime = maxSlowDownSpeedTime;
            slowDown = true;
            currentHealth = NetworkedHealth;
            OnHealthChanged?.Invoke(currentHealth);
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
