using System;
using AI.BehaviourTree;
using UnityEngine;

public class TreeRunnerMB : MonoBehaviour, ITreeRunner
{
    [Header("Settings")]
    [SerializeField,
     Tooltip("The data for this fish")]
    private FishData fishData;
    
    [SerializeField,
     Tooltip("The obstacle avoidance layer mask")]
    private LayerMask obstacleAvoidanceMask;

    /// <summary>
    /// The behaviour tree to execute on this object.
    /// </summary>
    private BehaviourTree _behaviourTreeToExecute;
    
    /// <summary>
    /// Whether the fish is inside a forbidden area.
    /// </summary>
    private bool _isInsideArea;
    
    /// <summary>
    /// The direction towards center of the area the fish is inside.
    /// </summary>
    private Vector3 _directionToArea;
    
    /// <summary>
    /// Whether the fish is in danger
    /// </summary>
    private bool _isSafe;
    
    /// <summary>
    /// The stamina manager to use for this fish.
    /// </summary>
    private IStaminaManager _staminaManager;
    
    public FishData FishType => fishData;

    private void Awake()
    {
        if(!fishData) throw new NullReferenceException("FishData is not set in " + gameObject.name);
        
        _staminaManager = GetComponent<StaminaManager>();
        
        if(_staminaManager is null) throw new NullReferenceException("StaminaManager is not found in " + gameObject.name);
    }

    private void Start()
    {
        _behaviourTreeToExecute = new BehaviourTree(gameObject.name);

        PrioritySelector actions = new PrioritySelector("Root");
        
        Sequence fleeSequence = new("Flee", 100);
        Leaf isInDanger = new Leaf("Is in danger?", new Condition(() => !_isSafe));
        Selector fastOrNormal = new Selector("Fast or Normal Flee");
        Sequence fastFleeSequence = new("Fast Flee");
        Leaf staminaOverThreshold = new ("Stamina over threshold?",
            new Condition(() => _staminaManager.CurrentStamina > fishData.StaminaThreshold));
        

        fastFleeSequence.AddChild(staminaOverThreshold);
        
        fastOrNormal.AddChild(fastFleeSequence);
        
        fleeSequence.AddChild(isInDanger);
        fleeSequence.AddChild(fastOrNormal);
        
        Leaf wanderAround = new Leaf("Wander Around",
            new WanderStrategy.Builder(transform)
                .WithSpeed(fishData.WanderSpeed)
                .WithRotationSpeed(fishData.RotationSpeed)
                .WithMaxPitch(fishData.MaxPitch)
                .WithObstacleAvoidanceLayerMask(obstacleAvoidanceMask)
                .WithObstacleAvoidanceDistance(fishData.ObstacleAvoidanceDistance)
                .WithForbiddenAreaCheck(IsInsideForbiddenArea)
                .Build());

        actions.AddChild(wanderAround);

        // Sequence huntSequence = new("Hunt");
        

        _behaviourTreeToExecute.AddChild(actions);
    }

    private void Update()
    {
        _behaviourTreeToExecute?.Evaluate();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(120f/255f, 33f/255f, 114f/255f, 1f);
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * 2f);
    }

    private (bool isInside, Vector3 direction) IsInsideForbiddenArea()
    {
        return (isInside: _isInsideArea, direction: _directionToArea);
    }

    public void AdjustAreaCheck((bool isInside, Vector3 direction) areaCheck)
    {
        _isInsideArea = areaCheck.isInside;
        _directionToArea = areaCheck.direction;
    }
}
