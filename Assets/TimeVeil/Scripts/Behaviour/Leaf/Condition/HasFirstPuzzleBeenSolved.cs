using UnityEngine;

public class HasFirstPuzzleBeenSolved : IBTNode
{
    private GameManager m_GameManager;
    
    public HasFirstPuzzleBeenSolved()
    {
        m_GameManager = GameManager.Instance;
    }
    
    public NodeState Evaluate()
    {
        if (m_GameManager.checkFirstPuzzleState)
        {
            return NodeState.Success;
        }

        return NodeState.Failure;
    }
}