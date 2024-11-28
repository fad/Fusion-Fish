using System;
using UnityEngine;
using Fusion;

public class EntityDetector : NetworkBehaviour
{
    [Header("Entity Settings")]
    [SerializeField]
    protected LayerMask layerMaskToCheck;

    protected virtual void OnTriggerEnter(Collider other)
    {
    }

    protected virtual void OnTriggerExit(Collider other)
    {
    }

    protected virtual void OnTriggerStay(Collider other)
    {
    }

    /// <summary>
    /// Checks if the given GameObject is an entity by comparing its layer with the specified layer mask.
    /// </summary>
    /// <param name="obj">The GameObject to check.</param>
    /// <returns>True if the GameObject's layer matches the layer mask, otherwise false.</returns>
    protected bool IsEntity(GameObject obj)
    {
        return (layerMaskToCheck & (1 << obj.layer)) != 0;
    }

    /// <summary>
    /// Checks if the given GameObject is the same as the current GameObject.
    /// </summary>
    /// <param name="obj">The GameObject to check.</param>
    /// <returns>True if the given GameObject is the same as the current GameObject, otherwise false.</returns>
    protected bool IsSelf(GameObject obj)
    {
        return obj == gameObject;
    }

    // protected bool IsHaveHealth(GameObject obj)
    //  {
    //      return obj.TryGetComponent<IHealthManager>(out IHealthManager healthManager);
    //  }

    protected bool IsNotValid(GameObject obj)
    {
        // return IsSelf(obj) || !IsEntity(obj) || IsHaveHealth(obj);
        return IsSelf(obj) || !IsEntity(obj);
    }

    /// <summary>
    /// Checks if the given GameObject is valid by ensuring the Runner is not null and the GameObject is not self or an invalid entity.
    /// </summary>
    /// <param name="obj">The GameObject to check.</param>
    /// <returns>True if the Runner is not null and the GameObject is valid, otherwise false.</returns>
    protected bool TryCheck(GameObject obj)
    {
        return !(Runner is null) && !IsNotValid(obj);
    }
}
