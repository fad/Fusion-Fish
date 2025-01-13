using System;
using AI.BehaviourTree;
using UnityEngine;

public class ChaseStrategy : StaminaMoveStrategy
{
    #region Fields from outside

    private readonly IAttackManager _attackManager;
    private readonly Func<Transform> _preyTransformGetter;
    private readonly Func<bool> _didPreyDie;
    private readonly float _attackValue;
    private readonly float _chanceToCatch;
    private readonly float _attackRange;
    private readonly float _distanceToLoseInterest;
    private readonly float _timeToLoseInterest;
    private readonly Action _resetBehavior;

    #endregion

    private Transform _preyTransform;
    private IHealthManager _preyHealthManager;
    private float _currentInterestTime;
    private bool _targetTransformDoesNotExist = false;

    public class Builder
    {
        public Transform Entity;
        public float NormalSpeed;
        public float FastSpeed;
        public float RotationSpeed;
        public float MaxPitch;
        public LayerMask ObstacleAvoidanceLayerMask;
        public float ObstacleAvoidanceDistance;
        public Func<Transform> PreyTransformGetter;
        public IStaminaManager StaminaManager;
        public HealthManager healthManager;
        public short StaminaThreshold;
        public Func<(bool, Vector3)> ForbiddenAreaCheck;
        public IAttackManager AttackManager;
        public float AttackValue;
        public float ChanceToCatch;
        public float AttackRange;
        public float TimeToLoseInterest;
        public float DistanceToLoseInterest;
        public Action ResetBehavior;
        public Func<bool> DidPreyDie;
        public bool UseForward;
        public Action<float> SpeedChangeCallback;

        public Builder(Transform entity)
        {
            Entity = entity;
        }

        public Builder WithNormalSpeed(float speed)
        {
            NormalSpeed = speed;
            return this;
        }

        public Builder WithFastSpeed(float speed)
        {
            FastSpeed = speed;
            return this;
        }

        public Builder WithRotationSpeed(float rotationSpeed)
        {
            RotationSpeed = rotationSpeed;
            return this;
        }

        public Builder WithMaxPitch(float maxPitch)
        {
            MaxPitch = maxPitch;
            return this;
        }

        public Builder WithObstacleAvoidanceLayerMask(LayerMask layerMask)
        {
            ObstacleAvoidanceLayerMask = layerMask;
            return this;
        }

        public Builder WithObstacleAvoidanceDistance(float distance)
        {
            ObstacleAvoidanceDistance = distance;
            return this;
        }

        public Builder WithPreyTransformGetter(Func<Transform> preyTransformGetter)
        {
            PreyTransformGetter = preyTransformGetter;
            return this;
        }

        public Builder WithStaminaManager(IStaminaManager staminaManager)
        {
            StaminaManager = staminaManager;
            return this;
        }

        public Builder WithMarkedAreaCheck(Func<(bool, Vector3)> forbiddenAreaCheck)
        {
            ForbiddenAreaCheck = forbiddenAreaCheck;
            return this;
        }

        public Builder WithStaminaThreshold(short threshold)
        {
            StaminaThreshold = threshold;
            return this;
        }

        public Builder WithAttackManager(IAttackManager attackManager)
        {
            AttackManager = attackManager;
            return this;
        }
        public Builder WithBiteStunDuration(float value)
        {
            ChanceToCatch = value;
            return this;
        }

        public Builder WithAttackValue(float value)
        {
            AttackValue = value;
            return this;
        }

        public Builder WithAttackRange(float range)
        {
            AttackRange = range;
            return this;
        }

        public Builder WithTimeToLoseInterest(float time)
        {
            TimeToLoseInterest = time;
            return this;
        }

        public Builder WithResetBehavior(Action resetBehavior)
        {
            ResetBehavior = resetBehavior;
            return this;
        }

        public Builder WithDistanceToLoseInterest(float distance)
        {
            DistanceToLoseInterest = distance;
            return this;
        }

        public Builder WithDidPreyDie(Func<bool> didPreyDie)
        {
            DidPreyDie = didPreyDie;
            return this;
        }

        public Builder WithUseForward(bool useForward)
        {
            UseForward = useForward;
            return this;
        }

        public Builder WithSpeedChangeCallback(Action<float> speedChangeCallback)
        {
            SpeedChangeCallback = speedChangeCallback;
            return this;
        }

        public ChaseStrategy Build()
        {
            return new ChaseStrategy(this);
        }
    }

