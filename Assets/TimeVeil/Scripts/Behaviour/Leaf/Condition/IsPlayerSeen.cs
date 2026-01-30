using System;
using UnityEngine;

public class IsPlayerSeen : IBTNode
{
    private Blackboard m_Blackboard;

    private Transform m_SmartCrowdTransform;
    
    private Transform m_PlayerTransform;

    private GameManager m_GameManager;

    private float m_SightDistance = 1.0f;

    private bool playerSeen = false;
    
    public IsPlayerSeen(Blackboard blackboard)
    {
        m_Blackboard = blackboard;
        m_SmartCrowdTransform = m_Blackboard.Get<Transform>("CrowdTransform");
        m_PlayerTransform = m_Blackboard.Get<Transform>("PlayerTransform");
        m_GameManager = GameManager.Instance;
        //m_GameManager.PlayerEnteredTriggerEvent.AddListener(PlayerSeenListener);
    }

    // private void PlayerSeenListener()
    // {
    //     playerSeen = true;
    // }
    
    public NodeState Evaluate()
    {
        Debug.Log($"IsPlayerSeen: player position is {m_PlayerTransform.position.ToString()}");
        Debug.Log($"IsPlayerSeen: Crowd position is {m_SmartCrowdTransform.position.ToString()}");
        Debug.Log($"IsPlayerSeen: Distance between them is is {Vector3.Distance(m_SmartCrowdTransform.position, m_PlayerTransform.position)}");
        if (Vector3.Distance(m_SmartCrowdTransform.position, m_PlayerTransform.position) < 1)
        {
            m_Blackboard.Set("CurrentTarget", m_PlayerTransform);
            m_GameManager.PlayerDead = true;
            return NodeState.Success;
        }

        Debug.Log("IsPlayerSeen : failure");
        return NodeState.Failure;
    }
}