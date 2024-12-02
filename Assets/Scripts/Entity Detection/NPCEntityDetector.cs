using System;
using Fusion;
using UnityEngine;

/// <summary>
/// Entity detector for NPC objects
/// </summary>
public class NPCEntityDetector : EntityDetector, IInitialisable
{
    [Header("Setup Settings")]
    [SerializeField, Tooltip("The data for this fish, used for FOV stuff")]
    private FishData fishData;

    [SerializeField, Tooltip("The root of this object")]
    private Transform root;

    private ITreeRunner _attachedAIBehaviour;
    

    protected override void OnTriggerEnter(Collider other)
    {
        DealWithHuntAdjustment(other);
    }

    protected override void OnTriggerExit(Collider other)
    {
        DealWithHuntAdjustment(other);
    }

    protected override void OnTriggerStay(Collider other)
    {
        DealWithHuntAdjustment(other);
    }

    public override void Spawned()
    {
        root.TryGetComponent(out _attachedAIBehaviour);

        if (_attachedAIBehaviour is null)
            throw new NullReferenceException("No <color=#16a085>AI behaviour (ITreeRunner)</color> found on " +
                                             root.name);
    }

    private void OnDrawGizmos()
    {
        bool hasSphereCollider = TryGetComponent(out SphereCollider sphereCollider);

        if (!hasSphereCollider) return;

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, sphereCollider.radius);
    }

    private bool IsInFOVAndInRange(Transform target)
    {
        Vector3 directionToTarget = (target.position - transform.parent.position).normalized;
        float angleToTarget = Vector3.Angle(transform.parent.forward, directionToTarget);
        float distanceToTarget = Vector3.Distance(transform.parent.position, target.position); // TODO: Use sqrMagnitude

        return angleToTarget < fishData.FOVAngle && distanceToTarget <= fishData.FOVRadius;
    }

    private void DealWithHuntAdjustment(Collider other)
    {
        if (!TryCheck(other.gameObject)) return;

        if (!other.TryGetComponent(out NetworkTransform networkTransform) ||
            !other.TryGetComponent(out IEntity entity)) return;

        // if (OtherNPCs.Contains(networkTransform)) return;

        if (!IsInFOVAndInRange(networkTransform.transform)) return;
        
        _attachedAIBehaviour.AdjustHuntOrFleeTarget((networkTransform.transform, entity));
    }

    public void Init(string fishDataName)
    {
        fishData = Resources.Load<FishData>($"FishData/{fishDataName}");
    }
}
