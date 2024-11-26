using System;
using System.Collections.Generic;
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
    
    
    [Networked] [Capacity(100)] public NetworkLinkedList<NetworkTransform> _otherNPCsNetworked {get;} = new();

    protected override void OnTriggerEnter(Collider other)
    {
        if (IsNotValid(other.gameObject)) return;
        
        if(other.TryGetComponent(out NetworkTransform networkTransform))
            DealWithHashsetRpc(networkTransform);
    }

    protected override void OnTriggerExit(Collider other)
    {
        if (IsNotValid(other.gameObject)) return;

        if(other.TryGetComponent(out NetworkTransform networkTransform))
            DealWithHashsetRpc(networkTransform, true);
    }

    private void Start()
    {
        root.TryGetComponent(out _attachedAIBehaviour);

        if (_attachedAIBehaviour is null)
            throw new NullReferenceException("No <color=#16a085>AI behaviour (ITreeRunner)</color> found on " + root.name);

    }

    private void Update()
    {
        var npcInFOV =
            _otherNPCsNetworked.FirstOrDefault(npc => IsInFOVAndInRange(npc.transform));
        if (npcInFOV is null ) return;

        _attachedAIBehaviour.AdjustHuntOrFleeTarget((npcInFOV.transform,npcInFOV.GetComponent<IEntity>()));
    }

    private void OnDrawGizmos()
    {
        bool hasSpereCollider = TryGetComponent(out SphereCollider sphereCollider);

        if (!hasSpereCollider) return;

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, sphereCollider.radius);
    }
    [Rpc(RpcSources.All, RpcTargets.All)]
    private void DealWithHashsetRpc(NetworkTransform entity, bool shouldBeRemoved = false)
    {
        bool isEntity = entity.gameObject.TryGetComponent(out IEntity entityObject);

        if (!isEntity) return;
        var healthManager = entity.gameObject.GetComponent<IHealthManager>();
        
        Action onDeathRemoval = () => RemoveFromSetOnDeathRpc(entity);
        
        if (shouldBeRemoved)
        {
            _otherNPCsNetworked.Remove(entity);
            
            
            if(healthManager is null) return;
            
            healthManager.OnDeath -= onDeathRemoval;
            return;
        }

        _otherNPCsNetworked.Add(entity);
        
        if(healthManager is null) return;
        healthManager.OnDeath += onDeathRemoval;
    }

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
        _otherNPCsNetworked.Remove(entity);
    }

    public void Init(string fishDataName)
    {
        fishData = Resources.Load<FishData>($"FishData/{fishDataName}");
    }
}