    private ChaseStrategy(Builder builder) : base(
        builder.Entity,
        builder.RotationSpeed,
        builder.MaxPitch,
        builder.ObstacleAvoidanceLayerMask,
        builder.ObstacleAvoidanceDistance,
        builder.ForbiddenAreaCheck,
        builder.StaminaManager,
        builder.healthManager,
        builder.StaminaThreshold,
        builder.NormalSpeed,
        builder.FastSpeed,
        builder.UseForward,
        builder.SpeedChangeCallback)
    {
        _preyTransformGetter = builder.PreyTransformGetter;
        _attackManager = builder.AttackManager;
        _attackValue = builder.AttackValue;
        _chanceToCatch = builder.ChanceToCatch;
        _attackRange = builder.AttackRange;
        _timeToLoseInterest = builder.TimeToLoseInterest;
        _resetBehavior = builder.ResetBehavior;
        _distanceToLoseInterest = builder.DistanceToLoseInterest;
        _didPreyDie = builder.DidPreyDie;

        _currentInterestTime = _timeToLoseInterest;

        ForwardModifier = builder.UseForward ? (short)1 : (short)-1;
    }


    /// <summary>
    /// Processes the chase strategy for the entity. This includes checking if the prey has died,
    /// handling the timer logic for losing interest, avoiding forbidden areas and obstacles,
    /// checking stamina, rotating towards the prey, and attacking if within range.
    /// </summary>
    /// <returns><see cref="Status.Success"/> if the prey was successfully killed. <see cref="Status.Failure"/> if the prey managed to escape or the timer to lose interest finished.
    /// <see cref="Status.Running"/> during the chase.</returns>
    public override Status Process()
    {   
        if (_didPreyDie())
        {
            ResetValues();
            return Status.Success;
        }
        
        GetPreyTransform();
        
        if (_targetTransformDoesNotExist)
        {
            ResetValues();
            return Status.Success;
        }
        
        HandleTimerLogic();

        if (_currentInterestTime <= 0)
        {
            ResetValues();
            return Status.Failure;
        }

        AvoidMarkedArea();
        AvoidObstacles();

        CheckStamina();
        RotateToPrey();
        
        float sqrMagnitude = (Entity.position - _preyTransform.position).sqrMagnitude;

        if (sqrMagnitude <= _attackRange * _attackRange)
        {
            _attackManager.Attack(_attackValue, _chanceToCatch, _preyTransform); // TODO: Do an attack strategy
            
            if(sqrMagnitude > .25f)
            {
                Vector3 directionToPrey = _preyTransform.position - Entity.position;
                Vector3 forwardDirection = Entity.forward * (ForwardModifier * (Speed * Time.deltaTime));
                Move(forwardDirection);
            }
        }
        else
        {
            Vector3 forwardDirection = Entity.forward * (ForwardModifier * (Speed * Time.deltaTime));
            Move(forwardDirection);
        }
        
        if (_didPreyDie())
        {
            ResetValues();
            return Status.Success;
        }

        Entity.rotation = Quaternion.Slerp(Entity.rotation, TargetRotation, RotationSpeed * Time.deltaTime);

        return Status.Running;
    }

    /// <summary>
    /// Gets the prey transform if it is not already set.
    /// </summary>
    private void GetPreyTransform()
    {
        _preyTransform ??= _preyTransformGetter();

        if (!_preyTransform) _targetTransformDoesNotExist = true;
    }

    /// <summary>
    /// Rotate the fish to face the prey in order to move in its direction.
    /// </summary>
    private void RotateToPrey()
    {
        Vector3 direction = (_preyTransform.position - Entity.position).normalized;

        TargetRotation = Quaternion.LookRotation(direction);
    }

    /// <summary>
    /// Starts the timer to lose interest in the prey if the prey is too far away. Will stop it again
    /// if the prey is close enough again.
    /// </summary>
    private void HandleTimerLogic()
    {
        if ((Entity.position - _preyTransform.position).sqrMagnitude >=
            _distanceToLoseInterest * _distanceToLoseInterest)
        {
            _currentInterestTime -= Time.deltaTime;
        }
        else
        {
            _currentInterestTime = _timeToLoseInterest;
        }
    }

    private void ResetValues()
    {
        _resetBehavior();
        _preyTransform = null;
        _targetTransformDoesNotExist = false;
        _currentInterestTime = _timeToLoseInterest;
    }
}
