using System;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using UnityEngine;

public class AuthorityRegister : MonoBehaviour, INetworkRunnerCallbacks
{
    public static AuthorityRegister Instance { get; private set; }
    
    public readonly HashSet<NetworkObject> AllNetworkObjects = new();
    
    private NetworkRunner _runner;
    
    private void Awake()
    {
        if (!Instance)
        {
            Instance = this;
            //DontDestroyOnLoad(this);
            _runner = FindObjectOfType<NetworkRunner>();

            if (!_runner)
            {
                Debug.LogError("No NetworkRunner found in the scene. Unable to deal with AuthorityTransfer.".InColor(Color.red));
                return;
            }
            
            _runner.AddCallbacks(this);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public void Register(NetworkObject obj)
    {
        AllNetworkObjects.Add(obj);
    }
    
    public void Unregister(NetworkObject obj)
    {
        AllNetworkObjects.Remove(obj);
    }
    
    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        foreach (NetworkObject obj in AllNetworkObjects)
        {
            if (obj.StateAuthority != player) continue;

            obj.GetComponent<AuthorityHandler>().RequestAuthority();
        }
    }
    
    #region Unused Callbacks

    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
    }

    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
    }

    

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
    }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
    {
    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
    }

    public void OnConnectedToServer(NetworkRunner runner)
    {
    }

    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
    {
    }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
    {
    }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {
    }

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
    {
    }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
    }

    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
    {
    }

    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
    {
    }

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data)
    {
    }

    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress)
    {
    }

    public void OnSceneLoadDone(NetworkRunner runner)
    {
    }

    public void OnSceneLoadStart(NetworkRunner runner)
    {
    }
    
    #endregion
}
