using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField] private float m_TotalTime = 60.0f;
    private float m_RemainingTime;

    private ServerGameManager m_ServerGameManager;
    
    public UnityEvent ClientConnectedEvent;
    
    public bool checkFirstPuzzleState = false;
    
    [HideInInspector]
    public UnityEvent PlayerEnteredTriggerEvent;
    
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

    private List<GameObject> m_PuzzlesList = new List<GameObject>();
    public List<GameObject> PuzzlesList
    {
        get => m_PuzzlesList;
        set
        {
            m_PuzzlesList = value;
            Debug.Log("GameManager: PuzzlesList was set and it contains this amount of puzzles " + m_PuzzlesList.Count);
        }
    }

    public bool PlayerDead = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        m_RemainingTime = m_TotalTime;
        
        m_ServerGameManager = ServerGameManager.Instance;
        m_CurrentGameState = GameState.Started;
        
        m_ServerGameManager.ClientConnectionEvent.AddListener(OnClientConnected);
    }

    // Update is called once per frame
    void Update()
    {
        if (m_CurrentGameState == GameState.Waiting)
        {
            return;
        }
         
        if (PlayerDead || m_RemainingTime <= 0.0f)
        {
            m_CurrentGameState = GameState.Failed;
            SceneManager.LoadScene(1);
        }
        else
        {
            m_RemainingTime -= Time.deltaTime;
            
            int solvedCount = 0;
            for (int i = 0; i < m_PuzzlesList.Count; i++)
            {
                
                // Temporary fix
                if (m_PuzzlesList[i].GetComponent<Puzzle>())
                    break;
                
                if (!m_PuzzlesList[i].GetComponent<Puzzle>().Solved)
                    break;
                solvedCount++;
                Debug.Log("GameManager: A puzzle has been solved, now there are " + solvedCount);
                checkFirstPuzzleState = true;
            }
    
            m_CurrentGameState = solvedCount switch
            {
                3 => GameState.Succeeded,
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
    }

    void OnGameStateChanged()
    {
        string gameState = Convert.ToInt32(m_CurrentGameState).ToString();
        
        string data = (int)DataLabel.GameState + "|" + gameState;
            
        Debug.Log("GameState: " + m_CurrentGameState);
        m_ServerGameManager.DataReliableSendEvent.Invoke(data, 0);
    }
}
