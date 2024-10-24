using System;
using UnityEngine;

public abstract class StaminaMoveStrategy : MoveStrategy
{
    protected readonly IStaminaManager StaminaManager;
    protected readonly short StaminaThreshold;
    protected readonly float NormalSpeed;
    protected readonly float FastSpeed;

    protected bool UsesStamina;


    protected StaminaMoveStrategy(Transform entity,
        float rotationSpeed,
        float maxPitch,
        LayerMask obstacleAvoidanceLayerMask,
        float obstacleAvoidanceDistance,
        Func<(bool, Vector3)> forbiddenAreaCheck,
        IStaminaManager staminaManager,
        short staminaThreshold,
        float normalSpeed,
        float fastSpeed)
        : base(
            entity, 
            rotationSpeed,
            maxPitch, 
            obstacleAvoidanceLayerMask, 
            obstacleAvoidanceDistance,
            forbiddenAreaCheck)
    {
        StaminaManager = staminaManager;
        StaminaThreshold = staminaThreshold;
        NormalSpeed = normalSpeed;
        FastSpeed = fastSpeed;
    }

    /// <summary>
    /// Checks the current stamina level and adjusts the speed accordingly.
    /// If the current stamina is above the threshold, the entity can use stamina and moves at a faster speed.
    /// Otherwise, the entity cannot use stamina and moves at a normal speed.
    /// </summary>
    protected virtual void CheckStamina()
    {
        if (StaminaManager.CurrentStamina > StaminaThreshold)
        {
            UsesStamina = true;
            Speed = FastSpeed;
            return;
        }

        UsesStamina = false;
        Speed = NormalSpeed;
    }
}
