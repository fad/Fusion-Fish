using AI.BehaviourTree;
using UnityEngine;

[CreateAssetMenu(fileName = "New Leaf", menuName = "AI/BehaviourTree/New Leaf")]
public class LeafSO : NodeSO
{
    [SerializeField,
     Tooltip("The logic to use for this leaf node")]
    protected StrategySO strategyToUse;

    protected override void OnEnable()
    {
        NodeToRepresent = new Leaf(nodeName, strategyToUse.Strategy, priority);
    }
}
