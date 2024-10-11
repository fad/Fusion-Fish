using Fusion;

public class NetworkObjectRegistryManager : NetworkBehaviour
{
    public override void Spawned()
    {
        if(!AuthorityRegister.Instance) return;
        
        AuthorityRegister.Instance.Register(Object);
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        if(!AuthorityRegister.Instance) return;
        
        AuthorityRegister.Instance.Unregister(Object);
    }
}
