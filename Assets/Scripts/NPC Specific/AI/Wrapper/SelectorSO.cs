using AI.BehaviourTree;
using UnityEngine;

[CreateAssetMenu(fileName = "New Selector", menuName = "AI/BehaviourTree/New Selector")]
public class SelectorSO : NodeSO
{
    protected override void OnEnable()
    {
        NodeToRepresent = new Selector(nodeName, priority);
    }
}