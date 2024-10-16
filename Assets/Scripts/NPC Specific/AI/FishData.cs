using UnityEngine;

public class FishData : ScriptableObject
{
    [Header("Hunt Values")]
    [SerializeField]
    private FishData[] preyList;
    
    [SerializeField]
    private FishData[] predatorList;

    [Header("Movement Values")]
    [SerializeField]
    private float wanderSpeed;
    
    [Header("Fleeing Values")]
    [SerializeField]
    private float safeDistance;
    
    
    
    public FishData[] PreyList => preyList;
    public FishData[] PredatorList => predatorList;
    
    public float WanderSpeed => wanderSpeed;
    
    public float SafeDistance => safeDistance;
}
