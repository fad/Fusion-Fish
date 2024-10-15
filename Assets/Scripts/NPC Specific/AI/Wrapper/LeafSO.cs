using UnityEngine;

[CreateAssetMenu(menuName = "New Leaf", fileName = "AI/BehaviourTree/New Leaf")]
public class LeafSO : NodeSO
{
    protected override void OnEnable()
    {
        //NodeToRepresent = new Leaf(nodeName);
    }
}
