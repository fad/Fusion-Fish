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
    protected readonly Func<(bool, Vector3)> MarkedAreaCheck;
    protected readonly bool UseForward;
    protected readonly Action<float> SpeedChangeCallback;
    protected float Speed;
    protected Quaternion TargetRotation;
    protected bool isRotating;
    protected short ForwardModifier;
    public float obstacleAvoidanceTime;

    protected MoveStrategy(Transform entity,
        float rotationSpeed,
        float maxPitch,
        LayerMask obstacleAvoidanceLayerMask,
        float obstacleAvoidanceDistance,
        Func<(bool, Vector3)> markedAreaCheck,
        bool useForward,
        Action<float> speedChangeCallback)
    {
        Entity = entity;
        RotationSpeed = rotationSpeed;
        MaxPitch = maxPitch;
        ObstacleAvoidanceLayerMask = obstacleAvoidanceLayerMask;
        ObstacleAvoidanceDistance = obstacleAvoidanceDistance;
        MarkedAreaCheck = markedAreaCheck;
        UseForward = useForward;
        SpeedChangeCallback = speedChangeCallback;
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
    protected virtual void AvoidMarkedArea()
    {
        (bool isInside, Vector3 direction) result = MarkedAreaCheck();

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
        if (obstacleAvoidanceTime > 0)
            obstacleAvoidanceTime -= Time.deltaTime;

        Vector3 targetDirection = TargetRotation * Vector3.forward;
        float angle = Vector3.Angle(Entity.forward, targetDirection);

        Vector3 nextDirection = Entity.forward;
        RaycastHit hit;

        if (Raycast())
        {
            if (!isRotating || angle <= 10)
            {
                int attempts = 0;
                while (Raycast())
                {
                    attempts++;
                    if (attempts >= 10)
                        break;
                }

                isRotating = true;
                TargetRotation = Quaternion.LookRotation(nextDirection);
                obstacleAvoidanceTime = 1;
            }
        }
        else
        {
            isRotating = false;
        }

        bool Raycast()
        {
            bool Touch = Physics.Raycast(Entity.position, nextDirection, out hit, ObstacleAvoidanceDistance,
                                     ObstacleAvoidanceLayerMask, QueryTriggerInteraction.Collide);

            if (Touch && hit.normal != Vector3.zero)
            {
                nextDirection = Vector3.Reflect(nextDirection, hit.normal);
            }
            return Touch;
        }
    }
}
