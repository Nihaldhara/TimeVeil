using UnityEngine;

public class IsNotReachedCondition : IBTNode
{
    private Blackboard m_Blackboard;

    private Transform m_AgentTransform;
    private Transform m_TargetTransform;

    public IsNotReachedCondition(Blackboard blackboard)
    {
        m_Blackboard = blackboard;

        m_AgentTransform = m_Blackboard.Get<Transform>("CrowdTransform");
        m_TargetTransform = m_Blackboard.Get<Transform>("CurrentTarget");
    }
        
    public NodeState Evaluate()
    {
        m_TargetTransform = m_Blackboard.Get<Transform>("CurrentTarget");
        
        if (Vector3.Distance(m_AgentTransform.position, m_TargetTransform.position) < 0.5f)
        {
            Debug.Log("IsNotReachedCondition: Target reached, moving on to next target");
            return NodeState.Success;
        }

        return NodeState.Failure;
    }
}
