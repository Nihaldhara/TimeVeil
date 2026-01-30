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

        List<IBTNode> moveTargetsActionNodes = new List<IBTNode>();
        foreach (var target in m_TargetsList)
        {
            moveTargetsActionNodes.Add(new MoveAction(m_Blackboard, m_MoveSpeed));
        }

        IBTNode playerCloseConditionNode = new IsPlayerSeen();
        IBTNode moveToPlayerAction = new MoveAction(m_Blackboard, m_RunSpeed); //playerPosition
        IBTNode sentinelWatchSelector = new SelectorNode(
            new WaitUntilConditionCompleteDecorator(m_Blackboard, playerCloseConditionNode, moveToPlayerAction),
            new SequenceNode(
                moveTargetsActionNodes.ToArray()
                )
        );
        IBTNode puzzleSolvedConditionNode = new HasFirstPuzzleBeenSolved();

        m_BehaviorTree = new WaitUntilConditionCompleteDecorator(m_Blackboard, puzzleSolvedConditionNode, sentinelWatchSelector);;
        Debug.Log("[My Debug] Behaviour tree built successfully.");

        m_Pathfinding.AgentNeedRepath = true;
    }
    
    /*
     m_BehaviorTree = new SelectorNode(
            moveTargetActionNode,
            new SequenceNode(
                new IsNotDoneCondition(m_Blackboard, "HasCrossed"),
                new SelectorNode(
                    new SequenceNode(
                        new IsNotDoneCondition(m_Blackboard, "OnEntryPointCrosswalk"),
                        moveTargetRecoverActionNode),
                    new WaitUntilConditionCompleteDecorator(m_Blackboard, IsLightStateconditionNode, CrosswalkAction)
                )


        )

     */
}