using AI.BehaviourTree;
using UnityEngine;

[CreateAssetMenu(fileName = "New Inverter", menuName = "AI/BehaviourTree/New Inverter")]
public class InverterSO : NodeSO
{
    protected override void OnEnable()
    {
        NodeToRepresent = new Inverter(nodeName);
    }
}