using System.Collections.Generic;
using System.Linq;

namespace AI.BehaviourTree
{
    /// <summary>
    /// Represents a priority selector node in a behavior tree. A priority selector node evaluates its children in order of their priority.
    /// If any child succeeds, the selector succeeds. If all children fail, the selector fails.
    /// </summary>
    public class PrioritySelector : Selector
    {
        /// <summary>
        /// A cached list of sorted children based on their priority.
        /// </summary>
        private List<Node> _sortedChildren;

        /// <summary>
        /// Gets the sorted children based on their priority.
        /// </summary>
        private List<Node> SortedChildren => _sortedChildren ??= SortChildren();

        /// <summary>
        /// Sorts the children nodes based on their priority in descending order.
        /// </summary>
        /// <returns>A list of sorted children nodes.</returns>
        protected virtual List<Node> SortChildren() => Children.OrderByDescending(child => child.Priority).ToList();

        /// <summary>
        /// Initializes a new instance of the <see cref="PrioritySelector"/> class with the specified name and priority.
        /// </summary>
        /// <param name="name">The name of the priority selector node.</param>
        /// <param name="priority">The priority of the priority selector node, default is 0.</param>
        public PrioritySelector(string name, int priority = 0) : base(name, priority)
        {
        }

        /// <summary>
        /// Resets the priority selector node and clears the cached sorted children.
        /// </summary>
        public override void Reset()
        {
            base.Reset();
            _sortedChildren = null;
        }

        /// <summary>
        /// Evaluates the priority selector node by evaluating its sorted children in order.
        /// </summary>
        /// <returns>
        /// Returns <see cref="Status.Running"/> if a child is still running,
        /// <see cref="Status.Success"/> if a child succeeds, or <see cref="Status.Failure"/> if all children fail.
        /// </returns>
        public override Status Evaluate()
        {
            foreach (Node child in SortedChildren)
            {
                switch (child.Evaluate())
                {
                    case Status.Running:
                        return Status.Running;
                    case Status.Success:
                        Reset();
                        return Status.Success;
                    default:
                        continue;
                }
            }

            Reset();
            return Status.Failure;
        }
    }
}
