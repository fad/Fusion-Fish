using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [Header("Ocean Player Spawn Points")] 
    public List<SpawnPoint> oceanSpawnPoints;
    
    [Header("River Player Spawn Points")] 
    public List<SpawnPoint> riverSpawnPoints;

    [HideInInspector] public int currentScene;

    public static SpawnManager Instance;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public List<SpawnPoint> SpawnPoints()
    {
        return currentScene switch
        {
            2 => oceanSpawnPoints,
            3 => riverSpawnPoints,
            _ => null
        };
    }
}
