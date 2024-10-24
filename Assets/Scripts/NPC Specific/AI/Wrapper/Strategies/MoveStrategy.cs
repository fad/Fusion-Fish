using System;
using UnityEngine;
using AI.BehaviourTree;

public abstract class MoveStrategy : IStrategy
{
    protected Transform Entity;
    protected float RotationSpeed;
    protected float MaxPitch;
    protected LayerMask ObstacleAvoidanceLayerMask;
    protected float ObstacleAvoidanceDistance;
    protected float Speed;
    
    protected Quaternion TargetRotation;
    protected Func<(bool, Vector3)> ForbiddenAreaCheck;
    
    public abstract Status Process();
    
    public virtual void Reset(){}
    
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
