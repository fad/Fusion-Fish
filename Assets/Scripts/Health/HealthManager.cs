using System;
using Fusion;
using UnityEngine;

public class HealthManager : NetworkBehaviour, IHealthManager, ISuckable, IGraspable
{
    [Header("Data Container")]
    [SerializeField]
    private FishData fishData;

    [Header("Health"), Space(10)]
    public float maxHealth;

    public float recoveryHealthInSecond = 10;
    public float timeToStartRecoveryHealth = 3;
    private bool _regeneration = false;

    [Networked]
    private TickTimer RegenTimer { get; set; }

    [Networked]
    public float NetworkedHealth { get; private set; }

    private ParticleSystem _bloodParticleSystem;

    [HideInInspector]
    public float currentHealth;

    public bool notAbleToGetBitten;

    private IHealthUtility _healthUtility;
    private SlowDownManager _slowDownManager;

    private bool _grasped;
    [HideInInspector] public float maxGraspedTime = 1;
    [HideInInspector] public float graspedTime;

    public event Action<float> OnHealthChanged;
    public event Action OnDeath;

    public bool HasSpawned {get; private set;} = false;
    private bool _died = false;

    public bool Died => _died;
    public float MaxHealth => maxHealth;
    public float NeededSuckingPower => maxHealth;

    public bool IsGrasped => _grasped;

    private ChangeDetector _changeDetector;


    private void Start() => _bloodParticleSystem = GameObject.Find("BloodParticles").GetComponent<ParticleSystem>();

    public override void Spawned()
    {
        if (_healthUtility == null) TryGetComponent(out _healthUtility);
        if (!_slowDownManager) TryGetComponent(out _slowDownManager);

        _changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);

        maxHealth = fishData.MaxHealth;
        recoveryHealthInSecond = fishData.RecoveryHealthInSecond;
        timeToStartRecoveryHealth = fishData.TimeToStartRecoveryHealth;
        Restart();
    }

    public void Restart()
    {
        _died = false;
        NetworkedHealth = maxHealth;
        currentHealth = NetworkedHealth;
        _slowDownManager?.SlowDown();
        HasSpawned = true;
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        HasSpawned = false;
    }

    private void OnDisable()
    {
        HasSpawned = false;
    }

    private void Update()
    {
        if (_grasped)
        {
            graspedTime -= Time.deltaTime;
            if (graspedTime <= 0)
            {
                _grasped = false;
            }
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (_regeneration && RegenTimer.ExpiredOrNotRunning(Runner))
        {
            RecoveryHealthRpc(recoveryHealthInSecond);
            RegenTimer = TickTimer.CreateFromSeconds(Runner, 1);

            if (NetworkedHealth >= maxHealth)
                _regeneration = false;
        }

        if (_changeDetector is null || Died) return;

        foreach (var propertyName in _changeDetector.DetectChanges(this, out var previousBuffer, out var currentBuffer))
        {
            switch (propertyName)
            {
                case nameof(NetworkedHealth):
                    {
                        CheckDeath();
                        break;
                    }
            }
        }
    }

    private void StartPassiveRecoveryHealth()
    {
        RegenTimer = TickTimer.CreateFromSeconds(Runner, timeToStartRecoveryHealth);
        _regeneration = true;
    }

    public void Damage(float amount)
    {
        if (!HasSpawned) return;

        if (notAbleToGetBitten) return;
        ReceiveDamageRpc(amount);
        CheckDeath(); // this line may not be deleted
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
    public void ReceiveDamageRpc(float damage)
    {
        if (damage >= NetworkedHealth * 0.2f)
        {
            _grasped = true;
            graspedTime = maxGraspedTime;
        }
        NetworkedHealth -= damage;

        if (NetworkedHealth > 0)
        {
            StartPassiveRecoveryHealth();
            PlayParticles(Color.red, 10);
        }

        OnHealthChanged?.Invoke(NetworkedHealth);
    }

    private void CheckDeath()
    {
        if (!HasSpawned) return;

        if (_healthUtility is not null)
        {
            if (_healthUtility is PlayerHealth && HasStateAuthority && currentHealth > NetworkedHealth)
                VFXManager.Instance.ShowHurtVignette();

            if (NetworkedHealth <= 0 && !_died)
            {
                _healthUtility.Die();
                OnDeath?.Invoke();
                _died = true;
            }
        }
    }

    public void PlayParticles(Color color, int burstCount)
    {
        var mainModule = _bloodParticleSystem.main;
        mainModule.startColor = new ParticleSystem.MinMaxGradient(color);

        var emissionModule = _bloodParticleSystem.emission;
        emissionModule.SetBursts(new ParticleSystem.Burst[] { new(0.0f, burstCount) });

        var healthObjectTransform = transform;
        var bloodParticleSystemTransform = _bloodParticleSystem.transform;
        //need to safe the current parent before it changes parent to revert that parent change
        var parent = bloodParticleSystemTransform.parent;

        bloodParticleSystemTransform.position = healthObjectTransform.position;
        bloodParticleSystemTransform.SetParent(healthObjectTransform);
        bloodParticleSystemTransform.localScale =
            healthObjectTransform.localScale.z < 1 ? Vector3.one : healthObjectTransform.localScale;
        _bloodParticleSystem.Play();
        bloodParticleSystemTransform.SetParent(parent);
    }

    public int GetSuckedIn()
    {
        DestroySuckableRpc();
        return fishData.XPValue;
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void DestroySuckableRpc()
    {
        Runner.Despawn(Object);
    }
}
