using System;
using UnityEngine;

public class IsPlayerSeen : IBTNode
{
    private Blackboard m_Blackboard;

    private GameManager m_GameManager;

    private float m_SightDistance = 1.0f;

    private bool playerSeen = false;
    
    public IsPlayerSeen()
    {
        m_GameManager = GameManager.Instance;
        m_GameManager.PlayerEnteredTriggerEvent.AddListener(PlayerSeenListener);
    }

    private void PlayerSeenListener()
    {
        playerSeen = true;
    }
    
    public NodeState Evaluate()
    {
        if (playerSeen)
        {
            return NodeState.Success;
        }

        return NodeState.Failure;
    }
}