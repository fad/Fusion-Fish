#if CMPSETUP_COMPLETE
using System.Collections.Generic;
using System.Linq;
using Fusion;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlayerPositionReset : NetworkBehaviour, IPlayerJoined, IPlayerLeft
{
    [SerializeField] private float minYValue = -10f;
    [HideInInspector] public SpawnPoint currentSpawnablePoint;
    private float currentDistance;

    private List<PlayerRef> playerInGame = new List<PlayerRef>();
    private List<SpawnPoint> spawnablePoints = new List<SpawnPoint>();
    private List<float> spawnPointDistances = new List<float>();

    void LateUpdate()
    {
        if (HasStateAuthority)
        {
            if (transform.position.y < minYValue)
            {
                ResetPlayerPosition();
            }
        }
    }
    
    public void ResetPlayerPosition()
    {
        if (playerInGame.Count > 1)
        {
            //goes through all the player inside the game
            //then goes through all the spawn points and caches the closest spawn point in relation to all the players 
            foreach (var playerRef in playerInGame)
            {
                foreach (var spawnPoint in SpawnManager.Instance.spawnPoints.Where(spawnPoint => Vector3.Distance(Runner.GetPlayerObject(playerRef).transform.position, spawnPoint.location) < currentDistance || currentDistance == 0))
                {
                    currentDistance = Vector3.Distance(Runner.GetPlayerObject(playerRef).transform.position, spawnPoint.location);
                    currentSpawnablePoint = spawnPoint;
                }

                spawnablePoints.Add(currentSpawnablePoint);
                spawnPointDistances.Add(currentDistance);
            }
        
            //Then goes trough all the closest spawn points and tries to find the furthest away from any other player and spawns there
            foreach (var distance in spawnPointDistances.Where(distance => currentDistance < distance))
            {
                currentDistance = distance;
                currentSpawnablePoint = spawnablePoints[spawnPointDistances.BinarySearch(distance)];
            }
            
            transform.SetPositionAndRotation(currentSpawnablePoint.location, currentSpawnablePoint.rotation);

            currentDistance = 0;
            currentSpawnablePoint = null;
            spawnablePoints.Clear();
            spawnPointDistances.Clear();
        }
        else
        {
            //When only one player is in game, then take a random spot to spawn the player
            var spawnPoint = Random.Range(0, SpawnManager.Instance.spawnPoints.Count - 1);
            var spawnPoints = SpawnManager.Instance.spawnPoints;

            transform.SetPositionAndRotation(spawnPoints[spawnPoint].location, spawnPoints[spawnPoint].rotation);
        }
    }
    
    public void PlayerJoined(PlayerRef player)
    {
        playerInGame.Add(player);
    }

    public void PlayerLeft(PlayerRef player)
    {
        playerInGame.Remove(player);
    }
}
#endif