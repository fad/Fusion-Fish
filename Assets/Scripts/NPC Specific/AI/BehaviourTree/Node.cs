using System.Collections.Generic;

namespace AI.BehaviourTree
{
    /// <summary>
    /// Represents a node in a behavior tree.
    /// </summary>
    public class Node
    {
        /// <summary>
        /// The name of the node.
        /// </summary>
        public readonly string Name;
        
        /// <summary>
        /// The priority of the node in case that a PrioritySelector is used.
        /// </summary>
        public readonly int Priority;

        /// <summary>
        /// The current child that is being evaluated.
        /// </summary>
        protected int CurrentChild;
        
        /// <summary>
        /// Every child of the node.
        /// </summary>
        public readonly List<Node> Children = new();

        /// <summary>
        /// Base constructor for a node.
        /// </summary>
        /// <param name="name">The name of the node, default is "Node".</param>
        /// <param name="priority">The priority of the node, default is 0.</param>
        public Node(string name = "Node", int priority = 0)
        {
            Name = name;
            Priority = priority;
        }
        
        
        /// <summary>
        /// Adds a child to the node.
        /// </summary>
        /// <param name="child">Node to add as a child</param>
        public virtual void AddChild(Node child)
        {
            Children.Add(child);
        }

        /// <summary>
        /// Evaluates the current node by calling the Evaluate method of the current child node.
        /// </summary>
        /// <returns>
        /// The status of the evaluation, which is determined by the child node's Evaluate method.
        /// </returns>
        public virtual Status Evaluate() => Children[CurrentChild].Evaluate();
        
        /// <summary>
        /// Resets the current node and all its child nodes.
        /// </summary>
        public virtual void Reset()
        {
            CurrentChild = 0;
            foreach(Node child in Children)
            {
                child.Reset();
            }
        }

        /// <summary>
        /// Returns the current child of the node that is executed.
        /// </summary>
        /// <returns>The name of the node</returns>
        public virtual string ShowCurrentChild()
        {
            if(Children.Count == 0) return Name;
            
            return Children[CurrentChild].ShowCurrentChild();
        }
    }
}
