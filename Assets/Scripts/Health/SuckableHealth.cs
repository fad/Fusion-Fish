using System;
using Fusion;
using UnityEngine;


/// <summary>
/// Health for suckable objects for NPCs to eat them
/// </summary>
public class SuckableHealth : NetworkBehaviour, IHealthManager
{
    [Header("Settings"), SerializeField]
    private float maxHealth = 1f;
    
    [Networked]
    private float NetworkedHealth { get; set; }
    
    private bool _died;
    
    #region IHealthManager field members
    public event Action<float> OnHealthChanged;
    public event Action OnDeath;
    public bool Died => _died;
    
    public float MaxHealth => maxHealth;
    
    #endregion

    public override void Spawned()
    {
        NetworkedHealth = maxHealth;
    }

    public void Damage(float amount)
    {
        NetworkedHealth -= amount;
        OnHealthChanged?.Invoke(NetworkedHealth);
        
        if (NetworkedHealth <= 0)
        {
            RpcDie();
        }
    }

    public void Heal(float amount)
    {
        // Noop
    }
    
    [Rpc(RpcSources.All, RpcTargets.All)]
    private void RpcDie()
    {
        _died = true;
        OnDeath?.Invoke();
        Destroy(gameObject);
    }
}
