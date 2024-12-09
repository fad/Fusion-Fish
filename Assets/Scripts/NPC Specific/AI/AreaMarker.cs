using System.Linq;
using UnityEngine;

public class AreaMarker : MonoBehaviour, IInitialisable
{
    [SerializeField,
    Tooltip("The fish this area marker belongs to.")]
    private FishData fishData;
    
    [SerializeField,
     Tooltip("Whether every fish should avoid this area.")]
    private bool avoidedByAll;
    
    private void OnTriggerEnter(Collider other)
    {
        if (!fishData) return;
        
        ChangeAreaCheck(other, true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!fishData) return;
        
        ChangeAreaCheck(other, false);
    }

    /// <summary>
    /// Determines whether the fish should avoid the area or not.
    /// </summary>
    /// <param name="other">The collider that entered / exited the trigger</param>
    /// <param name="shouldNotTurnAround">Whether the fish should turn around or not.</param>
    private void ChangeAreaCheck(Collider other, bool shouldNotTurnAround)
    {
        if (!other.gameObject.TryGetComponent(out INPC entity)) return;
        
        if (avoidedByAll || fishData.PreyList.Contains(entity.FishType))
        {
            shouldNotTurnAround = false;
        }
        
        // The direction is from the other object to this object.
        Vector3 direction = Vector3.Normalize(transform.position - other.transform.position);
        entity.AdjustAreaCheck((shouldNotTurnAround, direction));
    }

    private void OnDrawGizmos()
    {
        TryGetComponent(out BoxCollider boxCollider);

        if (!boxCollider) return;

        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, boxCollider.size);
    }

    public void Init(string fishDataName)
    {
        fishData = Resources.Load<FishData>($"FishData/{fishDataName}");
        
        if (!fishData)
        {
            Debug.LogError($"Fish data with name {fishDataName} not found!");
        }
    }
}
