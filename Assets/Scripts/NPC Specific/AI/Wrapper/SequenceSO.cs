using AI.BehaviourTree;
using UnityEngine;

[CreateAssetMenu(fileName = "New Sequence", menuName = "AI/BehaviourTree/New Sequence")]
public class SequenceSO : NodeSO
{
    protected override void OnEnable()
    {
        NodeToRepresent = new Sequence(nodeName, priority);
    }
}