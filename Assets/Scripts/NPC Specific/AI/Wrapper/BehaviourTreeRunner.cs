using AI.BehaviourTree;
using Fusion;
using UnityEngine;

public class BehaviourTreeRunner : NetworkBehaviour
{
    [SerializeField,
     Tooltip("The behaviour tree to generate and execute")]
    private BehaviourTreeSO behaviourTreeToGenerate;

    /// <summary>
    /// The generated behaviour tree to execute on this object.
    /// </summary>
    private BehaviourTree _behaviourTreeToExecute;

    public override void Spawned()
    {
        _behaviourTreeToExecute = behaviourTreeToGenerate.Construct();
    }

    private void Update()
    {
        _behaviourTreeToExecute?.Evaluate();
    }
}
