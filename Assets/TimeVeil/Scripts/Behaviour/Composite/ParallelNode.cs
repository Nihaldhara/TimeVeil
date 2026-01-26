using System.Collections.Generic;

/// <summary>
/// A composite behavior tree node that evaluates all its child nodes in parallel.
/// Returns <c>Failure</c> immediately if any child returns <c>Failure</c>.
/// Returns <c>Running</c> if at least one child is still running.
/// Returns <c>Success</c> only if all children return <c>Success</c>.
/// </summary>
public class ParallelNode : IBTNode
{
    /// <summary>
    /// The list of child nodes to be evaluated simultaneously.
    /// </summary>
    private List<IBTNode> m_Children;

    /// <summary>
    /// Initializes a new instance of the <see cref="ParallelNode"/> class with the given child nodes.
    /// </summary>
    /// <param name="children">The child nodes to evaluate in parallel.</param>
    public ParallelNode(params IBTNode[] children)
    {
        m_Children = new List<IBTNode>(children);
    }

    /// <summary>
    /// Evaluates all child nodes.
    /// - Returns <c>Failure</c> if any child fails.
    /// - Returns <c>Running</c> if at least one child is still running.
    /// - Returns <c>Success</c> only if all children succeed.
    /// </summary>
    /// <returns>The resulting <see cref="NodeState"/> of the parallel evaluation.</returns>
    public NodeState Evaluate()
    {
        bool anyRunning = false;

        foreach (var child in m_Children)
        {
            NodeState result = child.Evaluate();

            if (result == NodeState.Failure)
                return NodeState.Failure;

            if (result == NodeState.Running)
                anyRunning = true;
        }

        return anyRunning ? NodeState.Running : NodeState.Success;
    }
}