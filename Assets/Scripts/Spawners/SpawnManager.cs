using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [Header("Ocean Player Spawn Points")] 
    public List<SpawnPoint> oceanSpawnPoints;
    
    [Header("Lake Player Spawn Points")] 
    public List<SpawnPoint> lakeSpawnPoints;
    
    [Header("TestLevel Player Spawn Points")]
    public List<SpawnPoint> testLevelSpawnPoints;

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
            3 => lakeSpawnPoints,
            4 => testLevelSpawnPoints,
            _ => null
        };
    }
}
