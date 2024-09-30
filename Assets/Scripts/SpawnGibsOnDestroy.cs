using UnityEngine;
using Fusion;
using Random = UnityEngine.Random;

public class SpawnGibsOnDestroy : NetworkBehaviour
{
    [SerializeField] private GameObject gibPrefab;
    [SerializeField] public int gibSpawnCount = 3;
    [SerializeField] private int gibsExperienceValue = 100;
    [HideInInspector] public bool spawnGibs;
    //Made a bool for NPCs to differentiate starfish and shrimp from other NPCs when attacking
    [SerializeField] public bool isShrimp;
    [SerializeField] public bool isStarFish;

    private void OnDestroy()
    {
        if (!gameObject.scene.isLoaded || !spawnGibs) 
            return;
        
        if (gibPrefab != null && gibSpawnCount > 0)
        {
            gibPrefab.GetComponent<HealthManager>().experienceValue = gibsExperienceValue;
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

                //need to go with that /4 workaround so starfishes drop gibs with the right size
                //also for the gib position because otherwise it would spawn inside the ground
                if (TryGetComponent<NPCHealth>(out var npcHealth) && npcHealth.isStarFish)
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