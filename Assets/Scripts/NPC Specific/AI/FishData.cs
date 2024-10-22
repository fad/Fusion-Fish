using UnityEngine;

[CreateAssetMenu(fileName = "New FishData", menuName = "Data/New Fish Data", order = 0)]
public class FishData : ScriptableObject
{
    [Header("Hunt Values")]
    [SerializeField]
    private FishData[] preyList;
    
    [SerializeField]
    private FishData[] predatorList;

    [Header("Movement Values")]
    [SerializeField, Min(1f), Tooltip("The speed at which the fish will wander around normally.")]
    private float wanderSpeed = 1f;

    [SerializeField, Min(1f), Tooltip("The speed at which the fish will rotate.")]
    private float rotationSpeed = 1f;
    
    [SerializeField, Range(30f, 60f), Tooltip("The maximum pitch the fish can have.")]
    private float maxPitch;

    [SerializeField, Min(1f), Tooltip("The distance to avoid obstacles.")]
    private float obstacleAvoidanceDistance = 1f;
    
    
    [Header("Fleeing Values")]
    [SerializeField]
    private float safeDistance;
    
    
    
    public FishData[] PreyList => preyList;
    public FishData[] PredatorList => predatorList;
    
    public float WanderSpeed => wanderSpeed;
    public float RotationSpeed => rotationSpeed;
    public float MaxPitch => maxPitch;
    public float ObstacleAvoidanceDistance => obstacleAvoidanceDistance;
    
    public float SafeDistance => safeDistance;
}
