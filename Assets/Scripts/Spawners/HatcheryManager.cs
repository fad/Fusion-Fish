using Fusion;
using UnityEngine;

[RequireComponent(typeof(NetworkObject))]
public class HatcheryManager : NetworkBehaviour
{
    
    [Header("Cooldown Settings")]
    [SerializeField] 
    private float spawnCooldown = 120f;
    
    [SerializeField] 
    private float resetCooldown = 30f;

    [Header("Spawner Settings")]
    [SerializeField] 
    private Transform fishTypeToSpawn;
    
    [SerializeField] 
    private Vector2Int spawnAmountRange = new(1,3);

    [SerializeField]
    private NPCSpawner spawnerToUse;

    [Header("Visual Settings")]
    [SerializeField]
    private GameObject eggs;
    
    private TickTimer _spawnTimer;
    private TickTimer _resetTimer;

    private void Awake()
    {
        _spawnTimer = TickTimer.CreateFromSeconds(Runner, spawnCooldown);
    }

    public override void FixedUpdateNetwork()
    {
        if (_spawnTimer.Expired(Runner))
        {
            SpawnFishes();
        }
        
        if(_resetTimer.Expired(Runner))
        {
            ResetHatchery();
        }
    }

    private void SpawnFishes()
    {
        _spawnTimer = default;
        eggs.SetActive(false);

        Debug.Log("Spawning fishes");
        
        //spawnerToUse.Spawn(Runner, fishTypeToSpawn, spawnAmountRange);
        
        _resetTimer = TickTimer.CreateFromSeconds(Runner, resetCooldown);
    }
    
    private void ResetHatchery()
    {
        _resetTimer = default;
        eggs.SetActive(true);
        
        Debug.Log("Resetting hatchery");
        
        _spawnTimer = TickTimer.CreateFromSeconds(Runner, spawnCooldown);
    }
}
