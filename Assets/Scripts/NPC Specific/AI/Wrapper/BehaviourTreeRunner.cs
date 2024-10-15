using Fusion;
using UnityEngine;

public class BehaviourTreeRunner : NetworkBehaviour
{
    [SerializeField]
    private BehaviourTreeSO behaviourTreeToExecute;

    public override void Spawned()
    {
        behaviourTreeToExecute.SetRunner(this);
        behaviourTreeToExecute.AddChildren();
    }

    private void Update()
    {
        behaviourTreeToExecute.Execute();
    }
}
