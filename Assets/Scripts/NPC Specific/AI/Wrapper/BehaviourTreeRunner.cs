using System;
using System.Linq;
using AI.BehaviourTree;
using Fusion;
using UnityEngine;

public class BehaviourTreeRunner : NetworkBehaviour, INPC, IInitialisable
{
    [Header("Settings")]
    [SerializeField,
     Tooltip("The data for this fish")]
    private FishData fishData;

    [SerializeField,
     Tooltip("The obstacle avoidance layer mask")]
    private LayerMask obstacleAvoidanceMask;

    /// <summary>
    /// The behaviour tree to execute on this object.
    /// </summary>
    private BehaviourTree _behaviourTreeToExecute;
    
    /// <summary>
    /// Whether the fish has been initialized.
    /// </summary>
    [Networked] private bool Initialized { get; set; }
    
    /// <summary>
    /// The name of the fish data to use for this fish.
    /// </summary>
    [Networked] private string FishDataName { get; set; }

    /// <summary>
    /// Whether the fish is inside a forbidden area.
    /// </summary>
    private bool _isInsideArea;

    /// <summary>
    /// The direction towards center of the area the fish is inside.
    /// </summary>
    private Vector3 _directionToArea;

    /// <summary>
    /// Whether the fish is in danger
    /// </summary>
    private bool _isInDanger;

    /// <summary>
    /// Whether the fish is locked onto prey.
    /// </summary>
    private bool _isHunting;


    /// <summary>
    /// The current target for hunt or flee behaviour.
    /// If the fish is hunting, the target will be followed.
    /// If the fish is fleeing, the fish will move away from the target.
    /// </summary>
    private Transform _target;

    /// <summary>
    /// The health manager to use for this fish.
    /// </summary>
    private HealthManager _healthManager;    
    
    /// <summary>
    /// The stamina manager to use for this fish.
    /// </summary>
    private IStaminaManager _staminaManager;

    /// <summary>
    /// The attack manager to use for this fish.
    /// </summary>
    private IAttackManager _attackManager;

    /// <summary>
    /// The target's health manager to subscribe and unsubscribe from.
    /// </summary>
    private IHealthManager _targetHealthManager;


    private Animator _animator;

    public FishData FishType => fishData;

    public BehaviourTree Tree => _behaviourTreeToExecute;
    
    public bool IsInDanger => _isInDanger;
    public bool IsHunting => _isHunting;

    private static readonly Color WanderingColor = new(120f / 255f, 33f / 255f, 114f / 255f, 1f);
    private static readonly Color FleeingColor = new(39f / 255f, 174f / 255f, 96f / 255f, 1f);
    private static readonly Color HuntingColor = new(231f / 255f, 76f / 255f, 60f / 255f, 1f);
    private static readonly int MovingSpeed = Animator.StringToHash("movingSpeed");

    public event Action<Transform> OnTargetChanged;

    public void Init(string fishDataName)
    {
        FishSpawnHandler.Instance.FishDataNameDictionary.TryGetValue(fishDataName, out fishData);

        if (!fishData)
            throw new NullReferenceException("<color=#9b59b6>FishData</color> is not found with name: " + fishDataName);
        
        Initialized = true;
        FishDataName = fishDataName;
    }

