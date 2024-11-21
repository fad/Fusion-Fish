using Fusion;
using UnityEngine;

public class NPCHealth : NetworkBehaviour
{
    private HealthManager _healthManager;
    private SpawnGibsOnDestroy _spawnGibsOnDestroy;

    //Made a bool for NPCs to differentiate starfish and shrimp from other NPCs when attacking
    [SerializeField]
    public bool isShrimp;

    [SerializeField]
    public bool isStarFish;

    private void Start()
    {
        _healthManager = GetComponent<HealthManager>();
        _spawnGibsOnDestroy = GetComponentInChildren<SpawnGibsOnDestroy>();
    }

    public void Die()
    {
        if (_spawnGibsOnDestroy && _healthManager.spawnGibs)
        {
            _spawnGibsOnDestroy.spawnGibs = true;
            _healthManager.PlayParticles(Color.red, 30);
        }

        NPCDeathRpc();
    }

    [Rpc(RpcSources.All, RpcTargets.All, InvokeLocal = true)]
    private void NPCDeathRpc()
    {
        Destroy(gameObject);
    }
}
