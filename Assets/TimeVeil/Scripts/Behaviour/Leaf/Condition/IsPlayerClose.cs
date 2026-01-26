using UnityEngine;

public class IsPlayerClose : IBTNode
{
    private Blackboard m_Blackboard;
    
    public IsPlayerClose(Blackboard blackboard)
    {
        m_Blackboard = blackboard;
    }
    
    public NodeState Evaluate()
    {
        throw new System.NotImplementedException();
    }
}