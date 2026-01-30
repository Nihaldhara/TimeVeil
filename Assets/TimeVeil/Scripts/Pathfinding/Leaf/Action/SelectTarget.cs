using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class SelectTarget : IBTNode
{
    private Blackboard m_Blackboard;

    private List<Transform> m_TargetsList;
    private int m_Index;
    
    public SelectTarget(Blackboard blackboard)
    {
        m_Blackboard = blackboard;
        m_TargetsList = m_Blackboard.Get<List<Transform>>("TargetsList");
        m_Index = 1;
    }

    public NodeState Evaluate()
    {
        if (m_Index < m_TargetsList.Count - 1)
        {
            m_Blackboard.Set("CurrentTarget", m_TargetsList[m_Index]);
            m_Index = 0;
            return NodeState.Success;
        }

        m_Blackboard.Set("CurrentTarget", m_TargetsList[m_Index]);
        m_Index++;
        return NodeState.Success;
    }
}
