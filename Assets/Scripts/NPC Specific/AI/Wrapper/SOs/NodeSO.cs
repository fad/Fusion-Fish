using UnityEngine;
using AI.BehaviourTree;

public abstract class NodeSO : ScriptableObject
{
    [Header("Node Properties")]
    [SerializeField,
     Tooltip("The name of the node to use internally in the tree.")]
    protected string nodeName;

    [SerializeField,
     Tooltip("The priority of this node inside of a priority selector")]
    protected int priority;

    [SerializeField,
     Tooltip("The children of this node. It needs to be in the same order as they should be evaluated.")]
    protected NodeSO[] children;

    /// <summary>
    /// The runner for the overall behavior tree. This class is responsible for executing the behavior tree
    /// and injecting itself into the nodes so that data can be accessed within the nodes.
    /// </summary>
    protected ITreeRunner Runner;

    /// <summary>
    /// The node that this ScriptableObject represents in the behavior tree.
    /// </summary>
    protected Node NodeToRepresent;

    protected abstract void OnEnable();

    /// <summary>
    /// Recursively adds all children nodes to their respective parent nodes in the behavior tree.
    /// </summary>
    protected void AddChildren()
    {
        if (children == null || children.Length == 0) return;

        foreach (NodeSO child in children)
        {
            child.SetRunner(Runner);
            child.AddChildren();
            NodeToRepresent.AddChild(child.NodeToRepresent);
        }
    }
    
    /// <summary>
    /// Sets the runner for the overall behavior tree. This method is used to inject the 
    /// <see cref="Runner"/> instance into the node so that data can be accessed within the nodes.
    /// </summary>
    /// <param name="runner">The <see cref="Runner"/> instance to be set.</param>
    public void SetRunner(ITreeRunner runner)
    {
        Runner = runner;
    }
}
