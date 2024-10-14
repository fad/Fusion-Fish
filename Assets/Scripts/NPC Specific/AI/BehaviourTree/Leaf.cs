namespace AI.BehaviourTree
{
    public class Leaf : Node
    {
        /// <summary>
        /// The strategy to be executed by the leaf node.
        /// </summary>
        private readonly IStrategy _strategy;

        /// <summary>
        /// Initializes a new instance of the <see cref="Leaf"/> class with the specified name, strategy, and priority.
        /// </summary>
        /// <param name="name">The name of the leaf node.</param>
        /// <param name="strategy">The strategy to be executed by the leaf node.</param>
        /// <param name="priority">The priority of the leaf node, default is 0.</param>
        public Leaf(string name, IStrategy strategy, int priority = 0) : base(name, priority)
        {
            _strategy = strategy;
        }

        /// <summary>
        /// Evaluates the leaf node by executing its strategy.
        /// </summary>
        /// <returns>The status of the strategy execution.</returns>
        public override Status Evaluate() => _strategy.Process();

        /// <summary>
        /// Resets the leaf node by resetting its strategy.
        /// </summary>
        public override void Reset() => _strategy.Reset();
    }
}
