using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class FusionPool : MonoBehaviour, INetworkObjectProvider
{
    private Dictionary<NetworkObject, Queue<NetworkObject>> _instantiatedObjects = new();

    public NetworkObjectAcquireResult AcquirePrefabInstance(NetworkRunner runner, in NetworkPrefabAcquireContext context,
        out NetworkObject result)
    {
        if (!NetworkProjectConfig.Global.PrefabTable.Contains(context.PrefabId))
        {
            result = null;
            return NetworkObjectAcquireResult.Failed;
        }
        
        result = NetworkProjectConfig.Global.PrefabTable.Load(context.PrefabId, false);
        return NetworkObjectAcquireResult.Success;
    }

    public void ReleaseInstance(NetworkRunner runner, in NetworkObjectReleaseContext context)
    {
        return;
    }
}
