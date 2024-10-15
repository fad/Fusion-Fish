using AI.BehaviourTree;
using UnityEngine;

[CreateAssetMenu(fileName = "New PrioritySelector", menuName = "AI/BehaviourTree/New PrioritySelector")]
public class PrioritySelectorSO : NodeSO
{
    protected override void OnEnable()
    {
        NodeToRepresent = new PrioritySelector(nodeName);
    }
}
