using System;
using UnityEngine;
using AI.BehaviourTree;

public abstract class MoveStrategy : IStrategy
{
    protected readonly Transform Entity;
    protected readonly float RotationSpeed;
    protected readonly float MaxPitch;
    protected readonly LayerMask ObstacleAvoidanceLayerMask;
    protected readonly float ObstacleAvoidanceDistance;
    protected readonly Func<(bool, Vector3)> ForbiddenAreaCheck;
    protected readonly bool UseForward;

    protected float Speed;
    protected Quaternion TargetRotation;
    protected short ForwardModifier;

    protected MoveStrategy(Transform entity, 
        float rotationSpeed, 
        float maxPitch, 
        LayerMask obstacleAvoidanceLayerMask,
        float obstacleAvoidanceDistance, 
        Func<(bool, Vector3)> forbiddenAreaCheck,
        bool useForward)
    {
        Entity = entity;
        RotationSpeed = rotationSpeed;
        MaxPitch = maxPitch;
        ObstacleAvoidanceLayerMask = obstacleAvoidanceLayerMask;
        ObstacleAvoidanceDistance = obstacleAvoidanceDistance;
        ForbiddenAreaCheck = forbiddenAreaCheck;
        UseForward = useForward;
    }

    public abstract Status Process();

    public virtual void Reset()
    {
    }

    /// <summary>
    /// Checks if the entity is inside a forbidden area and adjusts the target rotation to move away from it.
    /// If the entity is inside the forbidden area, the target rotation is inverted to move in the opposite direction.
    /// The change interval is set to the maximum value to avoid frequent direction changes.
    /// </summary>
    protected virtual void AvoidForbiddenArea()
    {
        (bool isInside, Vector3 direction) result = ForbiddenAreaCheck();

        if (result.isInside)
        {
            TargetRotation = Quaternion.LookRotation(-result.direction, Entity.up);
        }
    }

    /// <summary>
    /// Checks for obstacles in the entity's forward direction and adjusts the target rotation to avoid them.
    /// </summary>
    protected virtual void AvoidObstacles()
    {
        if (Physics.Raycast(Entity.position, Entity.forward, out RaycastHit hit, ObstacleAvoidanceDistance,
                ObstacleAvoidanceLayerMask))
        {
            TargetRotation = Quaternion.LookRotation(Vector3.Reflect(Entity.forward, hit.normal));
        }
    }
}
