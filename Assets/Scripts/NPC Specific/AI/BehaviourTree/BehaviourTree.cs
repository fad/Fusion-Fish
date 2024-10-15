namespace AI.BehaviourTree
{
    /// <summary>
    /// Represents a behavior tree node. A behavior tree is a hierarchical structure of nodes that control the behavior of an AI agent.
    /// </summary>
    public class BehaviourTree : Node
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BehaviourTree"/> class with the specified name.
        /// </summary>
        /// <param name="name">The name of the behavior tree node.</param>
        public BehaviourTree(string name) : base(name)
        {
        }

        /// <summary>
        /// Evaluates the behavior tree node by evaluating its current child node.
        /// </summary>
        /// <returns>
        /// Returns <see cref="Status.Running"/> as the behavior tree is continuously running.
        /// </returns>
        public override Status Evaluate()
        {
            while(CurrentChild < Children.Count)
            {
                Status status = Children[CurrentChild].Evaluate();
                if(status != Status.Success)
                {
                    return status;
                }
                CurrentChild++;
            }

            return Status.Success;
        }
    }
}
