using UnityEngine;
using Fusion;

public class EntityDetector : NetworkBehaviour
{
    [Header("Entity Settings")]
    [SerializeField] protected LayerMask layerMaskToCheck;
    
    protected virtual void OnTriggerEnter(Collider other)
    {
    }
    
    protected virtual void OnTriggerExit(Collider other)
    {
    }
    
    protected bool IsEntity(GameObject obj)
    {
        return (layerMaskToCheck & (1<<obj.layer)) != 0;
    }
    
    protected bool IsSelf(GameObject obj)
    {
        return obj == gameObject;
    }
   protected bool IsHaveHealth(GameObject obj)
    {
        return obj.TryGetComponent<IHealthManager>(out IHealthManager healthManager);
    }
    protected bool IsNotValid(GameObject obj)
    {
        return IsSelf(obj) || !IsEntity(obj) || IsHaveHealth(obj);
    }
    
    
}
