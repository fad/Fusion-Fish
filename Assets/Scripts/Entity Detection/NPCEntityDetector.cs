using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Entity detector for NPC objects
/// </summary>
public class NPCEntityDetector : EntityDetector
{
    [Header("Data settings for this NPC")]
    [SerializeField, Tooltip("The data for this fish, used for FOV stuff")]
    private FishData fishData;

    private HashSet<Transform> _otherNPCs = new();
    
    protected override void OnTriggerEnter(Collider other)
    {
        if (IsNotValid(other.gameObject)) return;
        
        DealWithHashset(other.gameObject);
    }

    protected override void OnTriggerExit(Collider other)
    {
        if (IsNotValid(other.gameObject)) return;
        
        DealWithHashset(other.gameObject, true);
    }
    
    private void Update()
    {
        Transform npcInFOV = _otherNPCs.FirstOrDefault(isInFOVAndInRange);
        
        if (npcInFOV is null) return;
        
        // Handle stuff
    }

    private void DealWithHashset(GameObject entity, bool shouldBeRemoved = false)
    {
        bool hasTreeRunner = entity.TryGetComponent(out ITreeRunner _);
        
        if (!hasTreeRunner) return;

        if (shouldBeRemoved)
        {
            _otherNPCs.Remove(entity.transform);
            return;
        }

        _otherNPCs.Add(entity.transform);
    }

    private bool isInFOVAndInRange(Transform target)
    {
        Vector3 directionToTarget = (target.position - transform.parent.position).normalized;
        float angleToTarget = Vector3.Angle(transform.parent.forward, directionToTarget);
        float distanceToTarget = Vector3.Distance(transform.parent.position, target.position);
        
        return angleToTarget < fishData.FOVAngle && distanceToTarget <= fishData.FOVRadius;
    }
}
