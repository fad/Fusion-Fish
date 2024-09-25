using System;
using UnityEngine;
using Fusion;
using Random = UnityEngine.Random;

namespace BiggestFish.Gameplay
{
    public class SpawnGibsOnDestroy : NetworkBehaviour
    {
        [SerializeField] private GameObject gibPrefab;
        [SerializeField] public int gibSpawnCount = 3;
        [SerializeField] private int gibsExperienceValue = 100;
        [HideInInspector] public bool spawnGibs;
        [SerializeField] private bool isStarFish;
        
        private void OnDestroy()
        {
            if (!gameObject.scene.isLoaded || !spawnGibs) 
                return;
            
            if (gibPrefab != null && gibSpawnCount > 0)
            {
                gibPrefab.GetComponent<Health>().experienceValue = gibsExperienceValue;
                SpawnMeatObjects(HudUI.Instance.playerManager.hostPlayerRunner);
            }
        }
        
        public void SpawnMeatObjects(NetworkRunner runner)
        {
            if (gibPrefab != null)
                for (var i = 0; i < gibSpawnCount; i++)
                {
                    //need to go with that /3 workaround so starfishes drop gibs with the right size
                    if (isStarFish)
                    {
                        gibPrefab.transform.localScale = transform.localToWorldMatrix.lossyScale / 3;
                    }
                    else
                    {
                        gibPrefab.transform.localScale = transform.localToWorldMatrix.lossyScale / gibSpawnCount;
                    }
                    var randomOffset = Random.insideUnitSphere * 0.5f; // Adjust the offset as needed
                    var spawnPosition = transform.position + randomOffset;
                    runner.Spawn(gibPrefab, spawnPosition, Quaternion.identity);
                }
        }
    }
}