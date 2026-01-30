using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

/// <summary>
/// Manages path calculation between an agent and a target using the A* algorithm.
/// Monitors position and grid changes to dynamically recalculate the path.
/// </summary>
public class Pathfinding : MonoBehaviour
{
    [Header("Configuration")]
    /// <summary>
    /// Reference to the object seeking the path.
    /// </summary>
    [SerializeField] private Transform m_Agent;

    /// <summary>
    /// Reference to the target to reach.
    /// </summary>
    [SerializeField]private Transform m_Target;

    /// <summary>
    /// Public accessor for Current Target.
    /// </summary>
    public Transform Target
    {
        get { return m_Target; }
        set { m_Target = value; }
    }


    /// <summary>
    /// Reference to the grid used for pathfinding.
    /// </summary>
    private PathGrid m_Grid;

    /// <summary>
    /// Property reference to the grid used for pathfinding.
    /// </summary>
    public PathGrid Grid
    {
        get { return m_Grid; }
        set { m_Grid = value; }
    }

    /// <summary>
    /// Diagonal cost offset
    /// </summary>
    [SerializeField] int m_BigDiagonalCost = 17;

    /// <summary>
    /// Diagonal cost offset
    /// </summary>
    [SerializeField] int m_DiagonalCost = 14;

    /// <summary>
    /// Straight cost offset
    /// </summary>
    [SerializeField] int m_StraightCost = 10;

    /// <summary>
    /// Interval in seconds between path updates.
    /// </summary>
    [SerializeField] private float m_UpdateInterval = 0.5f;

    /// <summary>
    /// Last calculated path as a list of nodes.
    /// </summary>
    private List<Node> m_LastPath;

    /// <summary>
    /// Timer used to track update intervals.
    /// </summary>
    private float m_UpdateTimer;

    /// <summary>
    /// Last known position of the agent.
    /// </summary>
    private Vector3 m_LastAgentPos;

    /// <summary>
    /// Last known position of the target.
    /// </summary>
    private Vector3 m_LastTargetPos;

    /// <summary>
    /// Delegate instance for path update notifications.
    /// </summary>
    public delegate void OnPathUpdated(List<Node> newPath);

    private event OnPathUpdated m_OnPathUpdated;


    /// <summary>
    /// Public event to subscribe to path updates.
    /// </summary>
    public event OnPathUpdated OnPathUpdatedEvent
    {
        add { m_OnPathUpdated += value; }
        remove { m_OnPathUpdated -= value; }
    }

    private bool m_AgentNeedRepath;

    public bool AgentNeedRepath
    {
        get { return m_AgentNeedRepath; }
        set { m_AgentNeedRepath = value; }
    }


    /// <summary>
    /// Checks if the grid is properly assigned.
    /// </summary>
    void Awake()
    {
        if (!m_Grid)
        {
            Debug.Log("[My Debug] Grid Component not set");
        }
    }

    /// <summary>
    /// Initializes positions and calculates the initial path.
    /// </summary>
    void Start()
    {
        if (!m_Agent)
            m_Agent = transform;
        //m_LastAgentPos = m_Agent.position;
        m_LastTargetPos = m_Agent.position;
        //FindPath(m_Agent.position, m_Target.position);
    }

    /// <summary>
    /// Updates the path if the agent, target, or grid has changed.
    /// </summary>
    void Update()
    {
        m_UpdateTimer += Time.deltaTime;
        if (m_UpdateTimer >= m_UpdateInterval)
        {
            m_UpdateTimer = 0f;
            //bool seekerMoved = Vector3.Distance(m_Agent.position, m_LastAgentPos) > 0.1f;
            bool targetMoved = Vector3.Distance(m_Target.position, m_LastTargetPos) > 0.1f;
            bool gridChanged = m_Grid.HasGridChanged();
            //Debug.Log($" Target Moved {targetMoved} and Grid Changed {gridChanged}");

            if ( /*seekerMoved ||*/ m_AgentNeedRepath || targetMoved || gridChanged)
            {
                //Debug.Log($"[My Debug] Route Recalculated ...");
                FindPath(m_Agent.position, m_Target.position);
                m_LastAgentPos = m_Agent.position;
                m_LastTargetPos = m_Target.position;
                m_AgentNeedRepath = false;
            }
        }
    }
    
