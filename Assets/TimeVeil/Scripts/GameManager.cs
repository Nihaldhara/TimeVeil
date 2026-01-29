using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    [SerializeField] private float m_TotalTime = 60.0f;
    private float m_RemainingTime;

    private ServerGameManager m_ServerGameManager;
    
    public UnityEvent ClientConnectedEvent;
    
    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    
    private GameState _currentGameState;
    private GameState m_CurrentGameState
    {
        get => _currentGameState;
        set
        {
            if (_currentGameState != value)
            {
                _currentGameState = value;
                OnGameStateChanged();
            }
        }
    }

    private List<GameObject> m_PuzzlesList;

    public List<GameObject> PuzzlesList
    {
        get => m_PuzzlesList;
        set => m_PuzzlesList = value;
    }

    public bool PlayerDead = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        m_RemainingTime = m_TotalTime;
        
        m_ServerGameManager = ServerGameManager.Instance;
        m_CurrentGameState = GameState.Waiting;
        Debug.Log("GameState: " + m_CurrentGameState);
        
        m_ServerGameManager.ClientConnectionEvent.AddListener(OnClientConnected);
    }

    // Update is called once per frame
    void Update()
    {
        if (m_CurrentGameState == GameState.Waiting)
        {
            Debug.Log("GameState: " + m_CurrentGameState);
            return;
        }
         
        if (PlayerDead || m_RemainingTime <= 0.0f)
        {
            m_CurrentGameState = GameState.Failed;
            Debug.Log("GameState: " + m_CurrentGameState);
        }
        else
        {
            m_RemainingTime -= Time.deltaTime;
            
            int solvedCount = 0;
            for (int i = 0; i < m_PuzzlesList.Count; i++)
            {
                if (!m_PuzzlesList[i].GetComponent<Puzzle>().Solved)
                    break;
                solvedCount++;
            }
    
            m_CurrentGameState = solvedCount switch
            {
                5 => GameState.Succeeded,
                4 => GameState.Stage4,
                3 => GameState.Stage3,
                2 => GameState.Stage2,
                1 => GameState.Stage1,
                _ => m_CurrentGameState
            };
        }
    }

    void OnClientConnected()
    {
        m_CurrentGameState = GameState.Started;
        ClientConnectedEvent.Invoke();
        Debug.Log("GameState: " + m_CurrentGameState);
    }

    void OnGameStateChanged()
    {
        string gameState = Convert.ToInt32(m_CurrentGameState).ToString();
        
        string data = (int)DataLabel.GameState + "|" + gameState;
        
        m_ServerGameManager.DataReliableSendEvent.Invoke(data, 0);
    }
}
