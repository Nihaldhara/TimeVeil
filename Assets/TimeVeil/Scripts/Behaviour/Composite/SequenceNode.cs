using System.Collections.Generic;

/// <summary>
/// A composite behavior tree node that executes its child nodes in sequence.
/// It returns <c>Success</c> only if all children return <c>Success</c>.
/// If any child returns <c>Failure</c> or <c>Running</c>, the sequence stops and returns that state.
/// </summary>
public class SequenceNode : IBTNode
{
    /// <summary>
    /// The list of child nodes to be evaluated in order.
    /// </summary>
    private readonly List<IBTNode> m_Children;

    /// <summary>
    /// Initializes a new instance of the <see cref="SequenceNode"/> class with the given child nodes.
    /// </summary>
    /// <param name="nodes">The child nodes to execute in sequence.</param>
    public SequenceNode(params IBTNode[] nodes)
    {
        m_Children = new List<IBTNode>(nodes);
    }

    /// <summary>
    /// Evaluates each child node in order.
    /// Returns <c>Success</c> only if all children return <c>Success</c>.
    /// Returns <c>Failure</c> or <c>Running</c> immediately if any child does.
    /// </summary>
    /// <returns>The resulting <see cref="NodeState"/> of the sequence.</returns>
    public NodeState Evaluate()
    {
        foreach (var child in m_Children)
        {
            var result = child.Evaluate();
            if (result != NodeState.Success)
                return result; // Failure or Running
        }
        return NodeState.Success;
    }
}