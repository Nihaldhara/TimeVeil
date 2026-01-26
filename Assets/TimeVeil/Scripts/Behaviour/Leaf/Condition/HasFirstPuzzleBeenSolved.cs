public class HasFirstPuzzleBeenSolved : IBTNode
{
    private bool m_CheckFirstPuzzleState;
    
    public HasFirstPuzzleBeenSolved(bool checkFirstPuzzleState)
    {
        m_CheckFirstPuzzleState = checkFirstPuzzleState;
    }
    
    public NodeState Evaluate()
    {
        var puzzleManager = PuzzleManager.Instance;
        
        if (puzzleManager == null)
            return NodeState.Failure;
            
        return puzzleManager.checkFirstPuzzleState == m_CheckFirstPuzzleState 
            ? NodeState.Success 
            : NodeState.Failure;
    }
}