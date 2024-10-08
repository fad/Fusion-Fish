using Fusion;
using UnityEngine;

public class NPCSpawner : MonoBehaviour
{
    [Header("Spawner Bounds")]
    [SerializeField] private Collider spawnerBounds;

    public void Spawn(NetworkRunner runner, Transform typeToSpawn, Vector2Int range)
    {
        int randomNumber = Random.Range(range.x, range.y+1);
        
        for (int i = 0; i < randomNumber; i++)
        {
            Vector3 randomPosition = Random.insideUnitSphere * spawnerBounds.bounds.extents.magnitude;
            
            runner.Spawn(typeToSpawn.gameObject, randomPosition, Quaternion.identity);
        }
        
        
    }
}
