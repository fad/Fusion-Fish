using System;
using UnityEngine;
using AI.BehaviourTree;

public class FleeStrategy : MoveStrategy
{
    #region Fields from outside
    
    private readonly Func<Transform> _predatorTransformGetter;
    private readonly IStaminaManager _staminaManager;
    private readonly Action _resetThreatAction;
    private readonly float _safeDistance;
    
    #endregion
    
    private Transform _predatorTransform;
    
    public class Builder
    {
        public Transform Entity;
        public float Speed;
        public float RotationSpeed;
        public float MaxPitch;
        public LayerMask ObstacleAvoidanceLayerMask;
        public float ObstacleAvoidanceDistance;
        public Func<Transform> PredatorTransformGetter;
        public IStaminaManager StaminaManager;
        public Action ResetThreatAction;
        public float SafeDistance;
        
        public Builder(Transform entity)
        {
            Entity = entity;
        }
        
        public Builder WithSpeed(float speed)
        {
            Speed = speed;
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
        
        public Builder WithObstacleAvoidanceLayerMask(LayerMask obstacleAvoidanceLayerMask)
        {
            ObstacleAvoidanceLayerMask = obstacleAvoidanceLayerMask;
            return this;
        }
        
        public Builder WithObstacleAvoidanceDistance(float obstacleAvoidanceDistance)
        {
            ObstacleAvoidanceDistance = obstacleAvoidanceDistance;
            return this;
        }
        
        public Builder WithPredatorTransformGetter(Func<Transform> predatorTransformGetter)
        {
            PredatorTransformGetter = predatorTransformGetter;
            return this;
        }
        
        public Builder WithStaminaManager(IStaminaManager staminaManager)
        {
            StaminaManager = staminaManager;
            return this;
        }
        
        public Builder WithResetThreatAction(Action resetThreatAction)
        {
            ResetThreatAction = resetThreatAction;
            return this;
        }
        
        public Builder WithSafeDistance(float safeDistance)
        {
            SafeDistance = safeDistance;
            return this;
        }
        
        public FleeStrategy Build()
        {
            return new FleeStrategy(this);
        }
    }
    
    private FleeStrategy(Builder builder)
    {
        Speed = builder.Speed;
        Entity = builder.Entity;
        RotationSpeed = builder.RotationSpeed;
        MaxPitch = builder.MaxPitch;
        ObstacleAvoidanceLayerMask = builder.ObstacleAvoidanceLayerMask;
        ObstacleAvoidanceDistance = builder.ObstacleAvoidanceDistance;
        _predatorTransformGetter = builder.PredatorTransformGetter;
        _staminaManager = builder.StaminaManager;
        _resetThreatAction = builder.ResetThreatAction;
        _safeDistance = builder.SafeDistance;
    }
    
    public override Status Process()
    {
        if(Vector3.Distance(Entity.position, _predatorTransform.position) > _safeDistance)
        {
            _resetThreatAction();
            return Status.Success;
        }
        
        AvoidForbiddenArea();
        AvoidObstacles();
        
        GetPredatorTransform();
        RotateToOppositeDirection();
        
        Vector3 forwardDirection = Entity.forward * (Speed * Time.deltaTime);

        Move(forwardDirection);
        
        Entity.rotation = Quaternion.Slerp(Entity.rotation, TargetRotation, RotationSpeed * Time.deltaTime);

        return Status.Running;
    }
    
    /// <summary>
    /// Gets the predator transform if it is not already set.
    /// </summary>
    private void GetPredatorTransform()
    {
        _predatorTransform ??= _predatorTransformGetter();
    }

    /// <summary>
    /// Rotates the entity to the opposite direction of the predator.
    /// </summary>
    private void RotateToOppositeDirection()
    {
        Vector3 directionToPredator = _predatorTransform.position - Entity.position;
        
        Vector3 oppositeDirection = -directionToPredator.normalized;
        
        TargetRotation = Quaternion.LookRotation(oppositeDirection);
    }

    /// <summary>
    /// Constantly moves the entity and decreases the stamina if a StaminaManager is set.
    /// </summary>
    /// <param name="forwardDirection"></param>
    private void Move(Vector3 forwardDirection)
    {
        Entity.position += forwardDirection;

        if (_staminaManager is null) return;
        
        _staminaManager.Decrease();
    }
}