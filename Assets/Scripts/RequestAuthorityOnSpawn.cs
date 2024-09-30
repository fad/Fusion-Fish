using Fusion;

public class RequestAuthorityOnSpawn : NetworkBehaviour, IPlayerJoined
{
    //Requests to get state authority over itself, because when a player leaves with the state authority over this object, then the object stops working
    public void PlayerJoined(PlayerRef player)
    {
        Object.RequestStateAuthority();
    }
}
