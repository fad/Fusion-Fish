using System;
using System.Linq;
using AI.BehaviourTree;
using Fusion;
using UnityEngine;

public class BehaviourTreeRunner : NetworkBehaviour, ITreeRunner, IEntity, IInitialisable
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

    private static readonly Color WanderingColor = new(120f / 255f, 33f / 255f, 114f / 255f, 1f);
    private static readonly Color FleeingColor = new(39f / 255f, 174f / 255f, 96f / 255f, 1f);
    private static readonly Color HuntingColor = new(231f / 255f, 76f / 255f, 60f / 255f, 1f);
    private static readonly int MovingSpeed = Animator.StringToHash("movingSpeed");


    public void Init(string fishDataName)
    {
        FishSpawnHandler.Instance.FishDataNameDictionary.TryGetValue(fishDataName, out fishData);

        if (!fishData)
            throw new NullReferenceException("<color=#9b59b6>FishData</color> is not found with name: " + fishDataName);
        
        _staminaManager = GetComponentInChildren<IStaminaManager>();

        if (_staminaManager is null)
            throw new NullReferenceException("<color=#2980b9>StaminaManager</color> is not found in " +
                                             gameObject.name);

        _attackManager = GetComponentInChildren<IAttackManager>();

        if (_attackManager is null)
            throw new NullReferenceException("<color=#c0392b>AttackManager</color> is not found in " + gameObject.name);
        
        Initialized = true;
        FishDataName = fishDataName;
    }

    // TODO: Possibly cache this to only create it once and use it for all fish of the same type
    public override void Spawned()
    {
        if (!fishData)
        {
            fishData = FishSpawnHandler.Instance.FishDataNameDictionary[FishDataName];
        }

        if (!_animator)
        {
            _animator = GetComponentInChildren<Animator>();
        }
        
        
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
                .WithForbiddenAreaCheck(IsInsideForbiddenArea)
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
                .WithForbiddenAreaCheck(IsInsideForbiddenArea)
                .WithAttackManager(_attackManager)
                .WithAttackRange(fishData.AttackRange)
                .WithAttackValue(fishData.AttackValue)
                .WithDidPreyDie(() => _targetHealthManager.Died)
                .WithUseForward(true)
                .WithSpeedChangeCallback(SetAnimatorMoveSpeed)
                .Build()
        );


        chaseSequence.AddChild(isHunting);
        chaseSequence.AddChild(huntBehavior);


        Leaf wanderAround = new Leaf("Wander Around",
            new WanderStrategy.Builder(transform)
                .WithSpeed(fishData.WanderSpeed)
                .WithRotationSpeed(fishData.RotationSpeed)
                .WithMaxPitch(fishData.MaxPitch)
                .WithObstacleAvoidanceLayerMask(obstacleAvoidanceMask)
                .WithObstacleAvoidanceDistance(fishData.ObstacleAvoidanceDistance)
                .WithForbiddenAreaCheck(IsInsideForbiddenArea)
                .WithUseForward(true)
                .WithSpeedChangeCallback(SetAnimatorMoveSpeed)
                .Build());


        actions.AddChild(fleeSequence);
        actions.AddChild(chaseSequence);
        actions.AddChild(wanderAround);

        _behaviourTreeToExecute.AddChild(actions);
    }

    private void Update()
    {
        if (!HasStateAuthority || !Initialized) return;

        _behaviourTreeToExecute?.Evaluate();
    }

    private (bool isInside, Vector3 direction) IsInsideForbiddenArea()
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

            return;
        }

        if (FishType.PreyList.Contains(targetData.targetBehaviour.FishType) && !_isInDanger)
        {
            _target = targetData.targetTransform;
            _target.TryGetComponent(out _targetHealthManager);

            _isHunting = true;
        }
    }

    private void ResetHuntBehaviour()
    {
        _isHunting = false;
        _target = null;
        _targetHealthManager = null;
    }

    private void ResetFleeBehaviour()
    {
        _isInDanger = false;
        _target = null;
    }

    private void SetAnimatorMoveSpeed(float speed)
    {
        if(!_animator) return;
        
        _animator.SetFloat(MovingSpeed, speed);
    }

}
