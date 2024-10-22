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
    
    private bool _isInsideArea;
    private Vector3 _directionToArea;
    private bool _isSafe;
    
    public FishData FishType => fishData;

    private void Awake()
    {
        if(!fishData) throw new NullReferenceException("FishData is not set in " + gameObject.name);
    }

    private void Start()
    {
        _behaviourTreeToExecute = new BehaviourTree(gameObject.name);

        PrioritySelector actions = new PrioritySelector("Root");
        
        Sequence fleeSequence = new("Flee", 100);
        Leaf isInDanger = new Leaf("Is in danger?", new Condition(() => !_isSafe));

        
        fleeSequence.AddChild(isInDanger);
        
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
