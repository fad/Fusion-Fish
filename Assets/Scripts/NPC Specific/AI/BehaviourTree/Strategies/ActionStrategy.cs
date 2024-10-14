using System;

namespace AI.BehaviourTree
{
    /// <summary>
    /// Represents an action strategy in a behavior tree. An action strategy executes a specified action and returns a success status.
    /// </summary>
    public class ActionStrategy : IStrategy
    {
        /// <summary>
        /// The action to be executed by the strategy.
        /// </summary>
        private readonly Action _actionToDo;

        /// <summary>
        /// Initializes a new instance of the <see cref="ActionStrategy"/> class with the specified action.
        /// </summary>
        /// <param name="actionToDo">The action to be executed by the strategy.</param>
        public ActionStrategy(Action actionToDo)
        {
            _actionToDo = actionToDo;
        }

        /// <summary>
        /// Processes the action strategy by executing the specified action.
        /// </summary>
        /// <returns>
        /// Returns <see cref="Status.Success"/> after the action is executed.
        /// </returns>
        public Status Process()
        {
            _actionToDo();
            return Status.Success;
        }
    }
}
