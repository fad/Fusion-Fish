using System;
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

    private ITreeRunner _attachedAIBehaviour;

    private readonly HashSet<(Transform entity, ITreeRunner entityTreeRunner)> _otherNPCs = new();

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

    private void Start()
    {
        transform.parent.TryGetComponent(out _attachedAIBehaviour);

        if (_attachedAIBehaviour is null)
            throw new NullReferenceException("No AI behaviour found on " + gameObject.name);
    }

    private void Update()
    {
        (Transform entity, ITreeRunner entityTreeRunner) npcInFOV =
            _otherNPCs.FirstOrDefault(npc => IsInFOVAndInRange(npc.entity));

        if (npcInFOV.entity is null || npcInFOV.entityTreeRunner is null) return;

        _attachedAIBehaviour.AdjustHuntOrFleeTarget(npcInFOV);
    }

    private void OnDrawGizmos()
    {
        bool hasSpereCollider = TryGetComponent(out SphereCollider sphereCollider);

        if (!hasSpereCollider) return;

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, sphereCollider.radius);
    }

    private void DealWithHashset(GameObject entity, bool shouldBeRemoved = false)
    {
        bool hasTreeRunner = entity.TryGetComponent(out ITreeRunner behaviour);

        if (!hasTreeRunner) return;

        if (shouldBeRemoved)
        {
            _otherNPCs.Remove((entity: entity.transform, entityTreeRunner: behaviour));
            return;
        }

        _otherNPCs.Add((entity: entity.transform, entityTreeRunner: behaviour));
    }

    private bool IsInFOVAndInRange(Transform target)
    {
        Vector3 directionToTarget = (target.position - transform.parent.position).normalized;
        float angleToTarget = Vector3.Angle(transform.parent.forward, directionToTarget);
        float distanceToTarget = Vector3.Distance(transform.parent.position, target.position);

        return angleToTarget < fishData.FOVAngle && distanceToTarget <= fishData.FOVRadius;
    }
}
