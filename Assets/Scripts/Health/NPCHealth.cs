using Fusion;
using UnityEngine;

public class NPCHealth : NetworkBehaviour
{
    private HealthManager healthManager;
    //Made a bool for NPCs to differentiate starfish and shrimp from other NPCs when attacking
    [SerializeField] public bool isShrimp;
    [SerializeField] public bool isStarFish;
    
    private void Start()
    {
        healthManager = GetComponent<HealthManager>();
    }

    public void NPCCheckDeath()
    {
        if (GetComponent<HealthManager>().NetworkedHealth <= 0)
        {
            if (TryGetComponent<SpawnGibsOnDestroy>(out var spawnGibsOnDestroy) && healthManager.spawnGibs)
            {
                spawnGibsOnDestroy.spawnGibs = true;
            }
            
            NPCDeathRpc();
        }
    }
    
    [Rpc(RpcSources.All, RpcTargets.All, InvokeLocal = true)]
    private void NPCDeathRpc()
    {
        healthManager.PlayParticles(Color.red, 30);

        Destroy(gameObject);
    }
}
