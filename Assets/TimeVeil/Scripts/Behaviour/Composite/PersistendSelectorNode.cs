using System.Collections.Generic;

public class PersistentSelectorNode : IBTNode
{
    private readonly List<IBTNode> m_Children;
    private int m_CurrentIndex = 0;

    public PersistentSelectorNode(params IBTNode[] nodes)
    {
        m_Children = new List<IBTNode>(nodes);
    }

    public NodeState Evaluate()
    {
        var currentChild = m_Children[m_CurrentIndex];
        var result = currentChild.Evaluate();

        if (result == NodeState.Running)
            return NodeState.Running;

        if (result == NodeState.Success)
        {
            m_CurrentIndex = 0; 
            return NodeState.Success;
        }

        m_CurrentIndex++;
        while (m_CurrentIndex < m_Children.Count)
        {
            result = m_Children[m_CurrentIndex].Evaluate();
            if (result != NodeState.Failure)
            {
                if (result == NodeState.Running)
                    return NodeState.Running;
                else
                {
                    m_CurrentIndex = 0;
                    return NodeState.Success;
                }
            }
            m_CurrentIndex++;
        }

        m_CurrentIndex = 0;
        return NodeState.Failure;
    }

}