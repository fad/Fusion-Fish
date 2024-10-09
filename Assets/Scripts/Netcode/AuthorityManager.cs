using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class AuthorityManager : NetworkBehaviour, IPlayerLeft
{
    // [Networked] private NetworkDictionary<NetworkObject, PlayerRef> AuthorityDictionary { get; set; } = new();
    

    public void PlayerLeft(PlayerRef player)
    {
        //if (AuthorityDictionary.Count == 0)
        {
            Debug.LogWarning("There are no NetworkObjects to transfer authority to.");
            return;
        }
        
        HandleAuthorityTransfer(player);

    }

    private PlayerRef GetNewAuthorityPlayer(PlayerRef oldAuthority)
    {
        foreach (PlayerRef player in Runner.ActivePlayers)
        {
            if (player != oldAuthority)
            {
                return player;
            }
        }
        
        return oldAuthority;
    }

    // TODO: Implement this method
    private void HandleAuthorityTransfer(PlayerRef player)
    {
        // foreach (KeyValuePair<NetworkObject, PlayerRef> authority in AuthorityDictionary)
        // {
        //     NetworkObject networkObject = authority.Key;
            
            // if (AuthorityDictionary[networkObject] == player)
            // {
            //     AuthorityDictionary[networkObject] = GetNewAuthorityPlayer(player);
            //     networkObject.RequestStateAuthority();
            // }
            //
            // if(networkObject.StateAuthority == player)
            // {
            //     networkObject.RequestStateAuthority();
            // }
        }
    }
