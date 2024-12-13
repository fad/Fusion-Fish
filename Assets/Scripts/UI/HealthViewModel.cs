using Fusion;
using UnityEngine;
using UnityEngine.UI;

public class HealthViewModel : NetworkBehaviour
{
    [Header("Needed components")]
    [SerializeField] private HealthManager healthModel;
    
    [SerializeField] private Slider healthSlider;
    
    public void Start()
    {
        healthModel.OnHealthChanged += RPC_UpdateHealthSlider;
        
        healthSlider.maxValue = healthModel.maxHealth;
        healthSlider.value = healthModel.currentHealth;
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        healthModel.OnHealthChanged -= RPC_UpdateHealthSlider;
    }
    
    public void AdjustHealthBarVisibility(bool isVisible)
    {
        healthSlider.gameObject.SetActive(isVisible);
        if(healthModel.HasSpawned)
            healthSlider.value = healthModel.NetworkedHealth;
    }

    
    [Rpc(RpcSources.All, RpcTargets.All)]
    private void RPC_UpdateHealthSlider(float value)
    {
        healthSlider.value = value;    
    }
}
