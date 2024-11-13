using UnityEngine;

public class PointSpawner : NetworkedSpawner
{
    public override void Spawned()
    {
        if(HasSpawned) return;
        
        FishSpawnHandler.Instance.Spawn(fishData, transform.position);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, 1f);
    }
}
