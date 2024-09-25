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
            {
                for (var i = 0; i < gibSpawnCount; i++)
                {
                    var spawnPosition = transform.position + Random.insideUnitSphere * 0.5f;

                    //need to go with that /3 workaround so starfishes drop gibs with the right size
                    //also for the gib position because otherwise it would spawn inside the ground
                    if (isStarFish)
                    {
                        gibPrefab.transform.localScale = transform.localToWorldMatrix.lossyScale / 4;
                        spawnPosition = transform.position;
                    }
                    else
                    {
                        gibPrefab.transform.localScale = transform.localToWorldMatrix.lossyScale / gibSpawnCount;
                    }
                    
                    runner.Spawn(gibPrefab, spawnPosition, Quaternion.identity);
                }   
            }
        }
    }
}