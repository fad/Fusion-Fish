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
        float fastSpeed,
        bool useForward, 
        Action<float> speedChangeCallback)
        : base(
            entity,
            rotationSpeed,
            maxPitch,
            obstacleAvoidanceLayerMask,
            obstacleAvoidanceDistance,
            forbiddenAreaCheck,
            useForward,
            speedChangeCallback)
    {
        StaminaManager = staminaManager;
        StaminaThreshold = staminaThreshold;
        NormalSpeed = normalSpeed;
        FastSpeed = fastSpeed;
    }
    
    /// <summary>
    /// Constantly moves the entity and decreases the stamina if a StaminaManager is set.
    /// </summary>
    /// <param name="forwardDirection">The direction to move in</param>
    protected virtual void Move(Vector3 forwardDirection)
    {
        Entity.position += forwardDirection;

        if (!UsesStamina) return;

        StaminaManager?.Decrease();
    }

    /// <summary>
    /// Checks the current stamina level and adjusts the speed accordingly.
    /// If the current stamina is above the threshold, the entity can use stamina and moves at a faster speed.
    /// Otherwise, the entity cannot use stamina and moves at a normal speed.
    /// </summary>
    protected virtual void CheckStamina()
    {
        if (UsesStamina)
        {
            if (StaminaManager.CurrentStamina > 0)
            {
                Speed = FastSpeed;
            }
            else
            {
                UsesStamina = false;
                Speed = NormalSpeed;
            }
            
            SpeedChangeCallback?.Invoke(Speed);
            return;
        }

        if (StaminaManager.CurrentStamina > StaminaThreshold)
        {
            UsesStamina = true;
            Speed = FastSpeed;
        }
        else
        {
            Speed = NormalSpeed;
        }
        
        SpeedChangeCallback?.Invoke(Speed);
    }
}
