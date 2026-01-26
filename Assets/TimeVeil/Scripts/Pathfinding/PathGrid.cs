using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents a grid system used for pathfinding and obstacle detection.
/// Parent this GameObject to a room anchor to inherit its rotation automatically.
/// The grid is built on the XZ plane (horizontal floor) with Y as the vertical axis.
/// </summary>
public class PathGrid : MonoBehaviour
{
    // === Member Variables ===
    [Header("Configuration")]
    /// <summary>
    /// Layer mask used to detect unwalkable areas (obstacles).
    /// </summary>
    [SerializeField] private LayerMask m_UnwalkableMask;

    /// <summary>
    /// Layer mask used to detect walkable areas (floor).
    /// </summary>
    [SerializeField] private LayerMask m_WalkableMask;

    /// <summary>
    /// Size of the grid in world units (X = width, Y = height/vertical, Z = depth).
    /// </summary>
    [SerializeField] private Vector3 m_GridWorldSize = new Vector3(5, 0.5f, 5);

    /// <summary>
    /// Radius of each node in the grid.
    /// </summary>
    [SerializeField] private float m_NodeRadius = 0.1f;

    /// <summary>
    /// Radius of the unit to consider when checking for walkability.
    /// </summary>
    [SerializeField] private float m_UnitRadius = 0.1f;

    /// <summary>
    /// Toggle of the grid update
    /// </summary>
    [SerializeField] private bool m_RealTimeUpdate = false;

    /// <summary>
    /// Time interval between grid updates.
    /// </summary>
    [SerializeField] private float m_UpdateInterval = 0.5f;

    /// <summary>
    /// Timer used to track update intervals.
    /// </summary>
    private float m_UpdateTimer;

    /// <summary>
    /// 3D array representing the grid of nodes.
    /// </summary>
    private Node[,,] m_Grid;

    /// <summary>
    /// Diameter of each node (calculated from radius).
    /// </summary>
    private float m_NodeDiameter;

    /// <summary>
    /// Size of the grid in number of nodes (X, Y, Z).
    /// </summary>
    private Vector3Int m_GridSize;

    /// <summary>
    /// List of nodes representing the current path.
    /// </summary>
    public List<Node> m_Path;

    /// <summary>
    /// Stores the previous walkable state of each node for change detection.
    /// </summary>
    private bool[,,] m_PreviousWalkableState;

    // === Unity Methods ===

    /// <summary>
    /// Called when the script instance is being loaded.
    /// Initializes the grid.
    /// </summary>
    void Awake()
    {
        Initialize();
    }

    /// <summary>
    /// Called every frame. Updates the grid if necessary.
    /// </summary>
    void Update()
    {
        m_UpdateTimer += Time.deltaTime;
        if (m_UpdateTimer >= m_UpdateInterval && m_RealTimeUpdate)
        {
            if (HasGridChanged())
            {
                Initialize(); // Recreate the grid only if changes are detected
            }

            m_UpdateTimer = 0f;
        }
    }

    // === Custom Methods ===

    /// <summary>
    /// Initializes the grid by calculating dimensions and creating nodes.
    /// </summary>
    public void Initialize()
    {
        m_NodeDiameter = m_NodeRadius * 2;
        m_GridSize.x = Mathf.RoundToInt(m_GridWorldSize.x / m_NodeDiameter);
        m_GridSize.y = Mathf.RoundToInt(m_GridWorldSize.y / m_NodeDiameter);
        m_GridSize.z = Mathf.RoundToInt(m_GridWorldSize.z / m_NodeDiameter);
        
        // Ensure at least 1 node in each dimension
        m_GridSize.x = Mathf.Max(1, m_GridSize.x);
        m_GridSize.y = Mathf.Max(1, m_GridSize.y);
        m_GridSize.z = Mathf.Max(1, m_GridSize.z);
        
        CreateGrid();
    }

