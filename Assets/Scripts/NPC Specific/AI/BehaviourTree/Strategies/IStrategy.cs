namespace AI.BehaviourTree
{
    /// <summary>
    /// Defines the interface for a strategy in a behavior tree. A strategy is a component that can process and reset its state.
    /// </summary>
    public interface IStrategy
    {
        /// <summary>
        /// Processes the strategy and returns the status of the process.
        /// </summary>
        /// <returns>
        /// Returns <see cref="Status.Success"/> if the process is successful, otherwise returns <see cref="Status.Failure"/>.
        /// </returns>
        Status Process();

        /// <summary>
        /// Resets the state of the strategy.
        /// </summary>
        void Reset()
        {
        }
    }
}
