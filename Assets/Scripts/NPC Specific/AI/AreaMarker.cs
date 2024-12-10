using System.Collections.Generic;
using UnityEngine;

public class AreaMarker : MonoBehaviour
{
    [SerializeField,
     Tooltip("The fishes that should not cross this area marker.")]
    private FishData[] fishDataToUse;
    
    [SerializeField,
     Tooltip("Whether every fish should avoid this area.")]
    private bool avoidedByAll;

    private HashSet<FishData> _fishesToReflect;

    private void Start()
    {
        _fishesToReflect = new HashSet<FishData>(fishDataToUse);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (fishDataToUse.Length <= 0) return;
        
        ChangeAreaCheck(other, true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (fishDataToUse.Length <= 0) return;
        
        ChangeAreaCheck(other, false);
    }
    
    private void ChangeAreaCheck(Collider other, bool isInside)
    {
        bool hasFishData = other.gameObject.TryGetComponent(out INPC entity);
        
        if (avoidedByAll || (hasFishData && _fishesToReflect.Contains(entity.FishType)))
        {
            // The direction is from the other object to this object.
            Vector3 direction = Vector3.Normalize(transform.position - other.transform.position);
            entity.AdjustAreaCheck((isInside, direction));
        }
    }

    private void OnDrawGizmos()
    {
        TryGetComponent(out BoxCollider boxCollider);

        if (!boxCollider) return;

        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, boxCollider.size);
    }
}
