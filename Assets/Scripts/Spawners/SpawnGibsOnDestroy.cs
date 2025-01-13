using UnityEngine;
using Fusion;
using Random = UnityEngine.Random;

public class SpawnGibsOnDestroy : NetworkBehaviour
{
    [Header("Gibs settings")]
    [SerializeField] private NetworkObject gibPrefab;

    [SerializeField, Space(10)] public int gibSpawnCount = 3;
    [SerializeField] private bool spawnGibs;

    [Header("FishData Settings")]
    [SerializeField]
    private FishData fishData;
    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        if (!spawnGibs || !gameObject.scene.isLoaded || !hasState) return;

        SpawnGibsRpc();
    }

    public  void SpawnMeatObjects(int currentGibSpawnCount)
    {
        if (gibPrefab && currentGibSpawnCount > 0 )
        {
            var spawnPosition = transform.position + Random.insideUnitSphere * 0.5f;

            gibPrefab.transform.localScale = transform.localToWorldMatrix.lossyScale / gibSpawnCount;

            NetworkObject gib =  Runner.Spawn(gibPrefab, spawnPosition, Quaternion.identity);
            gib.GetComponent<ISuckable>().SetupXP(fishData.XPValue);

            currentGibSpawnCount--;
            SpawnMeatObjects(currentGibSpawnCount);
        }
    }
    public void FishDataUpdate(FishData newData)
    {
        if(newData!=null)
            fishData = newData;
    }
    
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void SpawnGibsRpc()
    {
        SpawnMeatObjects(gibSpawnCount);
    }
}