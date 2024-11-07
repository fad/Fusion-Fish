using System.Linq;
using UnityEngine;

public class AreaMarker : MonoBehaviour
{
    [SerializeField,
    Tooltip("The fish this area marker belongs to.")]
    private FishData fishData;
    
    [SerializeField,
     Tooltip("Whether every fish should avoid this area.")]
    private bool avoidedByAll;
    
    private void OnTriggerEnter(Collider other)
    {
        ChangeAreaCheck(other, true);
    }

    private void OnTriggerExit(Collider other)
    {
        ChangeAreaCheck(other, false);
    }

    private void ChangeAreaCheck(Collider other, bool isInside)
    {
        if (avoidedByAll || (other.gameObject.TryGetComponent(out IEntity entity) && fishData.PreyList.Contains(entity.FishType)))
        {
            other.TryGetComponent(out ITreeRunner treeRunner);
            
            // The direction is from the other object to this object.
            Vector3 direction = Vector3.Normalize(transform.position - other.transform.position);
            treeRunner?.AdjustAreaCheck((isInside, direction));
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
