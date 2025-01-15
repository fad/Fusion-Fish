using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "SpawnPointsData", menuName = "Data/SpawnPointsData")]
public class SpawnPointsData : ScriptableObject
{
    [Header("Ocean Player Spawn Points")] 
    public List<SpawnPoint> oceanSpawnPoints;
    
    [Header("Lake Player Spawn Points")] 
    public List<SpawnPoint> lakeSpawnPoints;
    
    [Header("TestLevel Player Spawn Points")]
    public List<SpawnPoint> testLevelSpawnPoints;    
    
    [Header("TestLevel Player Spawn Points")]
    public List<SpawnPoint> hdOceanSpawnPoints;

    public List<SpawnPoint> GetSpawnPoints(int currentScene)
    {
        return currentScene switch
        {
            2 => oceanSpawnPoints,
            3 => lakeSpawnPoints,
            4 => testLevelSpawnPoints,
            5 => hdOceanSpawnPoints,
            _ => null
        };
    }

}
