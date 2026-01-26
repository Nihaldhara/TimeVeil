using System.Collections.Generic;

/// <summary>
/// A composite behavior tree node that evaluates its child nodes in order.
/// It returns <c>Success</c> or <c>Running</c> as soon as one child returns a non-failure state.
/// If all children return <c>Failure</c>, the selector returns <c>Failure</c>.
/// </summary>
public class SelectorNode : IBTNode
{
    /// <summary>
    /// The list of child nodes to be evaluated.
    /// </summary>
    private readonly List<IBTNode> m_Children;

    /// <summary>
    /// Initializes a new instance of the <see cref="SelectorNode"/> class with the given child nodes.
    /// </summary>
    /// <param name="nodes">The child nodes to evaluate in order.</param>
    public SelectorNode(params IBTNode[] nodes)
    {
        m_Children = new List<IBTNode>(nodes);
    }

    /// <summary>
    /// Evaluates each child node in order.
    /// Returns <c>Success</c> or <c>Running</c> as soon as one child returns a non-failure state.
    /// Returns <c>Failure</c> only if all children fail.
    /// </summary>
    /// <returns>The resulting <see cref="NodeState"/> of the selector.</returns>
    public NodeState Evaluate()
    {
        foreach (var child in m_Children)
        {
            var result = child.Evaluate();
            if (result != NodeState.Failure)
                return result; // Success or Running
        }
        return NodeState.Failure;
    }
}