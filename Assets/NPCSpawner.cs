using System.Collections.Generic;
using UnityEngine;

public class NPCSpawner : MonoBehaviour
{
    [SerializeField] private List<Transform> spawnableFishes;

    private List<Transform> spawnPoints = new List<Transform>();

    private void Start()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            spawnPoints.Add(transform.GetChild(i));
        }
    }

    public void SpawnFish()
    {
        var randomFish = Random.Range(0, spawnableFishes.Count - 1);
        
        var randomSpawnPoint = Random.Range(0, spawnPoints.Count - 1);

        Instantiate(spawnableFishes[randomFish], spawnPoints[randomSpawnPoint].transform.position, Quaternion.identity);
    }
}
