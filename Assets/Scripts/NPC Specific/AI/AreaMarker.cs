using System.Linq;
using UnityEngine;

public class AreaMarker : MonoBehaviour
{
    [SerializeField,
    Tooltip("The fish this area marker belongs to.")]
    private FishData fishData;
    
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
        if (other.gameObject.TryGetComponent(out ITreeRunner treeRunner) && fishData.PreyList.Contains(treeRunner.FishType))
        {
            // The direction is from the other object to this object.
            Vector3 direction = Vector3.Normalize(transform.position - other.transform.position);
            treeRunner.AdjustAreaCheck((isInside, direction));
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
