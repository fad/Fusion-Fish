using System;
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
    
    public string NodeName => nodeName;
    public int Priority => priority;
    public NodeSO[] Children => children;
    
    /// <summary>
    /// The node that this ScriptableObject represents in the behavior tree.
    /// </summary>
    protected Node NodeToRepresent;

    protected abstract void OnEnable();

    /// <summary>
    /// Recursively adds all children nodes to their respective parent nodes in the behavior tree.
    /// </summary>
    public void AddChildren()
    {
        if (children == null || children.Length == 0) return;

        foreach (NodeSO child in children)
        {
            child.AddChildren();
            NodeToRepresent.AddChild(child.NodeToRepresent);
        }
    }
}