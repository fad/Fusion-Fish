using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Fusion;
using Unity.VisualScripting;
using UnityEngine.Serialization;

public class FishSpawnHandler : NetworkBehaviour
{
    [Header("Prefab Settings")]
    [SerializeField, 
     Tooltip("The base prefab to use as a parent for the fish")]
    private SpawnInitialiser fishPrefab;
    
    private Dictionary<int, FishData> _fishDataDictionary = new();
    
    public static FishSpawnHandler Instance { get; private set; }

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
        
        LoadFishData();
    }
    
    private void LoadFishData()
    {
        FishData[] fishData = Resources.LoadAll<FishData>("FishData");
        
        foreach (FishData data in fishData)
        {
            _fishDataDictionary.Add(data.FishID, data);
        }
    }

    public void Spawn(FishData data, Vector3 position)
    {
        if (!HasStateAuthority) return;
        
        Runner.Spawn(fishPrefab, position, Quaternion.identity, onBeforeSpawned: (runner, obj) =>
        {
            Initialise(data, obj, runner);
        });
    }
    
    public void Spawn(int id, Vector3 position)
    {
        if (_fishDataDictionary.TryGetValue(id, out FishData data))
        {
            Spawn(data, position);
        }
    }
    
    public void Spawn(string fishName, Vector3 position)
    {
        FishData data = _fishDataDictionary.Values.FirstOrDefault(fishData => fishData.name == fishName);
        if (data)
        {
            Spawn(data, position);
        }
    }

    private void Initialise(FishData data, NetworkObject obj, NetworkRunner runner)
    {
        SpawnInitialiser SI = obj.GetComponentInChildren<SpawnInitialiser>();
        NetworkObject child = runner.Spawn(data.FishPrefab, transform.position, Quaternion.identity);
        
        // TODO: Network Transform on Child Object
        child.transform.SetParent(obj.transform);

        foreach (IInitialisable initialisable in obj.GetComponentsInChildren<IInitialisable>())
        {
            initialisable.Init();
        }
    }
}
