using AI.BehaviourTree;
using UnityEngine;

[CreateAssetMenu(fileName = "New Behaviour Tree", menuName = "AI/BehaviourTree/New Behaviour Tree")]
public class BehaviourTreeSO : NodeSO
{
    protected override void OnEnable()
    {
        NodeToRepresent = new BehaviourTree(nodeName);
    }

    public void Execute()
    {
        NodeToRepresent.Evaluate();
    }

}