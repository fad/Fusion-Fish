using Fusion;
using UnityEngine;

[RequireComponent(typeof(NetworkObject))]
public class HatcheryManager : NetworkBehaviour
{
    [Header("Cooldown Settings")]
    [SerializeField]
    [Tooltip("Time it takes for the hatchery to spawn fishes")]
    private float spawnCooldownInSeconds = 120f;
    
    [SerializeField]
    [Tooltip("Time it takes for the hatchery to \"respawn\" eggs")]
    private float resetCooldownInSeconds = 30f;

    [Header("Spawner Settings")]
    [SerializeField] 
    [Tooltip("The type of fish to spawn")]
    private FishData fishTypeToSpawn;
    
    [SerializeField] 
    [Tooltip("The range for the amount of fish to spawn. \nX is the inclusive minimum, Y is the inclusive maximum.")]
    private Vector2Int spawnAmountRange = new(1,3);

    [SerializeField]
    [Tooltip("The spawner to use for spawning the fish")]
    private NPCSpawner spawnerToUse;

    [Header("Visual Settings")]
    [SerializeField]
    [Tooltip("The eggs to show when the hatchery is ready to spawn")]
    private GameObject eggs;
    
    [Networked] private TickTimer SpawnTimer { get; set; }
    [Networked] private TickTimer ResetTimer { get; set; }

    [Networked] private bool TimersAreRunning { get; set; }

    public override void Spawned()
    {
        TimersAreRunning = SpawnTimer.IsRunning || ResetTimer.IsRunning;
            
        if (TimersAreRunning) return;
        
        SpawnTimer = TickTimer.CreateFromSeconds(Runner, spawnCooldownInSeconds);
    }

    public override void FixedUpdateNetwork()
    {
        if(!fishTypeToSpawn || !spawnerToUse) return;
        
        if (SpawnTimer.Expired(Runner))
        {
            SpawnFishes();
        }
        
        if(ResetTimer.Expired(Runner))
        {
            ResetHatchery();
        }
    }

    private void SpawnFishes()
    {
        SpawnTimer = default;
        eggs.SetActive(false);
        
        spawnerToUse.Spawn(fishTypeToSpawn, spawnAmountRange);
        
        ResetTimer = TickTimer.CreateFromSeconds(Runner, resetCooldownInSeconds);
    }
    
    private void ResetHatchery()
    {
        ResetTimer = default;
        eggs.SetActive(true);
        
        SpawnTimer = TickTimer.CreateFromSeconds(Runner, spawnCooldownInSeconds);
    }
}
