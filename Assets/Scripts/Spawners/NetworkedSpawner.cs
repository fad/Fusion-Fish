using UnityEngine;
using Fusion;

public abstract class NetworkedSpawner : NetworkBehaviour, IInitialisable
{
    [Header("Settings")]
    [SerializeField, Tooltip("The fish to spawn")]
    protected FishData fishData;
    
    [Networked] protected bool HasSpawned { get; set; } 
    
    public abstract override void Spawned();
    
    public void Init(string fishDataName)
    {
        fishData = Resources.Load<FishData>($"FishData/{fishDataName}");
        
        if (!fishData)
        {
            Debug.LogError($"Fish data with name {fishDataName} not found!");
        }
    }
}
