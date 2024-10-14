using System;

namespace AI.BehaviourTree
{
    /// <summary>
    /// Represents a condition strategy in a behavior tree. A condition strategy evaluates a predicate function and returns a status based on the result.
    /// </summary>
    public class Condition : IStrategy
    {
        /// <summary>
        /// The predicate function to be evaluated by the strategy.
        /// </summary>
        private readonly Func<bool> _predicate;

        /// <summary>
        /// Initializes a new instance of the <see cref="Condition"/> class with the specified predicate function.
        /// </summary>
        /// <param name="predicate">The predicate function to be evaluated by the strategy.</param>
        public Condition(Func<bool> predicate)
        {
            _predicate = predicate;
        }

        /// <summary>
        /// Processes the condition strategy by evaluating the predicate function.
        /// </summary>
        /// <returns>
        /// Returns <see cref="Status.Success"/> if the predicate evaluates to true, otherwise returns <see cref="Status.Failure"/>.
        /// </returns>
        public Status Process() => _predicate() ? Status.Success : Status.Failure;
    }
}
