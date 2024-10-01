using Fusion;

public class RequestAuthorityOnSpawn : NetworkBehaviour
{
    //Requests to get state authority over itself, because when a player leaves with the state authority over this object, then the object stops working
    public override void Spawned()
    {
        Object.RequestStateAuthority();
    }
}
