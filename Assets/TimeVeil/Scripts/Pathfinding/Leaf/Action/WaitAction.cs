using UnityEngine;

public class WaitAction : IBTNode
{
    private float m_StartTime;
    private float m_CurrentTime;
    
    private float m_DelayMax;
    private float m_DelayMin;
    
    public WaitAction(float DelayMax, float DelayMin)
    {
        m_DelayMax = DelayMax;
        m_DelayMin = DelayMin;
        m_StartTime = Random.Range(m_DelayMin, m_DelayMax);
        m_CurrentTime = m_StartTime;
    }
    
    public NodeState Evaluate()
    {
        m_CurrentTime -= Time.deltaTime;

        if (m_CurrentTime <= 0.0f)
        {
            m_StartTime = Random.Range(m_DelayMin, m_DelayMax);
            m_CurrentTime = m_StartTime;
            return NodeState.Success;
        }

        return NodeState.Failure;
    }
}
