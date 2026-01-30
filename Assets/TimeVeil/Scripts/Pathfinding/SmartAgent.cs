using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Controls a crowd agent using a behavior tree.
/// </summary>
[RequireComponent(typeof(Pathfinding))]
public class SmartAgent : MonoBehaviour
{
    private IBTNode m_BehaviorTree;
    private Blackboard m_Blackboard;

    private List<Transform> m_TargetsList = new List<Transform>();

    public List<Transform> TargetsList
    {
        get => m_TargetsList;
        set
        {
            m_TargetsList = value;
            Debug.Log("[My Debug] Set targets list in SmartAgent");
            Init();
            BuildBehaviourTree();
        }
    }

    private int m_CurrentTarget = 0;

    [Header("Movement")]
    private float m_MoveSpeed = 1.0f;

    private float m_RunSpeed = 5.0f;

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
    /// Initializes the behavior tree and blackboard on awake.
    /// </summary>
    void Awake()
    {

    }

    void Init ()
    {
        m_Blackboard = new Blackboard();
        m_Pathfinding = GetComponent<Pathfinding>();

        // Store references in the blackboard
        m_Blackboard.Set("CrowdTransform", transform);
        m_Blackboard.Set("Sentinel", gameObject);
        m_Blackboard.Set("PathFinding", m_Pathfinding);
        m_Blackboard.Set("TargetsList", m_TargetsList);
        m_Blackboard.Set("CurrentTarget", m_TargetsList[0]);  

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
            m_BehaviorTree.Evaluate();
            //Debug.Log(m_BehaviorTree.Evaluate());
        }
        else
        {
            Debug.Log("[My Debug] BehaviourTree is null!!");
        }
    }

    public void BuildBehaviourTree()
    {
        Debug.Log("[My Debug] Building behaviour tree");

         IBTNode checkPlayer = new IsPlayerSeen(m_Blackboard);
         //IBTNode moveToPlayer = new MoveAction(m_Blackboard, m_RunSpeed);
         IBTNode moveToTarget = new MoveAction(m_Blackboard, m_MoveSpeed);
         IBTNode isNotReached = new IsNotReachedCondition(m_Blackboard);
         IBTNode waitNode = new WaitAction(10.0f, 5.0f);
         IBTNode selectTarget = new SelectTarget(m_Blackboard);
         IBTNode puzzleSolvedConditionNode = new HasFirstPuzzleBeenSolved();
        
         IBTNode patrolSequence = new SequenceNode(moveToTarget, isNotReached, waitNode, selectTarget);
        IBTNode catchingSequence = new SequenceNode(checkPlayer, moveToTarget);

        IBTNode sentinelWatchSelector = new SelectorNode(catchingSequence, patrolSequence);
        
        //m_BehaviorTree = new WaitUntilConditionCompleteDecorator(m_Blackboard, puzzleSolvedConditionNode, sentinelWatchSelector);
         //m_BehaviorTree = new SelectorNode(catchingSequence, patrolSequence);
         m_BehaviorTree = sentinelWatchSelector;
        Debug.Log("[My Debug] Behaviour tree built successfully.");

        m_Pathfinding.AgentNeedRepath = true;
    }
}