    /// <summary>
    /// Creates the grid and initializes each node with its walkability and position.
    /// Grid is built on the XZ plane (floor) with Y as vertical.
    /// Uses transform axes to support rotation inheritance from parent objects.
    /// </summary>
    void CreateGrid()
    {
        m_Grid = new Node[m_GridSize.x, m_GridSize.y, m_GridSize.z];
        
        // Use transform axes - rotation is inherited from parent hierarchy
        // right = X axis (width), up = Y axis (vertical), forward = Z axis (depth)
        Vector3 right = transform.right;
        Vector3 up = transform.up;
        Vector3 forward = transform.forward;
        
        // Start from bottom-left-back corner of the grid
        // Grid extends along X (right) and Z (forward), with Y (up) for vertical layers
        Vector3 gridOrigin = transform.position 
            - right * m_GridWorldSize.x / 2 
            - up * m_GridWorldSize.y / 2 
            - forward * m_GridWorldSize.z / 2;

        for (int x = 0; x < m_GridSize.x; x++)
        {
            for (int y = 0; y < m_GridSize.y; y++)
            {
                for (int z = 0; z < m_GridSize.z; z++)
                {
                    // Build grid on XZ plane (horizontal), Y is vertical
                    Vector3 worldPoint = gridOrigin 
                        + right * (x * m_NodeDiameter + m_NodeRadius)
                        + up * (y * m_NodeDiameter + m_NodeRadius)
                        + forward * (z * m_NodeDiameter + m_NodeRadius);

                    // Ensure node itself isn't colliding with unwalkable geometry
                    bool walkable = !(Physics.CheckSphere(worldPoint, m_NodeRadius + m_UnitRadius, m_UnwalkableMask));

                    if (walkable)
                    {
                        // Ensure node is directly above a walkable surface
                        // Raycast downward in world space (gravity direction)
                        RaycastHit hit;
                        float groundCheckDistance = m_NodeDiameter * 1.5f;
                        Vector3 rayOrigin = worldPoint + Vector3.up * m_NodeRadius * 0.5f;

                        // Only consider ground that belongs to the walkable layer mask
                        if (!Physics.Raycast(rayOrigin, Vector3.down, out hit, groundCheckDistance, m_WalkableMask))
                        {
                            walkable = false;
                        }
                    }

                    Vector3Int pos = new Vector3Int(x, y, z);
                    m_Grid[x, y, z] = new Node(walkable, worldPoint, pos);
                }
            }
        }

        m_PreviousWalkableState = new bool[m_GridSize.x, m_GridSize.y, m_GridSize.z];
        for (int x = 0; x < m_GridSize.x; x++)
        {
            for (int y = 0; y < m_GridSize.y; y++)
            {
                for (int z = 0; z < m_GridSize.z; z++)
                {
                    m_PreviousWalkableState[x, y, z] = m_Grid[x, y, z].IsWalkable;
                }
            }
        }
    }

    /// <summary>
    /// Checks if the walkability of any node has changed since the last update.
    /// </summary>
    /// <returns>True if any node's walkability has changed; otherwise, false.</returns>
    public bool HasGridChanged()
    {
        bool hasChanged = false;

        for (int x = 0; x < m_GridSize.x; x++)
        {
            for (int y = 0; y < m_GridSize.y; y++)
            {
                for (int z = 0; z < m_GridSize.z; z++)
                {
                    Vector3 worldPoint = m_Grid[x, y, z].WorldPosition;

                    // Perform walkability check
                    bool currentWalkable =
                        !(Physics.CheckSphere(worldPoint, m_NodeRadius + m_UnitRadius, m_UnwalkableMask));

                    // Add raycast check for ground
                    if (currentWalkable)
                    {
                        RaycastHit hit;
                        float groundCheckDistance = m_NodeDiameter * 1.5f;
                        Vector3 rayOrigin = worldPoint + Vector3.up * m_NodeRadius * 0.5f;

                        if (!Physics.Raycast(rayOrigin, Vector3.down, out hit, groundCheckDistance, m_WalkableMask))
                        {
                            currentWalkable = false;
                        }
                    }

                    // Compare with previous state
                    if (currentWalkable != m_PreviousWalkableState[x, y, z])
                    {
                        // To avoid false positives due to physics jitter, confirm change persists
                        int confirmationChecks = 3;
                        bool confirmedChange = true;

                        for (int i = 0; i < confirmationChecks; i++)
                        {
                            bool recheck = !(Physics.CheckSphere(worldPoint, m_NodeRadius + m_UnitRadius,
                                m_UnwalkableMask));
                            if (recheck != currentWalkable)
                            {
                                confirmedChange = false;
                                break;
                            }
                        }

                        if (confirmedChange)
                        {
                            hasChanged = true;
                            break;
                        }
                    }
                }

                if (hasChanged) break;
            }

            if (hasChanged) break;
        }

        return hasChanged;
    }

