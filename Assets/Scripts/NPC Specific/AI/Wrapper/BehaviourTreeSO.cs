using AI.BehaviourTree;
using UnityEngine;

[CreateAssetMenu(fileName = "New Behaviour Tree", menuName = "AI/BehaviourTree/New Behaviour Tree", order = 0)]
public class BehaviourTreeSO : NodeSO
{
    protected override void OnEnable()
    {
        NodeToRepresent = new BehaviourTree(nodeName);
    }

    public BehaviourTree Construct()
    {
        AddChildren();
        return NodeToRepresent as BehaviourTree;
    }

}