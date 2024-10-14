namespace AI.BehaviourTree
{
    /// <summary>
    /// Represents the status of a node or strategy in a behavior tree.
    /// </summary>
    public enum Status
    {
        /// <summary>
        /// Indicates that the process was successful.
        /// </summary>
        Success,

        /// <summary>
        /// Indicates that the process failed.
        /// </summary>
        Failure,

        /// <summary>
        /// Indicates that the process is still running.
        /// </summary>
        Running
    }
}
