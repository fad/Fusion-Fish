using System.Collections.Generic;
using UnityEngine;

public class FoodSpawner : MonoBehaviour
{
    [SerializeField] private List<Transform> spawnableFood;

    private List<Transform> spawnPoints = new List<Transform>();

    private void Start()
    {
        for (var i = 0; i < transform.childCount; i++)
        {
            spawnPoints.Add(transform.GetChild(i));
        }
    }

    public void SpawnFood()
    {
        var randomFood = Random.Range(0, spawnableFood.Count - 1);
        
        var randomSpawnPoint = Random.Range(0, spawnPoints.Count - 1);

        Instantiate(spawnableFood[randomFood], spawnPoints[randomSpawnPoint].transform.position, Quaternion.identity);
    }
}
