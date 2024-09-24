#if CMPSETUP_COMPLETE
using System.Collections.Generic;
using Fusion;
using UnityEngine;
using Random = UnityEngine.Random;

namespace AvocadoShark
{
    public class PlayerPosResetter : NetworkBehaviour, IPlayerJoined
    {
        [HideInInspector] public SpawnPoint currentSpawnablePoint;
        private float currentDistance;
        public float minYValue = -10f;

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
                foreach (var playerRef in playerInGame)
                {
                    foreach (var spawnPoint in SpawnManager.Instance.spawnPoints)
                    {
                        if (Vector3.Distance(Runner.GetPlayerObject(playerRef).transform.position, spawnPoint.location) <
                            currentDistance || currentDistance == 0)
                        {
                            currentDistance = Vector3.Distance(Runner.GetPlayerObject(playerRef).transform.position, spawnPoint.location);
                            currentSpawnablePoint = spawnPoint;
                        }
                    }

                    spawnablePoints.Add(currentSpawnablePoint);
                    spawnPointDistances.Add(currentDistance);
                }
            
                foreach (var distance in spawnPointDistances)
                {
                    if (currentDistance < distance)
                    {
                        currentDistance = distance;
                        currentSpawnablePoint = spawnablePoints[spawnPointDistances.BinarySearch(distance)];
                    }
                }
                
                transform.SetPositionAndRotation(currentSpawnablePoint.location, currentSpawnablePoint.rotation);
            }
            else
            {
                var spawnPoint = Random.Range(0, SpawnManager.Instance.spawnPoints.Count - 1);
                var spawnPoints = SpawnManager.Instance.spawnPoints;

                transform.SetPositionAndRotation(spawnPoints[spawnPoint].location, spawnPoints[spawnPoint].rotation);
            }
        }
        
        public void PlayerJoined(PlayerRef player)
        {
            playerInGame.Add(player);
        }
    }
}
#endif