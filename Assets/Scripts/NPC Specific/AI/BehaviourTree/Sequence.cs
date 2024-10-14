namespace AI.BehaviourTree
{
    /// <summary>
    /// Represents a sequence node in a behavior tree. A sequence node evaluates its children in order.
    /// If any child fails, the sequence fails. If all children succeed, the sequence succeeds.
    /// </summary>
    public class Sequence : Node
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Sequence"/> class with the specified name and priority.
        /// </summary>
        /// <param name="name">The name of the sequence node.</param>
        /// <param name="priority">The priority of the sequence node, default is 0.</param>
        public Sequence(string name, int priority = 0) : base(name, priority)
        {
        }

        /// <summary>
        /// Evaluates the sequence node by evaluating its children in order.
        /// </summary>
        /// <returns>
        /// Returns <see cref="Status.Running"/> if a child is still running, 
        /// <see cref="Status.Failure"/> if a child fails, or <see cref="Status.Success"/> if all children succeed.
        /// </returns>
        public override Status Evaluate()
        {
            if (CurrentChild < Children.Count)
            {
                switch (Children[CurrentChild].Evaluate())
                {
                    case Status.Running:
                        return Status.Running;
                    case Status.Failure:
                        CurrentChild = 0;
                        return Status.Failure;
                    default:
                        CurrentChild++;
                        return CurrentChild == Children.Count ? Status.Success : Status.Running;
                }
            }

            Reset();
            return Status.Success;
        }
    }
}
