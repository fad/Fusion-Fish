using Fusion;

public class RequestAuthorityOnSpawn : NetworkBehaviour
{
    public override void Spawned()
    {
        Object.RequestStateAuthority();
    }
}
