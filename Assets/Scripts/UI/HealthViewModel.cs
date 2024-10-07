using Fusion;
using UnityEngine;
using UnityEngine.UI;

public class HealthViewModel : NetworkBehaviour
{
    [Header("Needed components")]
    [SerializeField] private HealthManager healthModel;
    
    [SerializeField] private Slider healthSlider;
    
    public void OnEnable()
    {
        healthModel.OnHealthChanged += RPC_UpdateHealthSlider;
        
        healthSlider.maxValue = healthModel.maxHealth;
    }
    
    public void OnDisable()
    {
        healthModel.OnHealthChanged -= RPC_UpdateHealthSlider;
    }

    
    [Rpc(RpcSources.All, RpcTargets.All)]
    private void RPC_UpdateHealthSlider(float value)
    {
        healthSlider.value = value;    
    }
}
