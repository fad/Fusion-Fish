using UnityEngine;
using Fusion;

namespace BiggestFish.Gameplay
{
    public class SpawnGibsOnDestroy : NetworkBehaviour
    {
        [SerializeField] private GameObject gibPrefab;
        [SerializeField] public int gibSpawnCount = 3;

        private void OnDestroy()
        {
            if (gibPrefab != null && gibSpawnCount > 0)
            {
                SpawnMeatObjects(UI.Instance.playerManager.hostPlayerRunner);
            }
        }
        
        public void SpawnMeatObjects(NetworkRunner runner)
        {
            if (gibPrefab != null)
                for (var i = 0; i < gibSpawnCount; i++)
                {
                    var randomOffset = Random.insideUnitSphere * 0.5f; // Adjust the offset as needed
                    var spawnPosition = transform.position + randomOffset;
                    //Need to make that online
                    runner.Spawn(gibPrefab, spawnPosition, Quaternion.identity);
                }
        }
    }
}