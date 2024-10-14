namespace AI.BehaviourTree
{
    /// <summary>
    /// Represents an inverter node in a behavior tree. An inverter node inverts the result of its child node.
    /// </summary>
    public class Inverter : Node
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Inverter"/> class with the specified name.
        /// </summary>
        /// <param name="name">The name of the inverter node.</param>
        public Inverter(string name) : base(name)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Inverter"/> class with the specified name and priority.
        /// </summary>
        /// <param name="name">The name of the inverter node.</param>
        /// <param name="priority">The priority of the inverter node, default is 0.</param>
        public Inverter(string name, int priority = 0) : base(name, priority)
        {
        }

        /// <summary>
        /// Evaluates the inverter node by evaluating its child and inverting the result.
        /// </summary>
        /// <returns>
        /// Returns <see cref="Status.Running"/> if the child is still running,
        /// <see cref="Status.Success"/> if the child fails, or <see cref="Status.Failure"/> if the child succeeds.
        /// </returns>
        public override Status Evaluate()
        {
            switch (Children[0].Evaluate())
            {
                case Status.Running:
                    return Status.Running;
                case Status.Failure:
                    return Status.Success;
                default:
                    return Status.Failure;
            }
        }
    }
}
