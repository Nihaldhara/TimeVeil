using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Controls a crowd agent using a behavior tree.
/// The agent can move, dance, and react to traffic light states.
/// </summary>
[RequireComponent(typeof(Pathfinding))]
public class SmartAgent : MonoBehaviour
{
    private IBTNode m_BehaviorTree;
    private Blackboard m_Blackboard;

    private List<GameObject> m_TargetsList = new List<GameObject>();

    public List<GameObject> TargetsList
    {
        get => m_TargetsList;
        set
        {
            m_TargetsList = value;
            Debug.Log("[My Debug] Set targets list in SmartAgent");
            BuildBehaviourTree();
        }
    }

    private int m_CurrentTarget = 0;

    [Header("Movement")]
    private float m_MoveSpeed = 0.02f;

    /// <summary>
    /// 
    /// </summary>
    public float MoveSpeed
    {
        set { m_MoveSpeed = value; }
        get { return m_MoveSpeed; }
    }

    private Pathfinding m_Pathfinding;

    /// <summary>
    /// Reference Pathfinding
    /// </summary>
    public Pathfinding Pathfinding
    {
        get { return m_Pathfinding; }
    }

    private Transform m_PlayerPosition;

    /// <summary>
    /// Reference Pathfinding
    /// </summary>
    public Transform PlayerPosition
    {
        get { return m_PlayerPosition; }
    }

    /// <summary>
    /// Initializes the behavior tree and blackboard on awake.
    /// </summary>
    void Awake()
    {
        m_Blackboard = new Blackboard();
        m_Pathfinding = GetComponent<Pathfinding>();

        // Store references in the blackboard
        m_Blackboard.Set("CrowdTransform", transform);
        m_Blackboard.Set("PathFinding", m_Pathfinding);

        var player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            m_Blackboard.Set("PlayerTransform", player.transform);
            m_PlayerPosition = player.transform;
            Debug.Log("[My Debug] Player found");
        }
    }

    /// <summary>
    /// Evaluates the behavior tree every frame.
    /// </summary>
    void Update()
    {
        if (m_BehaviorTree != null)
        {
            Debug.Log(m_BehaviorTree.Evaluate());
        }
        else
        {
            Debug.Log("[My Debug] BehaviourTree is null!!");
        }
    }

    public void BuildBehaviourTree()
    {
        Debug.Log("[My Debug] Building behaviour tree");

        if (m_TargetsList.Count > 1)
        {
            m_Pathfinding.Target = m_TargetsList[m_CurrentTarget].transform;
        }

        List<IBTNode> moveTargetsActionNodes = new List<IBTNode>();
        foreach (var target in m_TargetsList)
        {
            moveTargetsActionNodes.Add(new MoveAction(m_Blackboard, m_MoveSpeed, target.transform));
        }

        IBTNode playerCloseConditionNode = new IsPlayerClose(m_Blackboard);
        IBTNode moveToPlayerAction = new MoveAction(m_Blackboard, m_MoveSpeed, m_PlayerPosition);
        IBTNode sentinelWatchSelector = new SelectorNode(
            new WaitUntilConditionCompleteDecorator(m_Blackboard, playerCloseConditionNode, moveToPlayerAction),
            new SequenceNode(moveTargetsActionNodes.ToArray())
        );
        IBTNode puzzleSolvedConditionNode = new HasFirstPuzzleBeenSolved(true);

        m_BehaviorTree = new WaitUntilConditionCompleteDecorator(m_Blackboard, puzzleSolvedConditionNode, sentinelWatchSelector);;
        Debug.Log("[My Debug] Behaviour tree built successfully.");
    }

    public void AddTarget(GameObject newTarget)
    {
        m_TargetsList.Add(newTarget);
    }
}