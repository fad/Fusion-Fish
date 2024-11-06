using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Fusion;

public class FishSpawnHandler : MonoBehaviour
{
    private Dictionary<int, FishData> _fishDataDictionary = new();
    private NetworkRunner _networkRunner;
    
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

    public void SetupRunner()
    {
        _networkRunner = FindObjectOfType<NetworkRunner>();
    }

    public void ResetRunner()
    {
        _networkRunner = null;
    }

    public void Spawn(FishData data, Vector3 position)
    {
        
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
}