    /// <summary>
    /// Calculates the path between two positions using the A* algorithm.
    /// </summary>
    /// <param name="startPos">The starting position.</param>
    /// <param name="targetPos">The target position.</param>
    public void FindPath(Vector3 startPos, Vector3 targetPos)
    {
        if (!m_Grid.IsPositionInsideGrid(targetPos))
        {
            Debug.Log("[My Debug] Target outside grid: Path is null");
            m_Grid.m_Path = null;
            m_OnPathUpdated?.Invoke(null);
            return;
        }
        
        if (!m_Grid.IsPositionInsideGrid(m_Agent.position))
        {
            Debug.Log("[My Debug] Agent outside grid");
        }

        Node startNode = m_Grid.NodeFromWorldPoint(startPos);
        Node targetNode = m_Grid.NodeFromWorldPoint(targetPos);

        List<Node> openSet = new List<Node>();
        HashSet<Node> closedSet = new HashSet<Node>();
        openSet.Add(startNode);

        while (openSet.Count > 0)
        {
            Node currentNode = openSet[0];
            for (int i = 1; i < openSet.Count; i++)
            {
                if (openSet[i].FCost < currentNode.FCost ||
                    openSet[i].FCost == currentNode.FCost && openSet[i].HCost < currentNode.HCost)
                {
                    currentNode = openSet[i];
                }
            }

            openSet.Remove(currentNode);
            closedSet.Add(currentNode);

            if (currentNode == targetNode)
            {
                Debug.Log("[My Debug] Retracing path to start node");
                RetracePath(startNode, targetNode);
                return;
            }

            foreach (Node neighbour in m_Grid.GetNeighbours(currentNode))
            {
                if (!neighbour.IsWalkable || closedSet.Contains(neighbour)) continue;

                int newCostToNeighbour = currentNode.GCost + GetDistance(currentNode, neighbour);
                if (newCostToNeighbour < neighbour.GCost || !openSet.Contains(neighbour))
                {
                    neighbour.GCost = newCostToNeighbour;
                    neighbour.HCost = GetDistance(neighbour, targetNode);
                    neighbour.Parent = currentNode;

                    if (!openSet.Contains(neighbour)) openSet.Add(neighbour);
                }
            }
        }


        Debug.Log("[My Debug] Target unreachable: Path is null");
        m_Grid.m_Path = null;
        m_OnPathUpdated?.Invoke(null);
    }

    /// <summary>
    /// Check if target can be reachable
    /// </summary>
    /// <returns>True if reachable or False</returns>
    public bool IsTargetReachable(Vector3 targetPos)
    {
        Debug.Log($"[IsTargetReachable] Checking target at {targetPos}");
        Debug.Log($"[IsTargetReachable] Agent position: {m_Agent.position}");
    
        // Check if target is inside the grid
        if (!m_Grid.IsPositionInsideGrid(targetPos))
        {
            Debug.LogWarning($"[IsTargetReachable] Target {targetPos} is OUTSIDE grid bounds");
            return false;
        }

        Node startNode = m_Grid.NodeFromWorldPoint(m_Agent.position);
        // Temporarily add this right after getting the start node
        var neighbours = m_Grid.GetNeighbours(startNode);
        Debug.Log($"[IsTargetReachable] Start node has {neighbours.Count} neighbours:");
        foreach (var n in neighbours)
        {
            Debug.Log($"  - ({n.Grid.x}, {n.Grid.y}) Walkable: {n.IsWalkable}");
        }
        
        Node targetNode = m_Grid.NodeFromWorldPoint(targetPos);

        Debug.Log($"[IsTargetReachable] Start node: ({startNode.Grid.x}, {startNode.Grid.y}), Walkable: {startNode.IsWalkable}");
        Debug.Log($"[IsTargetReachable] Target node: ({targetNode.Grid.x}, {targetNode.Grid.y}), Walkable: {targetNode.IsWalkable}");

        if (!startNode.IsWalkable)
        {
            Debug.LogWarning("[IsTargetReachable] START node is not walkable!");
            return false;
        }
    
        if (!targetNode.IsWalkable)
        {
            Debug.LogWarning("[IsTargetReachable] TARGET node is not walkable!");
            return false;
        }

        // A* search...
        List<Node> openSet = new List<Node> { startNode };
        HashSet<Node> closedSet = new HashSet<Node>();

        while (openSet.Count > 0)
        {
            Node currentNode = openSet[0];
            for (int i = 1; i < openSet.Count; i++)
            {
                if (openSet[i].FCost < currentNode.FCost ||
                    (openSet[i].FCost == currentNode.FCost && openSet[i].HCost < currentNode.HCost))
                {
                    currentNode = openSet[i];
                }
            }

            openSet.Remove(currentNode);
            closedSet.Add(currentNode);

            if (currentNode == targetNode)
            {
                return true; // Path exists
            }

            foreach (Node neighbour in m_Grid.GetNeighbours(currentNode))
            {
                if (!neighbour.IsWalkable || closedSet.Contains(neighbour)) continue;

                int newCostToNeighbour = currentNode.GCost + GetDistance(currentNode, neighbour);
                if (newCostToNeighbour < neighbour.GCost || !openSet.Contains(neighbour))
                {
                    neighbour.GCost = newCostToNeighbour;
                    neighbour.HCost = GetDistance(neighbour, targetNode);
                    neighbour.Parent = currentNode;

                    if (!openSet.Contains(neighbour)) openSet.Add(neighbour);
                }
            }
        }

        Debug.LogWarning($"[IsTargetReachable] NO PATH FOUND. Closed set: {closedSet.Count} nodes. Start: ({startNode.Grid.x},{startNode.Grid.y}) Target: ({targetNode.Grid.x},{targetNode.Grid.y})");
        return false; // No path found
    }


