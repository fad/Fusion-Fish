using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class FusionPool : MonoBehaviour, INetworkObjectProvider
{
    private readonly Dictionary<NetworkObject, Queue<NetworkObject>> _instantiatedObjects = new();

    public NetworkObjectAcquireResult AcquirePrefabInstance(NetworkRunner runner, in NetworkPrefabAcquireContext context,
        out NetworkObject result)
    {
        if (!NetworkProjectConfig.Global.PrefabTable.Contains(context.PrefabId))
        {
            result = null;
            return NetworkObjectAcquireResult.Failed;
        }
        
        NetworkObject loaded = NetworkProjectConfig.Global.PrefabTable.Load(context.PrefabId, isSynchronous: false);
        
        if(_instantiatedObjects.TryGetValue(loaded, out Queue<NetworkObject> queue) && queue.Count > 0)
        {
            result = queue.Dequeue();
            return NetworkObjectAcquireResult.Success;
        }
        
        result = CreateObject(loaded);
        
        return NetworkObjectAcquireResult.Success;
    }

    public void ReleaseInstance(NetworkRunner runner, in NetworkObjectReleaseContext context)
    {
        NetworkObject networkedObj = context.Object;
        networkedObj.gameObject.SetActive(false);
        _instantiatedObjects[networkedObj].Enqueue(networkedObj);
    }

    private NetworkObject CreateObject(NetworkObject obj)
    {
        if (!_instantiatedObjects.ContainsKey(obj))
        {
            _instantiatedObjects.Add(obj, new Queue<NetworkObject>());
        }
        return Instantiate(obj);
    }
}
