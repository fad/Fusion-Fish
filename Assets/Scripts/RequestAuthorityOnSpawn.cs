using Fusion;
using UnityEngine;

public class RequestAuthorityOnSpawn : NetworkBehaviour, IPlayerJoined
{
    public void PlayerJoined(PlayerRef player)
    {
        Object.RequestStateAuthority();
    }
}
