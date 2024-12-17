using System.Collections.Generic;
using Fusion;
using UnityEngine;

/// <summary>
/// Object pool used to store and reuse NetworkObjects.
/// </summary>
public class FusionPool : INetworkObjectProvider
{
    
    /// <summary>
    /// The dictionary that stores the prefab and the queue of instantiated objects.
    /// </summary>
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

    /// <summary>
    /// Creates a new NetworkObject from the loaded prefab. Adds a new queue if the prefab is not in the dictionary.
    /// </summary>
    /// <param name="loadedPrefab">The loaded prefab to instantiate.</param>
    /// <returns>The instantiated NetworkObject</returns>
    private NetworkObject CreateObject(NetworkObject loadedPrefab)
    {
        if (!_instantiatedObjects.ContainsKey(loadedPrefab))
        {
            _instantiatedObjects.Add(loadedPrefab, new Queue<NetworkObject>());
        }
        return Object.Instantiate(loadedPrefab);
    }
}
