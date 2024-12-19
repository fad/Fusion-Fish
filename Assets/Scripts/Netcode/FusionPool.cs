using System;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

/// <summary>
/// Object pool used to store and reuse NetworkObjects.
/// </summary>
public class FusionPool : Fusion.Behaviour, INetworkObjectProvider
{
    
    /// <summary>
    /// The dictionary that stores the prefab and the queue of instantiated objects.
    /// </summary>
    private readonly Dictionary<NetworkPrefabId, Queue<NetworkObject>> _instantiatedObjects = new();

    /// <summary>
    /// Uses a NetworkObject from the pool or instantiates a new one if the pool is empty or did not exist before.
    /// </summary>
    /// <param name="runner">The NetworkRunner responsible for spawning</param>
    /// <param name="prefab">The NetworkObject to instantiate</param>
    /// <param name="prefabId">The prefab id of the NetworkObject</param>
    /// <returns></returns>
    protected NetworkObject InstantiatePrefab(NetworkRunner runner, NetworkObject prefab, NetworkPrefabId prefabId)
    {
        NetworkObject result;
        
        if(_instantiatedObjects.TryGetValue(prefabId, out Queue<NetworkObject> queue))
        {
            if (queue.Count > 0)
            {
                result = queue.Dequeue();
                result.gameObject.SetActive(true);
            
                return result;   
            }
        }
        else
        {
            _instantiatedObjects.Add(prefabId, new Queue<NetworkObject>());
        }


        result = Instantiate(prefab);
        return result;
        
        
    }
    
    /// <summary>
    /// Returns an object to the pool or destroys it if the pool does not exist.
    /// </summary>
    /// <param name="runner"></param>
    /// <param name="prefabId"></param>
    /// <param name="instance"></param>
    protected void DestroyPrefabInstance(NetworkRunner runner, NetworkPrefabId prefabId, NetworkObject instance)
    {
        if(!_instantiatedObjects.TryGetValue(prefabId, out Queue<NetworkObject> queue))
        {
            Destroy(instance.gameObject);
            return;
        }
        
        queue.Enqueue(instance);
        instance.gameObject.SetActive(false);
    }

    public NetworkObjectAcquireResult AcquirePrefabInstance(NetworkRunner runner, in NetworkPrefabAcquireContext context,
        out NetworkObject result)
    {
        result = null;

        if (runner.SceneManager.IsBusy) // If the scene manager is busy, we can't load the prefab and need to retry.
        {
            return NetworkObjectAcquireResult.Retry;
        }

        NetworkObject prefab;

        try
        {
            // Load the prefab from the runner's prefab loader. This will NOT instantiate it.
            prefab = runner.Prefabs.Load(context.PrefabId, isSynchronous: context.IsSynchronous);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to acquire instance: {ex}");
            return NetworkObjectAcquireResult.Failed;
        }

        if (!prefab) return NetworkObjectAcquireResult.Retry;
        
        result = InstantiatePrefab(runner, prefab, context.PrefabId);
        
        runner.MoveToRunnerScene(result.gameObject);

        runner.Prefabs.AddInstance(context.PrefabId); // To keep track of the instance count.
        return NetworkObjectAcquireResult.Success;
    }

    public void ReleaseInstance(NetworkRunner runner, in NetworkObjectReleaseContext context)
    {
        NetworkObject instance = context.Object;

        if (!context.IsBeingDestroyed)
        {
            if (context.TypeId.IsPrefab)
            {
                NetworkPrefabId prefabId = context.TypeId.AsPrefabId;
                DestroyPrefabInstance(runner, prefabId, instance);
                runner.Prefabs.RemoveInstance(prefabId);
                return;
            }
            
            Destroy(instance.gameObject);
        }
    }
}