    // TODO: Possibly cache this to only create it once and use it for all fish of the same type
    public override void Spawned()
    {
        if (!fishData)
        {
            fishData = FishSpawnHandler.Instance.FishDataNameDictionary[FishDataName];
            if(!fishData)
                throw new NullReferenceException("<color=#9b59b6>FishData</color> is not found with name: " + FishDataName);
        }
        
        _staminaManager = GetComponentInChildren<IStaminaManager>();

        if (_staminaManager is null)
            throw new NullReferenceException("<color=#2980b9>StaminaManager</color> is not found in " + gameObject.name);

        _attackManager = GetComponentInChildren<IAttackManager>();

        if (_attackManager is null)
            throw new NullReferenceException("<color=#c0392b>AttackManager</color> is not found in " + gameObject.name);

        _healthManager = GetComponent<HealthManager>();
        
        if (_healthManager is null)
            throw new NullReferenceException("<color=#9b59b6>HealthManager</color> is not found in " + gameObject.name);
        
        if (!_animator)
        {
            _animator = GetComponentInChildren<Animator>();
        }
        
        // TODO: Make fish stick to their area
        
        _behaviourTreeToExecute = new BehaviourTree(gameObject.name);

        PrioritySelector actions = new PrioritySelector("Root");

        Sequence fleeSequence = new("Flee", 100);
        Leaf isInDanger = new Leaf("Is in danger?", new Condition(() => _isInDanger));

        Leaf fleeing = new Leaf("Fleeing",
            new FleeStrategy.Builder(transform)
                .WithNormalSpeed(fishData.WanderSpeed)
                .WithFastSpeed(fishData.FastSpeed)
                .WithStaminaThreshold(fishData.StaminaThreshold)
                .WithRotationSpeed(fishData.RotationSpeed)
                .WithMaxPitch(fishData.MaxPitch)
                .WithObstacleAvoidanceLayerMask(obstacleAvoidanceMask)
                .WithObstacleAvoidanceDistance(fishData.ObstacleAvoidanceDistance)
                .WithPredatorTransformGetter(() => _target)
                .WithResetThreatAction(ResetFleeBehaviour)
                .WithSafeDistance(fishData.SafeDistance)
                .WithStaminaManager(_staminaManager)
                .WithGraspable(_healthManager)
                .WithMarkedAreaCheck(IsInsideMarkedArea)
                .WithUseForward(true)
                .WithSpeedChangeCallback(SetAnimatorMoveSpeed)
                .Build()
        );

        fleeSequence.AddChild(isInDanger);
        fleeSequence.AddChild(fleeing);

        Sequence chaseSequence = new("Hunt", 60);
        Leaf isHunting = new Leaf("Is hunting?", new Condition(() => _isHunting));

        Leaf huntBehavior = new Leaf("Hunting",
            new ChaseStrategy.Builder(transform)
                .WithNormalSpeed(fishData.WanderSpeed)
                .WithFastSpeed(fishData.FastSpeed)
                .WithStaminaThreshold(fishData.StaminaThreshold)
                .WithStaminaManager(_staminaManager)
                .WithRotationSpeed(fishData.RotationSpeed)
                .WithMaxPitch(fishData.MaxPitch)
                .WithObstacleAvoidanceLayerMask(obstacleAvoidanceMask)
                .WithObstacleAvoidanceDistance(fishData.ObstacleAvoidanceDistance)
                .WithPreyTransformGetter(() => _target)
                .WithResetBehavior(ResetHuntBehaviour)
                .WithDistanceToLoseInterest(fishData.DistanceToLoseInterest)
                .WithTimeToLoseInterest(fishData.TimeToLoseInterest)
                .WithMarkedAreaCheck(IsInsideMarkedArea)
                .WithAttackManager(_attackManager)
                .WithAttackRange(fishData.AttackRange)
                .WithAttackValue(fishData.AttackValue)
                .WithDidPreyDie(DidPreyDie)
                .WithUseForward(true)
                .WithSpeedChangeCallback(SetAnimatorMoveSpeed)
                .Build()
        );


        chaseSequence.AddChild(isHunting);
        chaseSequence.AddChild(huntBehavior);
        
        Leaf wanderAround = new Leaf("Wander Around",
            new WanderStrategy.Builder(transform)
                .WithSpeed(fishData.WanderSpeed)
                .WithSpawnEntityPoint(transform.position)
                .WithWanderDistanceVertical(fishData.WanderDistanceVertical)
                .WithWanderDistanceHorizontal(fishData.WanderDistanceHorizontal)
                .WithRotationSpeed(fishData.RotationSpeed)
                .WithMaxPitch(fishData.MaxPitch)
                .WithObstacleAvoidanceLayerMask(obstacleAvoidanceMask)
                .WithObstacleAvoidanceDistance(fishData.ObstacleAvoidanceDistance)
                .WithMarkedAreaCheck(IsInsideMarkedArea)
                .WithUseForward(true)
                .WithSpeedChangeCallback(SetAnimatorMoveSpeed)
                .Build());


        actions.AddChild(fleeSequence);
        actions.AddChild(chaseSequence);
        actions.AddChild(wanderAround);

        _behaviourTreeToExecute.AddChild(actions);
        
        Initialized = true;
    }

    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority || !Initialized) return;
        
        _behaviourTreeToExecute?.Evaluate();
    }

    private (bool isInside, Vector3 direction) IsInsideMarkedArea()
    {
        return (isInside: _isInsideArea, direction: _directionToArea);
    }


    public void AdjustAreaCheck((bool isInside, Vector3 direction) areaCheck)
    {
        _isInsideArea = areaCheck.isInside;
        _directionToArea = areaCheck.direction;
    }

    public void AdjustHuntOrFleeTarget((Transform targetTransform, IEntity targetBehaviour) targetData)
    {
        if (_isHunting || _isInDanger) return;
        if (targetData.targetBehaviour.FishType == FishType) return; // if the other fish type is the same as this one

        if (FishType.PredatorList.Contains(targetData.targetBehaviour.FishType) && !_isHunting)
        {
            _target = targetData.targetTransform;
            _isInDanger = true;
        }

        if (FishType.PreyList.Contains(targetData.targetBehaviour.FishType) && !_isInDanger)
        {
            _target = targetData.targetTransform;
            _target.TryGetComponent(out _targetHealthManager);
            
            if (_targetHealthManager is not null)
            {
                _targetHealthManager.OnDeath += ResetHuntBehaviour;
            }

            _isHunting = true;
        }

        OnTargetChanged?.Invoke(_target);
    }

    private void ResetHuntBehaviour()
    {
        if (_targetHealthManager is not null)
        {
            _targetHealthManager.OnDeath -= ResetHuntBehaviour;
        }
        
        _isHunting = false;
        _isInDanger = false;
        _target = null;
        _targetHealthManager = null;
        OnTargetChanged?.Invoke(null);
    }

    private void ResetFleeBehaviour()
    {
        _isInDanger = false;
        _isHunting = false;
        _target = null;
        OnTargetChanged?.Invoke(null);
    }

    private void SetAnimatorMoveSpeed(float speed)
    {
        if(!_animator) return;
        
        _animator.SetFloat(MovingSpeed, speed);
    }

    private bool DidPreyDie()
    {
        return _targetHealthManager is null || _targetHealthManager.Died;
    }

}
