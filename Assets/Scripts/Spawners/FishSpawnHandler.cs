using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Fusion;

/// <summary>
/// Utility class to handle spawning fish.
/// </summary>
public class FishSpawnHandler : NetworkBehaviour
{
    private readonly Dictionary<int, FishData> _fishDataDictionary = new();
    private readonly Dictionary<string, FishData> _fishDataDictionaryByName = new();
    
    
    private const string FishDataPath = "FishData";
    
    /// <summary>
    /// The dictionary containing all the fish data by ID.
    /// </summary>
    public Dictionary<int, FishData> FishDataIDDictionary => _fishDataDictionary;
    
    /// <summary>
    /// The dictionary containing all the fish data by name.
    /// </summary>
    public Dictionary<string, FishData> FishDataNameDictionary => _fishDataDictionaryByName;

    public static FishSpawnHandler Instance { get; private set; }

    private void Awake()
    {
        if (!Instance)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        
        LoadFishData();
    }
    
    private void LoadFishData()
    {
        FishData[] fishData = Resources.LoadAll<FishData>(FishDataPath);
        
        foreach (FishData data in fishData)
        {
            _fishDataDictionary.Add(data.FishID, data);
            _fishDataDictionaryByName.Add(data.name, data);
        }
    }
    
    
    /// <summary>
    /// Spawns a fish at the given position.
    /// </summary>
    /// <param name="data">The fish data object to use for spawning.</param>
    /// <param name="position">The position to spawn the fish at.</param>
    public void Spawn(FishData data, Vector3 position)
    {
        Runner.Spawn(data.FishPrefab, position, Quaternion.identity);
    }
    
    /// <summary>
    /// Spawns a fish at the given position.
    /// </summary>
    /// <param name="id">The ID of the fish</param>
    /// <param name="position">The position to spawn the fish at.</param>
    public void Spawn(int id, Vector3 position)
    {
        if (_fishDataDictionary.TryGetValue(id, out FishData data))
        {
            Spawn(data, position);
        }
    }
    
    /// <summary>
    /// Spawn a fish at the given position.
    /// </summary>
    /// <param name="fishName">The name of the fish.</param>
    /// <param name="position">The position to spawn the fish at.</param>
    public void Spawn(string fishName, Vector3 position)
    {
        FishData data = _fishDataDictionary.Values.FirstOrDefault(fishData => fishData.name == fishName);
        if (data)
        {
            Spawn(data, position);
        }
    }

    /// <summary>
    /// Initialises the fish object with the given data.
    /// </summary>
    /// <param name="data">The data to use</param>
    /// <param name="obj">The base prefab that was used for spawning</param>
    /// <param name="runner">The NetworkRunner used for spawning</param>
    private void Initialise(FishData data, NetworkObject obj, NetworkRunner runner)
    {
        // BUG: The child object is not being spawned correctly since it is not a network object
        NetworkObject child = runner.Spawn(data.FishPrefab, transform.position, Quaternion.identity);
        
        child.transform.SetParent(obj.transform);
        child.transform.localPosition = Vector3.zero;

        RPC_Init(obj, data.name);
    }

    /// <summary>
    /// RPC to initialise the fish object for every client.
    /// </summary>
    /// <param name="obj">The base prefab that was used for spawning.</param>
    /// <param name="fishDataName">The name of the fish data to use.</param>
    [Rpc(RpcSources.All, RpcTargets.All)]
    private void RPC_Init(NetworkObject obj, string fishDataName)
    {
        foreach (var initialisable in obj.GetComponentsInChildren<IInitialisable>())
        {
            initialisable.Init(fishDataName);
        }
    }
}
