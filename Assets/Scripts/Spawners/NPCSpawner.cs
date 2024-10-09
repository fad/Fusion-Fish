using Fusion;
using UnityEngine;
using Random = UnityEngine.Random;

public class NPCSpawner : NetworkBehaviour
{
    [Header("Spawner Bounds")]
    [SerializeField]
    [Tooltip("The bounds of the spawner to spawn the NPCs in")]
    private Collider spawnerBounds;
    
    [SerializeField]
    [Tooltip("The scale of the spawner bounds")]
    private float spawnerBoundsScale = 1f;

    public void Spawn(NPCBehaviour typeToSpawn, Vector2Int range)
    {
        int randomNumber = Random.Range(range.x, range.y+1);
        
        for (int i = 0; i < randomNumber; i++)
        {
            Vector3 randomPosition = Random.insideUnitSphere * spawnerBounds.bounds.extents.magnitude * spawnerBoundsScale;
            Vector3 spawnPosition = spawnerBounds.transform.position + randomPosition;
            
            Runner.Spawn(typeToSpawn, spawnPosition, Quaternion.identity);
        }
    }
    
    private void OnDrawGizmos()
    {
        if(!spawnerBounds) return;
        
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(spawnerBounds.bounds.center, spawnerBounds.bounds.extents.magnitude * spawnerBoundsScale);
    }
}
