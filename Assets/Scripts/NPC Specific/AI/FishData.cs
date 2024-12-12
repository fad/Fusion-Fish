using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "New FishData", menuName = "Data/New Fish Data", order = 0)]
public class FishData : ScriptableObject
{
    [SerializeField, Tooltip("The ID of this fish.")]
    private short fishID;

    [Header("Prefab Settings")]
    [SerializeField, Tooltip("The prefab to use for the fish.")]
    private GameObject fishPrefab;

    [SerializeField, Tooltip("The scale of the fish.")]
    private float scale;

    [Header("General Values")]
    [SerializeField, Tooltip("The maximum Health of the entity.")]
    private float maxHealth = 10f;

    [SerializeField, Tooltip("The rate at which the Health will regenerate.")]
    private float recoveryHealthInSecond = 10;

    [SerializeField, Tooltip("Time to start regenerate.")]
    private float timeToStartRecoveryHealth = 3;

    [SerializeField, Tooltip("The maximum stamina of the entity.")]
    private short maxStamina = 10;

    [SerializeField, Tooltip("The rate at which the stamina will decrease.")]
    private short staminaDecreaseRate = 1;

    [SerializeField, Tooltip("The rate at which the stamina will regenerate.")]
    private short staminaRegenRate = 1;

    [SerializeField, Tooltip("The threshold for stamina at which the fish will start using it.")]
    private short staminaThreshold = 5;
    
    [SerializeField, Tooltip("The amount of XP the fish will give when killed.")]
    private short xpValue = 100;
    
    [SerializeField, Tooltip("The amount of gibs to spawn on death")]
    private short gibsSpawnValue = 2;

    [SerializeField, Min(30f), Tooltip("The angle of the field of view for the fish.")]
    private float FOV_Angle = 45f;

    [SerializeField, Min(3f), Tooltip("The detection radius inside of the FOV.")]
    private float FOV_Radius = 10f;

    [Header("Hunt Values")]
    [SerializeField, Tooltip("The list of prey for this fish. The fish will hunt these down on sight.")]
    private FishData[] preyList;

    [SerializeField,
     Tooltip("The list of predators for this fish. The fish will flee from these and avoid their areas if set.")]
    private FishData[] predatorList;

    [SerializeField, Min(1f), Tooltip("The damage value to use for attacks.")]
    private float attackValue = 1f;

    [SerializeField, Min(2f), Tooltip("The range at which the fish can attack.")]
    private float attackRange = 2f;

    [SerializeField, Tooltip("The cooldown for an attack in seconds.")]
    private float attackCooldown = 1f;

    [SerializeField, Min(1f), Tooltip("The time to lose interest in a target.")]
    private float timeToLoseInterest = 5f;

    [SerializeField, Min(1f), Tooltip("The distance to start losing interest in a target.")]
    private float distanceToLoseInterest = 20f;

    [Header("Movement Values")]
    [SerializeField, Min(0f), Tooltip("The speed at which the fish will wander around normally.")]
    private float wanderSpeed = 1f;

    [SerializeField, Min(0f), Tooltip("The speed at which the fish will flee when having stamina.")]
    private float fastSpeed = 1f;

    [SerializeField, Min(0f), Tooltip("The speed at which the fish will rotate.")]
    private float rotationSpeed = 1f;

    [SerializeField, Range(30f, 60f), Tooltip("The maximum pitch the fish can have.")]
    private float maxPitch;

    [SerializeField, Min(1f), Tooltip("The distance to avoid obstacles.")]
    private float obstacleAvoidanceDistance = 1f;
    
    [SerializeField,
     Tooltip("Еhe maximum horizontal range a fish can swim away from its spawn point.")]
    private float wanderDistanceHorizontal = 50f;    
    
    [SerializeField,
     Tooltip("Еhe maximum vertical range a fish can swim away from its spawn point.")]
    private float wanderDistanceVertical = 10f;

    [Header("Fleeing Values")]
    [SerializeField,
     Tooltip("How far away the fish shall flee in meters from the predator to be considered safe again.")]
    private float safeDistance = 50f;    
    
    public short FishID => fishID;
    public GameObject FishPrefab => fishPrefab;

    public float Scale => scale;

    public float MaxHealth => maxHealth;
    public float RecoveryHealthInSecond => recoveryHealthInSecond;
    public float TimeToStartRecoveryHealth => timeToStartRecoveryHealth;
    public short MaxStamina => maxStamina;
    public short StaminaDecreaseRate => staminaDecreaseRate;
    public short StaminaRegenRate => staminaRegenRate;

    public short StaminaThreshold => staminaThreshold;
    public short XPValue => xpValue;
    public short GibsSpawnValue => gibsSpawnValue;

    public float FOVAngle => FOV_Angle;
    public float FOVRadius => FOV_Radius;

    public FishData[] PreyList => preyList;
    public FishData[] PredatorList => predatorList;
    public float AttackValue => attackValue;
    public float AttackRange => attackRange;
    public float AttackCooldown => attackCooldown;
    public float TimeToLoseInterest => timeToLoseInterest;
    public float DistanceToLoseInterest => distanceToLoseInterest;

    public float WanderSpeed => wanderSpeed;
    public float FastSpeed => fastSpeed;
    public float RotationSpeed => rotationSpeed;
    public float MaxPitch => maxPitch;
    public float ObstacleAvoidanceDistance => obstacleAvoidanceDistance;

    public float SafeDistance => safeDistance;
    public float WanderDistanceVertical => wanderDistanceVertical;
    public float WanderDistanceHorizontal => wanderDistanceHorizontal;
}
