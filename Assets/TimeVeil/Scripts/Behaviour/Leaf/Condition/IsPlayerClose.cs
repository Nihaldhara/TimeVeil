using System;
using UnityEngine;

public class IsPlayerClose : IBTNode
{
    private Blackboard m_Blackboard;

    private Transform m_PlayerTransform;
    private Transform m_CrowdTransform;

    private float m_SightDistance = 1.0f;
    
    public IsPlayerClose(Blackboard blackboard)
    {
        m_Blackboard = blackboard;
        m_PlayerTransform = m_Blackboard.Get<Transform>("PlayerTransform");
        m_CrowdTransform = m_Blackboard.Get<Transform>("CrowdTransform");
    }
    
    public NodeState Evaluate()
    {
        if (Vector3.Distance(m_PlayerTransform.position, m_CrowdTransform.position) <= m_SightDistance)
            return NodeState.Success;

        return NodeState.Failure;
    }
}