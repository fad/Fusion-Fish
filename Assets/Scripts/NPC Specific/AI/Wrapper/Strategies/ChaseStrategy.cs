using System;
using AI.BehaviourTree;
using UnityEngine;

public class ChaseStrategy : StaminaMoveStrategy
{
    #region Fields from outside

    private readonly Func<Transform> _preyTransformGetter;
    
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
    }


    public override Status Process()
    {
        GetPreyTransform();
        
        // TODO: status success condition
        // Success: Prey is caught
        // Failure: Prey is lost after X seconds
        
        AvoidForbiddenArea();
        AvoidObstacles();

        CheckStamina();
        RotateToPrey();
        
        Vector3 forwardDirection = Entity.forward * (Speed * Time.deltaTime);

        Move(forwardDirection);

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
    
    private void RotateToPrey()
    {
        Vector3 direction = (_preyTransform.position - Entity.position).normalized;
        
        TargetRotation = Quaternion.LookRotation(direction);
    }
}
