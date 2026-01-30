using UnityEngine;

public class IsNotDoneCondition : IBTNode
{
    private Blackboard m_Blackboard;
    private string m_KeyName;

    public IsNotDoneCondition(Blackboard blackboard, string keyName)
    {
        m_Blackboard = blackboard;
        m_KeyName = keyName;
    }
        
    public NodeState Evaluate()
    {
        return m_Blackboard.Get<bool>(m_KeyName) ? NodeState.Failure : NodeState.Success;
    }

}
