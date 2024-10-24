using System;
using AI.BehaviourTree;
using UnityEngine;

public class ChaseStrategy : MoveStrategy
{
    #region Fields from outside

    #endregion


    public ChaseStrategy(Transform entity, 
        float rotationSpeed, 
        float maxPitch, 
        LayerMask obstacleAvoidanceLayerMask,
        float obstacleAvoidanceDistance, 
        Func<(bool, Vector3)> forbiddenAreaCheck) 
        : base(
        entity, 
        rotationSpeed,
        maxPitch, 
        obstacleAvoidanceLayerMask, 
        obstacleAvoidanceDistance, 
        forbiddenAreaCheck)
    {
    }

    public override Status Process()
    {
        throw new System.NotImplementedException();
    }
}
