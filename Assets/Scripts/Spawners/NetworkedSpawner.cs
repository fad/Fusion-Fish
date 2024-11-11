using UnityEngine;
using Fusion;

public abstract class NetworkedSpawner : NetworkBehaviour
{
    [Header("Settings")]
    [SerializeField, Tooltip("The fish to spawn")]
    protected FishData fishData;
    
    [Networked] protected bool HasSpawned { get; set; } 
    
    public abstract void Spawned();
}