    /// <summary>
    /// Gets the neighboring nodes of a given node.
    /// </summary>
    /// <param name="node">The node to find neighbors for.</param>
    /// <returns>List of neighboring nodes.</returns>
    public List<Node> GetNeighbours(Node node)
    {
        List<Node> neighbours = new List<Node>();

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                for (int z = -1; z <= 1; z++)
                {
                    if (x == 0 && y == 0 && z == 0) continue;
                    int checkX = node.Grid.x + x;
                    int checkY = node.Grid.y + y;
                    int checkZ = node.Grid.z + z;

                    if (checkX >= 0 && checkX < m_GridSize.x &&
                        checkY >= 0 && checkY < m_GridSize.y &&
                        checkZ >= 0 && checkZ < m_GridSize.z)
                    {
                        neighbours.Add(m_Grid[checkX, checkY, checkZ]);
                    }
                }
            }
        }

        return neighbours;
    }

    /// <summary>
    /// Converts a world position to the corresponding node in the grid.
    /// Accounts for transform rotation inherited from parent objects.
    /// </summary>
    /// <param name="worldPosition">The world position to convert.</param>
    /// <returns>The corresponding node in the grid.</returns>
    public Node NodeFromWorldPoint(Vector3 worldPosition)
    {
        // Transform world position to local grid-aligned space
        // InverseTransformPoint handles both position offset and rotation
        Vector3 localPosition = transform.InverseTransformPoint(worldPosition);

        // Calculate percentage position within grid bounds
        float percentX = Mathf.Clamp01((localPosition.x + m_GridWorldSize.x / 2) / m_GridWorldSize.x);
        float percentY = Mathf.Clamp01((localPosition.y + m_GridWorldSize.y / 2) / m_GridWorldSize.y);
        float percentZ = Mathf.Clamp01((localPosition.z + m_GridWorldSize.z / 2) / m_GridWorldSize.z);

        int x = Mathf.RoundToInt((m_GridSize.x - 1) * percentX);
        int y = Mathf.RoundToInt((m_GridSize.y - 1) * percentY);
        int z = Mathf.RoundToInt((m_GridSize.z - 1) * percentZ);

        return m_Grid[x, y, z];
    }

    /// <summary>
    /// Check if the target is inside the grid.
    /// Accounts for transform rotation inherited from parent objects.
    /// </summary>
    /// <param name="worldPosition">The world position to check.</param>
    /// <returns>True if the position is inside the grid bounds.</returns>
    public bool IsPositionInsideGrid(Vector3 worldPosition)
    {
        // Transform to local space (handles rotation from parent)
        Vector3 localPosition = transform.InverseTransformPoint(worldPosition);
        
        return localPosition.x >= -m_GridWorldSize.x / 2 && localPosition.x <= m_GridWorldSize.x / 2 &&
               localPosition.y >= -m_GridWorldSize.y / 2 && localPosition.y <= m_GridWorldSize.y / 2 &&
               localPosition.z >= -m_GridWorldSize.z / 2 && localPosition.z <= m_GridWorldSize.z / 2;
    }

    /// <summary>
    /// Draws gizmos in the editor to visualize the grid and path.
    /// Uses transform rotation for proper visualization.
    /// </summary>
    void OnDrawGizmos()
    {
        // Draw rotated wire cube using the transform's orientation
        Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, m_GridWorldSize);
        Gizmos.matrix = Matrix4x4.identity;

        if (m_Grid != null)
        {
            foreach (Node n in m_Grid)
            {
                if (n.IsWalkable)
                {
                    Gizmos.color = Color.white;
                    Gizmos.DrawCube(n.WorldPosition, Vector3.one * (m_NodeDiameter - 0.1f));
                }
            }
        }

        if (m_Path != null)
        {
            foreach (Node n in m_Path)
            {
                Gizmos.color = Color.black;
                Gizmos.DrawCube(n.WorldPosition, Vector3.one * (m_NodeDiameter - 0.1f));
            }
        }
    }

    /// <summary>
    /// Sets the grid world size and re-initializes if needed.
    /// </summary>
    /// <param name="gridWorldSize">New grid dimensions (X = width, Y = height, Z = depth).</param>
    public void SetGridWorldSize(Vector3 gridWorldSize)
    {
        m_GridWorldSize = gridWorldSize;
    }
}