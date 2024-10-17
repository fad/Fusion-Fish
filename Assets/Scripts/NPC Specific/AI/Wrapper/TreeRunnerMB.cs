using AI.BehaviourTree;
using UnityEngine;

public class TreeRunnerMB : MonoBehaviour, ITreeRunner
{
    [SerializeField,
     Tooltip("The data for this fish")]
    private FishData fishData;

    /// <summary>
    /// The behaviour tree to execute on this object.
    /// </summary>
    private BehaviourTree _behaviourTreeToExecute;

    /// <summary>
    /// Flag for the fish to know if it is in danger.
    /// </summary>
    private bool _isInDanger;

    private void Start()
    {
        _behaviourTreeToExecute = new BehaviourTree(gameObject.name);

        PrioritySelector actions = new PrioritySelector("Root");

        Leaf wanderAround = new Leaf("Wander Around",
            new WanderStrategy(
                transform,
                3.5f,
                2f,
                45f
            ));

        actions.AddChild(wanderAround);

        // Sequence huntSequence = new("Hunt");
        //
        // Sequence fleeSequence = new("Flee", 100);
        // Leaf isSafe = new Leaf("Is Safe?", new Condition(IsSafe));

        _behaviourTreeToExecute.AddChild(actions);
    }

    private void Update()
    {
        _behaviourTreeToExecute?.Evaluate();
    }

    private bool IsSafe()
    {
        return !_isInDanger;
    }
}
