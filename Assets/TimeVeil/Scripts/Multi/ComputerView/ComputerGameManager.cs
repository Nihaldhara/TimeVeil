using System;
using System.Collections.Generic;
using System.Globalization;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ComputerGameManager : MonoBehaviour
{
    [SerializeField] private float m_TotalTime = 60.0f;
    private float m_RemainingTime;

    [SerializeField] private TMP_Text m_TimerDisplay;

    [SerializeField] private GameObject puzzlePrefab;
    [SerializeField] private TMP_Text clueText;
    
    [SerializeField] private GameObject wallPrefab;
   
    [SerializeField] private Transform playerTransform;
    
    [SerializeField] private Transform sentinelTransform;
    
    [Header("Room Centering")]
    [SerializeField] private Transform m_RoomCenter;
    
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
    
    private ClientGameManager m_ClientGameManager;
    
    private List<GameObject> m_PuzzlesList = new List<GameObject>();
    private GameObject m_ActivePuzzle;
    
    public static ComputerGameManager Instance { get; private set; }

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

    private void Start()
    {
        m_RemainingTime = m_TotalTime;
        m_ClientGameManager = ClientGameManager.Instance;
        
        m_ClientGameManager.SentinelPositionReceiveEvent.AddListener(PlaceSentinel);
        m_ClientGameManager.PlayerPositionReceiveEvent.AddListener(PlacePlayer);
        m_ClientGameManager.WallTransformReceiveEvent.AddListener(InitializeRoom);
        m_ClientGameManager.PuzzleInfoReceiveEvent.AddListener(InitializePuzzles);
        m_ClientGameManager.GameStateReceiveEvent.AddListener(ReceiveGameState);
    }

    private void Update()
    {
        m_RemainingTime -= Time.deltaTime;
        
        float minutes = Mathf.Floor(m_RemainingTime / 60);
        float seconds = m_RemainingTime % 60;
        string niceTime = string.Format("{0:0}:{1:00}", minutes, seconds);

        m_TimerDisplay.text = niceTime;
        
        if (m_RemainingTime <= 0.0f)
        {
            TimerEnded();
        }
    }

    void TimerEnded()
    {
        clueText.text = "Your partner is lost forever in the 11th century, and will probably die at the hands of the Normands...";
    }
    
    private Vector3 ToLocalSpace(Vector3 worldPos)
    {
        return m_RoomCenter.position + worldPos;
    }
    
    private void InitializePuzzles(string data)
    {
        Debug.Log(data);
        string[] parsedData = data.Split('/');
        Vector3 position = new Vector3(
            float.Parse(parsedData[0], CultureInfo.InvariantCulture), 
            float.Parse(parsedData[1], CultureInfo.InvariantCulture), 
            float.Parse(parsedData[2], CultureInfo.InvariantCulture));
        string clue = parsedData[3];
        
        GameObject newPuzzle = Instantiate(puzzlePrefab, ToLocalSpace(position), Quaternion.identity, m_RoomCenter);
        newPuzzle.GetComponent<ComputerPuzzle>().ClueText = clueText;
        newPuzzle.GetComponent<ComputerPuzzle>().Clue = clue;
        newPuzzle.SetActive(false);
        m_PuzzlesList.Add(newPuzzle);
    }
    
    private void InitializeRoom(string data)
    {
        Debug.Log(data);
        string[] parsedData = data.Split('/');
        Vector3 position = new Vector3(
            float.Parse(parsedData[0], CultureInfo.InvariantCulture), 
            float.Parse(parsedData[1], CultureInfo.InvariantCulture), 
            float.Parse(parsedData[2], CultureInfo.InvariantCulture));
        Quaternion rotation = new Quaternion(
            float.Parse(parsedData[3], CultureInfo.InvariantCulture), 
            float.Parse(parsedData[4], CultureInfo.InvariantCulture), 
            float.Parse(parsedData[5], CultureInfo.InvariantCulture),
            float.Parse(parsedData[6], CultureInfo.InvariantCulture));
        Vector3 scale = new Vector3(
            float.Parse(parsedData[7], CultureInfo.InvariantCulture), 
            float.Parse(parsedData[8], CultureInfo.InvariantCulture), 
            float.Parse(parsedData[9], CultureInfo.InvariantCulture));
        
        wallPrefab.transform.localScale = scale;
        GameObject newWall = Instantiate(wallPrefab, ToLocalSpace(position), rotation, m_RoomCenter);
    }
    
    private void PlacePlayer(string data)
    {
        string[] parsedData = data.Split('/');
        Vector3 playerPosition = new Vector3(
            float.Parse(parsedData[0], CultureInfo.InvariantCulture), 
            float.Parse(parsedData[1], CultureInfo.InvariantCulture),
            float.Parse(parsedData[2], CultureInfo.InvariantCulture));

        playerTransform.position = ToLocalSpace(playerPosition);
    }
    
    private void PlaceSentinel(string data)
    {
        string[] parsedData = data.Split('/');
        Vector3 sentinelPosition = new Vector3(
            float.Parse(parsedData[0], CultureInfo.InvariantCulture), 
            float.Parse(parsedData[1], CultureInfo.InvariantCulture),
            float.Parse(parsedData[2], CultureInfo.InvariantCulture));

        sentinelTransform.position = ToLocalSpace(sentinelPosition);
    }
    
    private void ReceiveGameState(string gameState)
    {
        m_CurrentGameState = (GameState)int.Parse(gameState);
    }

    private void OnGameStateChanged()
    {
        if (m_CurrentGameState == GameState.Failed)
        {
            SceneManager.LoadScene(0);
        }
        
        if (m_CurrentGameState != GameState.Failed && 
            m_CurrentGameState != GameState.Succeeded && 
            m_CurrentGameState != GameState.Waiting)
        {
            m_ActivePuzzle.SetActive(false);
            m_ActivePuzzle = m_PuzzlesList[Convert.ToInt32(m_CurrentGameState)];
            m_ActivePuzzle.SetActive(true);
        }
    }
}
