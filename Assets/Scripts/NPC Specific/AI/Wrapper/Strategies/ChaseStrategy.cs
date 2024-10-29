using System;
using AI.BehaviourTree;
using UnityEngine;

public class ChaseStrategy : StaminaMoveStrategy
{
    #region Fields from outside

    private IAttackManager _attackManager;
    private readonly Func<Transform> _preyTransformGetter;
    private readonly float _attackValue;
    private readonly float _attackRange;
    private readonly float _distanceToLoseInterest;
    private readonly CountdownTimer _timerToLoseInterest;
    private readonly Action _resetBehavior;
    
    
    #endregion
    
    private Transform _preyTransform;

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
        public short StaminaThreshold;
        public Func<(bool, Vector3)> ForbiddenAreaCheck;
        public IAttackManager AttackManager;
        public float AttackValue;
        public float AttackRange;
        public float TimeToLoseInterest;
        public float DistanceToLoseInterest;
        public Action ResetBehavior;

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

        public Builder WithForbiddenAreaCheck(Func<(bool, Vector3)> forbiddenAreaCheck)
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
        builder.StaminaThreshold,
        builder.NormalSpeed,
        builder.FastSpeed)
    {
        _preyTransformGetter = builder.PreyTransformGetter;
        _attackManager = builder.AttackManager;
        _attackValue = builder.AttackValue;
        _attackRange = builder.AttackRange;
        _timerToLoseInterest = new CountdownTimer(builder.TimeToLoseInterest);
        _resetBehavior = builder.ResetBehavior;
        _distanceToLoseInterest = builder.DistanceToLoseInterest;
    }


    public override Status Process()
    {
        GetPreyTransform();
        HandleTimerLogic();
        
        if(_timerToLoseInterest.IsFinished)
        {
            _resetBehavior();
            return Status.Failure;
        }
        
        AvoidForbiddenArea();
        AvoidObstacles();

        CheckStamina();
        RotateToPrey();
        
        if(Vector3.Distance(Entity.position, _preyTransform.position) <= _attackRange)
        {
            _attackManager.Attack(_attackValue);
        }
        else
        {
            Vector3 forwardDirection = Entity.forward * (Speed * Time.deltaTime);
            Move(forwardDirection);    
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
        if(Vector3.Distance(Entity.position, _preyTransform.position) >= _distanceToLoseInterest)
        {
            _timerToLoseInterest.Start();
        }
        else
        {
            _timerToLoseInterest.Stop();
        }
    }
}
