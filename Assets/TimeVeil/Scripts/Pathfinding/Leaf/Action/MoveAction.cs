using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveAction : IBTNode
{
    private Blackboard m_Blackboard;
    private float m_MoveSpeed;
    private Transform m_SmartCrowdTransform;
    private Pathfinding m_PathFinding;
    private Transform m_Target;
    private string m_KeyName;

    private int m_TargetIndex;
    private Vector3[] m_Path;
    private bool m_IsFollowingPath;
    Vector3 currentWaypoint;

    public MoveAction(Blackboard blackboard, float moveSpeed, string keyName = "")
    {
        m_Blackboard = blackboard;
        m_MoveSpeed = moveSpeed;
        m_SmartCrowdTransform = m_Blackboard.Get<Transform>("CrowdTransform");
        m_PathFinding = m_Blackboard.Get<Pathfinding>("PathFinding");
        m_KeyName = keyName;
        m_PathFinding.OnPathUpdatedEvent += OnPathChanged;
    }

    public NodeState Evaluate()
    {
        m_Target = m_Blackboard.Get<Transform>("CurrentTarget");
        
        if (m_PathFinding.IsTargetReachable(m_Target.position) && m_PathFinding.Target != m_Target)
        {
            m_PathFinding.Target = m_Target;
            m_PathFinding.AgentNeedRepath = true;
        }
    
        if (!m_PathFinding.IsTargetReachable(m_Target.position))
        {
            Debug.Log("MoveAction: target is unreachable");
            return NodeState.Failure;
        }
    
        if (m_Path == null || m_Path.Length == 0)
        {
            Debug.Log("MoveAction: waiting for path computation");
            return NodeState.Running; 
        }

        if (!m_IsFollowingPath)
            currentWaypoint = GetClosestWaypoint();

        m_IsFollowingPath = true;

        if (Vector3.Distance(m_SmartCrowdTransform.position, currentWaypoint) < 0.1f)
        {
            m_TargetIndex++;
            if (m_TargetIndex >= m_Path.Length)
            {
                if (m_KeyName != "")
                    m_Blackboard.Set(m_KeyName, true);

                Debug.Log("MoveAction: Target reached, moving on to next target");
                return NodeState.Success;
            }
            currentWaypoint = m_Path[m_TargetIndex];
        }

        m_SmartCrowdTransform.position = Vector3.MoveTowards(m_SmartCrowdTransform.position, currentWaypoint, m_MoveSpeed * Time.deltaTime);
        RotateTowards(currentWaypoint);
        Debug.Log("MoveAction: sentinel is moving towards target");
        return NodeState.Running;
    }

    /// <summary>
    /// Callback invoked when the path is updated. Restarts movement if the path has changed.
    /// </summary>
    void OnPathChanged(List<Node> newPath)
    {
        // Ignore path updates that aren't for this action's target
        if (m_PathFinding.Target != m_Target)
            return;

        m_TargetIndex = 0;
        m_IsFollowingPath = false;

        if (newPath == null)
        {
            m_Path = null;
            return;
        }

        Vector3[] newWorldPath = newPath.ConvertAll(n => n.WorldPosition).ToArray();

        if (m_Path != null && AreWorldPathsEqual(m_Path, newWorldPath)) return;

        m_Path = newWorldPath;
    }


    /// <summary>
    /// Compares two paths to determine if they are effectively the same.
    /// </summary>
    bool AreWorldPathsEqual(Vector3[] a, Vector3[] b)
    {
        if (a.Length != b.Length) return false;
        for (int i = 0; i < a.Length; i++)
        {
            if (Vector3.Distance(a[i], b[i]) > 0.1f) return false;
        }
        return true;
    }

    /// <summary>
    /// Rotates the unit to face the direction of movement.
    /// </summary>
    /*private void RotateTowards(Vector3 targetPosition)
    {
        Vector3 direction = (targetPosition - m_SmartCrowdTransform.position).normalized;
        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            m_SmartCrowdTransform.rotation = Quaternion.Slerp(m_SmartCrowdTransform.rotation, lookRotation, Time.deltaTime * 10f);
        }
    }*/
    
    private void RotateTowards(Vector3 targetPosition)
    {
        Vector3 direction = targetPosition - m_SmartCrowdTransform.position;
        direction.y = 0; // Keep rotation on horizontal plane only
    
        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            m_SmartCrowdTransform.rotation = Quaternion.Slerp(m_SmartCrowdTransform.rotation, lookRotation, Time.deltaTime * 10f);
        }
    }

    /// <summary>
    /// Finds the closest waypoint in the path to the unit's current position.
    /// </summary>
    private Vector3 GetClosestWaypoint()
    {
        float minDist = float.MaxValue;
        int closestIndex = 0;
        for (int i = 0; i < m_Path.Length; i++)
        {
            float dist = Vector3.Distance(m_SmartCrowdTransform.position, m_Path[i]);
            if (dist < minDist)
            {
                minDist = dist;
                closestIndex = i;
            }
        }
        m_TargetIndex = closestIndex;
        return m_Path[m_TargetIndex];
    }
}
