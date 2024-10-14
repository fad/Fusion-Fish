namespace AI.BehaviourTree
{
    /// <summary>
    /// Represents a selector node in a behavior tree. A selector node evaluates its children in order.
    /// If any child succeeds, the selector succeeds. If all children fail, the selector fails.
    /// </summary>
    public class Selector : Node
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Selector"/> class with the specified name and priority.
        /// </summary>
        /// <param name="name">The name of the selector node.</param>
        /// <param name="priority">The priority of the selector node, default is 0.</param>
        public Selector(string name, int priority = 0) : base(name, priority)
        {
        }

        /// <summary>
        /// Evaluates the selector node by evaluating its children in order.
        /// </summary>
        /// <returns>
        /// Returns <see cref="Status.Running"/> if a child is still running,
        /// <see cref="Status.Success"/> if a child succeeds, or <see cref="Status.Failure"/> if all children fail.
        /// </returns>
        public override Status Evaluate()
        {
            if (CurrentChild < Children.Count)
            {
                switch (Children[CurrentChild].Evaluate())
                {
                    case Status.Running:
                        return Status.Running;
                    case Status.Success:
                        Reset();
                        return Status.Success;
                    default:
                        CurrentChild++;
                        return Status.Running;
                }
            }

            Reset();
            return Status.Failure;
        }
    }
}
