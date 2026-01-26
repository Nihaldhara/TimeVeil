/// <summary>
/// A decorator node that waits for a condition to be true before executing an action.
/// Once the condition is met, the action is allowed to complete even if the condition becomes false.
/// The running state is tracked using a key in the blackboard.
/// </summary>
public class WaitUntilConditionCompleteDecorator : IBTNode
{
    private IBTNode m_ConditionNode;
    private IBTNode m_ActionNode;
    private Blackboard m_Blackboard;
    private string m_StateKey;

    /// <summary>
    /// Initializes a new instance of the <see cref="WaitUntilConditionCompleteDecorator"/> class.
    /// </summary>
    /// <param name="blackboard">The shared blackboard used to store state.</param>
    /// <param name="conditionNode">The condition node to evaluate before starting the action.</param>
    /// <param name="actionNode">The action node to execute once the condition is met.</param>
    /// <param name="stateKey">The key used in the blackboard to track whether the action is running.</param>
    public WaitUntilConditionCompleteDecorator(Blackboard blackboard, IBTNode conditionNode, IBTNode actionNode, string stateKey = "isRunning")
    {
        m_ConditionNode = conditionNode;
        m_ActionNode = actionNode;
        m_Blackboard = blackboard;
        m_StateKey = stateKey;

        if (!m_Blackboard.Contains(m_StateKey))
            m_Blackboard.Set(m_StateKey, false);
    }

    /// <summary>
    /// Evaluates the decorator node.
    /// - If the action is not running and the condition is not met, returns <c>Failure</c>.
    /// - If the condition is met, starts the action and sets the running flag.
    /// - Allows the action to complete regardless of the condition's state once started.
    /// - Resets the running flag when the action finishes.
    /// </summary>
    /// <returns>The resulting <see cref="NodeState"/> of the evaluation.</returns>
    public NodeState Evaluate()
    {
        bool isRunning = m_Blackboard.Get<bool>(m_StateKey);

        if (!isRunning)
        {
            if (m_ConditionNode.Evaluate() != NodeState.Success)
                return NodeState.Failure;

            m_Blackboard.Set(m_StateKey, true);
        }

        NodeState result = m_ActionNode.Evaluate();

        if (result == NodeState.Success || result == NodeState.Failure)
        {
            m_Blackboard.Set(m_StateKey, false);
        }

        return result;
    }
}
