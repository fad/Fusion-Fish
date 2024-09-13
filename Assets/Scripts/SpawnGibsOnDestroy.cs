using UnityEngine;
using Fusion;

namespace BiggestFish.Gameplay
{
    public class SpawnGibsOnDestroy : NetworkBehaviour
    {
        [SerializeField] private GameObject gibPrefab;
        [SerializeField] private int gibSpawnCount = 3;
        [SerializeField] private int nutritionalValue = 1;

        private void OnDestroy()
        {
            if (gibPrefab != null && gibSpawnCount > 0)
                SpawnMeatObjects();
        }
        
        public void SpawnMeatObjects()
        {
            if (gibPrefab != null)
                for (var i = 0; i < gibSpawnCount; i++)
                {
                    var randomOffset = Random.insideUnitSphere * 0.5f; // Adjust the offset as needed
                    var spawnPosition = transform.position + randomOffset;
                    //Need to make that online
                    //Runner.Spawn(gibPrefab, spawnPosition, Quaternion.identity);
                    Instantiate(gibPrefab, spawnPosition, Quaternion.identity);
                }
        }
    }
}