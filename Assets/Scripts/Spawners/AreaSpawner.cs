using Fusion;
using UnityEngine;

public class AreaSpawner : NetworkBehaviour
{
    [Header("Settings")]
    [SerializeField, Tooltip("The fish to spawn")]
    private FishData fishData;
    
    [SerializeField, Tooltip("The min and max amount to spawn. X is min, Y is max.")]
    private Vector2Int minMaxAmountToSpawn = new(2,4);
    
    [Networked] private bool HasSpawned { get; set; }

    public override void Spawned()
    {
        if (HasSpawned) return;
        
        int amountToSpawn = Random.Range(minMaxAmountToSpawn.x, minMaxAmountToSpawn.y + 1);
        
        for (int i = 0; i < amountToSpawn; i++)
        {
            Vector3 spawnPosition = transform.position + new Vector3(Random.Range(-5f, 5f), 0f, Random.Range(-5f, 5f));
            FishSpawnHandler.Instance.Spawn(fishData, spawnPosition);
        }

        HasSpawned = true;
    }
}
