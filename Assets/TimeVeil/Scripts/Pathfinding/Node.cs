using UnityEngine;

/// <summary>
/// Represents a single node in the grid used for pathfinding.
/// Stores walkability, world position, grid coordinates, pathfinding costs, and parent reference.
/// </summary>
public class Node
{
    /// <summary>
    /// Indicates whether the node is walkable (not blocked by an obstacle).
    /// </summary>
    private bool m_IsWalkable;

    /// <summary>
    /// Public accessor for walkability status.
    /// </summary>
    public bool IsWalkable
    {
        get { return m_IsWalkable; }
        set { m_IsWalkable = value; }
    }

    /// <summary>
    /// The position of the node in world space.
    /// </summary>
    private Vector3 m_WorldPosition;

    /// <summary>
    /// Public accessor for the world position.
    /// </summary>
    public Vector3 WorldPosition
    {
        get { return m_WorldPosition; }
        set { m_WorldPosition = value; }
    }

    /// <summary>
    /// The grid coordinates of the node (used for indexing in the grid).
    /// </summary>
    private Vector3Int m_Grid;

    /// <summary>
    /// Gets or sets the grid coordinates of the node.
    /// </summary>
    public Vector3Int Grid
    {
        get { return m_Grid; }
        set { m_Grid = value; }
    }

    /// <summary>
    /// The cost from the start node to this node.
    /// </summary>
    private int m_GCost;

    /// <summary>
    /// Public accessor for the G cost.
    /// </summary>
    public int GCost
    {
        get { return m_GCost; }
        set { m_GCost = value; }
    }

    /// <summary>
    /// The heuristic cost from this node to the target node.
    /// </summary>
    private int m_HCost;

    /// <summary>
    /// Public accessor for the H cost.
    /// </summary>
    public int HCost
    {
        get { return m_HCost; }
        set { m_HCost = value; }
    }

    /// <summary>
    /// Reference to the parent node in the path.
    /// </summary>
    private Node m_Parent;

    /// <summary>
    /// Public accessor for the parent node.
    /// </summary>
    public Node Parent
    {
        get { return m_Parent; }
        set { m_Parent = value; }
    }

    /// <summary>
    /// The total cost (F = G + H), used for node comparison in A*.
    /// </summary>
    private int m_FCost => m_GCost + m_HCost;

    /// <summary>
    /// Public accessor for the F cost.
    /// </summary>
    public int FCost
    {
        get { return m_FCost; }
    }

    /// <summary>
    /// Constructor to initialize a node with walkability, world position, and grid coordinates.
    /// </summary>
    /// <param name="walkable">Whether the node is walkable</param>
    /// <param name="worldPos">World position of the node</param>
    /// <param name="grid">Grid coordinates of the node</param>
    public Node(bool walkable, Vector3 worldPos, Vector3Int grid)
    {
        m_IsWalkable = walkable;
        m_WorldPosition = worldPos;
        m_Grid = grid;
    }
}
