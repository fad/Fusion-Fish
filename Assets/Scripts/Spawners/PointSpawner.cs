public class PointSpawner : NetworkedSpawner
{
    public override void Spawned()
    {
        if(HasSpawned) return;
        
        FishSpawnHandler.Instance.Spawn(fishData, transform.position);
    }
}
