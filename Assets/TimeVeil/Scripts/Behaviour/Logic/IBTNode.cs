/// <summary>
/// Represents the possible states a behavior tree node can return.
/// </summary>
public enum NodeState
{
    /// <summary>
    /// The node is still executing.
    /// </summary>
    Running,

    /// <summary>
    /// The node has completed successfully.
    /// </summary>
    Success,

    /// <summary>
    /// The node has failed.
    /// </summary>
    Failure
}

/// <summary>
/// Interface for all behavior tree nodes.
/// Each node must implement the Evaluate method to return its current state.
/// </summary>
public interface IBTNode
{
    /// <summary>
    /// Evaluates the node and returns its current state.
    /// </summary>
    /// <returns>The current <see cref="NodeState"/> of the node.</returns>
    NodeState Evaluate();
}