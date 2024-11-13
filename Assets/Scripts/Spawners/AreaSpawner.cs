using UnityEngine;
using Random = UnityEngine.Random;

public class AreaSpawner : NetworkedSpawner
{
    [SerializeField, Tooltip("The min and max amount to spawn. X is min, Y is max.")]
    private Vector2Int minMaxAmountToSpawn = new(2,4);
    
    [SerializeField, Tooltip("The bounds of the area to spawn the fish in")]
    private Collider areaBounds;
    
    [SerializeField, Tooltip("The scale of the area bounds")]
    private float areaBoundsScale = 1f;

    public override void Spawned()
    {
        if (HasSpawned) return;
        
        int amountToSpawn = Random.Range(minMaxAmountToSpawn.x, minMaxAmountToSpawn.y + 1);
        
        for (int i = 0; i < amountToSpawn; i++)
        {
            Vector3 randomPosition = Random.insideUnitSphere * areaBounds.bounds.extents.magnitude * areaBoundsScale;
            Vector3 spawnPosition = areaBounds.transform.position + randomPosition;
            
            FishSpawnHandler.Instance.Spawn(fishData, spawnPosition);
        }

        HasSpawned = true;
    }

    private void OnDrawGizmos()
    {
        if (!areaBounds) return;
        
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireCube(areaBounds.bounds.center, areaBounds.bounds.size * areaBoundsScale);
    }
}
