using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class AuthorityRegister : MonoBehaviour, IPlayerLeft
{
    public static AuthorityRegister Instance { get; private set; }
    
    public readonly HashSet<NetworkObject> AllNetworkObjects = new();
    
    private void Awake()
    {
        if (!Instance)
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }
        else
        {
            Destroy(gameObject);
        }
    }


    public void PlayerLeft(PlayerRef player)
    {
        Debug.Log($"Player {player.ToString()} left.");
        
        foreach (NetworkObject obj in AllNetworkObjects)
        {
            if (obj.StateAuthority != player) continue;

            obj.GetComponent<AuthorityHandler>().RequestAuthority();
        }
    }
    
    public void Register(NetworkObject obj)
    {
        AllNetworkObjects.Add(obj);
        Debug.Log($"Registered {obj.name} with AuthorityRegister. Count: {AllNetworkObjects.Count}".InColor(Color.green));
    }
    
    public void Unregister(NetworkObject obj)
    {
        AllNetworkObjects.Remove(obj);
    }
}
