using System;
using UnityEngine;
using AI.BehaviourTree;

public class FleeStrategy : StaminaMoveStrategy
{
    #region Fields from outside

    private readonly Func<Transform> _predatorTransformGetter;
    private readonly Action _resetThreatAction;
    private readonly float _safeDistance;

    #endregion

    private Transform _predatorTransform;

    public class Builder
    {
        public Transform Entity;
        public float NormalSpeed;
        public float FastSpeed;
        public float RotationSpeed;
        public float MaxPitch;
        public LayerMask ObstacleAvoidanceLayerMask;
        public float ObstacleAvoidanceDistance;
        public Func<Transform> PredatorTransformGetter;
        public IStaminaManager StaminaManager;
        public IGraspable Graspable;
        public Action ResetThreatAction;
        public float SafeDistance;
        public short StaminaThreshold;
        public Func<(bool, Vector3)> ForbiddenAreaCheck;
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

        public Builder WithStaminaThreshold(short threshold)
        {
            StaminaThreshold = threshold;
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

        public Builder WithGraspable(IGraspable graspable)
        {
            Graspable = graspable;
            return this;
        }

        public Builder WithMarkedAreaCheck(Func<(bool, Vector3)> forbiddenAreaCheck)
        {
            ForbiddenAreaCheck = forbiddenAreaCheck;
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

        public FleeStrategy Build()
        {
            return new FleeStrategy(this);
        }
    }

    private FleeStrategy(Builder builder) : base(builder.Entity,
        builder.RotationSpeed,
        builder.MaxPitch,
        builder.ObstacleAvoidanceLayerMask,
        builder.ObstacleAvoidanceDistance,
        builder.ForbiddenAreaCheck,
        builder.StaminaManager,
        builder.Graspable,
        builder.StaminaThreshold,
        builder.NormalSpeed,
        builder.FastSpeed,
        builder.UseForward,
        builder.SpeedChangeCallback)
    {
        _predatorTransformGetter = builder.PredatorTransformGetter;
        _resetThreatAction = builder.ResetThreatAction;
        _safeDistance = builder.SafeDistance;

        ForwardModifier = builder.UseForward ? (short)1 : (short)-1;
    }

    /// <summary>
    /// Flees from the predator by moving in the opposite direction of it. Uses stamina if the current stamina is above the threshold.
    /// </summary>
    /// <returns>
    /// Returns <see cref="Status.Success"/> if the entity is at a safe distance from the predator,
    /// otherwise returns <see cref="Status.Running"/>.
    /// </returns>
    public override Status Process()
    {
        GetPredatorTransform();

        if ((Entity.position - _predatorTransform.position).sqrMagnitude >= _safeDistance * _safeDistance)
        {
            _resetThreatAction();
            return Status.Success;
        }
        StayInMarkedArea();
        RotateToOppositeDirection();
        AvoidObstacles();

        if (Graspable.IsGrasped)
        {
            AttractionToPredator();
            return Status.Running;
        }

        CheckStamina();

        Vector3 forwardDirection = Entity.forward * (ForwardModifier * (Speed * Time.deltaTime));

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
    /// Attraction to the predator if the entity is caught.
    /// </summary>
    private void AttractionToPredator()
    {
        Vector3 directionToPredator = _predatorTransform.position - Entity.position;
        Vector3 newEntityDirection = directionToPredator.normalized * (ForwardModifier * (Speed * Time.deltaTime));
        
        if ((Entity.position - _predatorTransform.position).sqrMagnitude >= 1f)
            Move(newEntityDirection);
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
}
