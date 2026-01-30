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
        m_Index = 0;
    }

    public NodeState Evaluate()
    {
        m_Index++;
        
        if (m_Index >= m_TargetsList.Count)
        {
            Debug.Log($"Select Target : Reset Current Index {m_Index} and targetlist count {m_TargetsList.Count}");
            m_Index = 0;
            m_Blackboard.Set("CurrentTarget", m_TargetsList[m_Index]);
            return NodeState.Success;
        }
        
        m_Blackboard.Set("CurrentTarget", m_TargetsList[m_Index]);
        
        Debug.Log($"Select Target : Valid Current Index {m_Index} and targetlist count {m_TargetsList.Count}");
        


        

        return NodeState.Success;
    }
}
