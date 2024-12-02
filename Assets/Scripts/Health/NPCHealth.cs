using Fusion;
using UnityEngine;

public class NPCHealth : NetworkBehaviour, IHealthUtility
{
    private HealthManager _healthManager;

    private void Start()
    {
        _healthManager = GetComponent<HealthManager>();
    }

    public void Die()
    {
        _healthManager.PlayParticles(Color.red, 30);
        NPCDeathRpc();
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void NPCDeathRpc()
    {
        Runner.Despawn(Object);
    }
}
