using System;
using System.Linq;
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

    [Networked, Capacity(30)]
    public NetworkLinkedList<NetworkTransform> OtherNPCs => default;

    protected override void OnTriggerEnter(Collider other)
    {
        // if (!TryCheck(other.gameObject)) return;
        //
        // if (!other.TryGetComponent(out NetworkTransform networkTransform)) return;
        //
        // Debug.Log("Adding to set");
        // DealWithListRpc(networkTransform);
        
        DealWithHuntAdjustment(other);
    }

    protected override void OnTriggerExit(Collider other)
    {
        // if (!TryCheck(other.gameObject)) return;
        //
        // if (!other.TryGetComponent(out NetworkTransform networkTransform)) return;
        //
        // DealWithListRpc(networkTransform, true);
        
        DealWithHuntAdjustment(other);
    }

    protected override void OnTriggerStay(Collider other)
    {
        // if (!TryCheck(other.gameObject)) return;
        //
        // if (!other.TryGetComponent(out NetworkTransform networkTransform)) return;
        //
        // if (OtherNPCs.Contains(networkTransform)) return;
        //
        // DealWithListRpc(networkTransform);
        
        DealWithHuntAdjustment(other);
    }

    public override void Spawned()
    {
        root.TryGetComponent(out _attachedAIBehaviour);

        if (_attachedAIBehaviour is null)
            throw new NullReferenceException("No <color=#16a085>AI behaviour (ITreeRunner)</color> found on " +
                                             root.name);
    }

    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority) return;

        var npcInFOV =
            OtherNPCs.FirstOrDefault(npc => IsInFOVAndInRange(npc.transform));

        if (npcInFOV is null || !npcInFOV.gameObject.TryGetComponent(out IEntity entity)) return;

        _attachedAIBehaviour.AdjustHuntOrFleeTarget((npcInFOV.transform, entity));
    }

    private void OnDrawGizmos()
    {
        bool hasSphereCollider = TryGetComponent(out SphereCollider sphereCollider);

        if (!hasSphereCollider) return;

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, sphereCollider.radius);
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    private void DealWithListRpc(NetworkTransform entity, bool shouldBeRemoved = false)
    {
        bool isEntity = entity.gameObject.TryGetComponent(out IHealthManager healthManager);

        if (!isEntity) return;

        Action onDeathRemoval = () => RemoveFromSetOnDeathRpc(entity);

        if (shouldBeRemoved)
        {
            OtherNPCs.Remove(entity);


            if (healthManager is null) return;

            healthManager.OnDeath -= onDeathRemoval;
            return;
        }

        OtherNPCs.Add(entity);

        if (healthManager is null) return;
        healthManager.OnDeath += onDeathRemoval;
    } // The exception is pointing here

    private bool IsInFOVAndInRange(Transform target)
    {
        Vector3 directionToTarget = (target.position - transform.parent.position).normalized;
        float angleToTarget = Vector3.Angle(transform.parent.forward, directionToTarget);
        float distanceToTarget = Vector3.Distance(transform.parent.position, target.position);

        return angleToTarget < fishData.FOVAngle && distanceToTarget <= fishData.FOVRadius;
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    private void RemoveFromSetOnDeathRpc(NetworkTransform entity)
    {
        OtherNPCs.Remove(entity);
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