    /// <summary>
    /// Reconstructs the path from the target node back to the start node.
    /// </summary>
    /// <param name="startNode">The starting node.</param>
    /// <param name="endNode">The target node.</param>
    void RetracePath(Node startNode, Node endNode)
    {
        List<Node> newPath = new List<Node>();
        Node currentNode = endNode;

        while (currentNode != startNode)
        {
            newPath.Add(currentNode);
            currentNode = currentNode.Parent;
        }

        newPath.Add(startNode);
        newPath.Reverse();

        if (newPath.Count == 0)
        {
            return;
        }

        m_Grid.m_Path = newPath;

        if (m_LastPath == null || !ArePathsEqual(m_LastPath, newPath))
        {
            m_LastPath = newPath;
            m_OnPathUpdated?.Invoke(newPath);
        }
    }


    /// <summary>
    /// Compares two paths to determine if they are identical.
    /// </summary>
    /// <param name="pathA">First path to compare.</param>
    /// <param name="pathB">Second path to compare.</param>
    /// <returns>True if paths are equal; otherwise, false.</returns>
    bool ArePathsEqual(List<Node> pathA, List<Node> pathB)
    {
        if (pathA.Count != pathB.Count) return false;

        for (int i = 0; i < pathA.Count; i++)
        {
            if (Vector3.Distance(pathA[i].WorldPosition, pathB[i].WorldPosition) > 0.1f)
                return false;
        }

        return true;
    }


    /// <summary>
    /// Calculates the distance between two nodes, considering diagonal movement.
    /// </summary>
    /// <param name="a">First node.</param>
    /// <param name="b">Second node.</param>
    /// <returns>Distance cost between the nodes.</returns>
    int GetDistance(Node a, Node b)
    {
        int dstX = Mathf.Abs(a.Grid.x - b.Grid.x);
        int dstY = Mathf.Abs(a.Grid.y - b.Grid.y);
        int dstZ = Mathf.Abs(a.Grid.z - b.Grid.z);

        if (dstX > dstY && dstX > dstZ)
        {
            return m_BigDiagonalCost * dstZ + m_DiagonalCost * (dstY - dstZ) + m_StraightCost * (dstX - dstY);
        }
        else if (dstY > dstX && dstY > dstZ)
        {
            return m_BigDiagonalCost * dstZ + m_DiagonalCost * (dstX - dstZ) + m_StraightCost * (dstY - dstX);
        }
        else
            return m_BigDiagonalCost * dstY + m_DiagonalCost * (dstX - dstY) + m_StraightCost * (dstZ - dstX);
    }
}