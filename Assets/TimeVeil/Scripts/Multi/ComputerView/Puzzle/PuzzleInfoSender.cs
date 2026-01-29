using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public class PuzzleInfoSender : MonoBehaviour
{
    private GameManager m_GameManager;
    
    private List<GameObject> m_PuzzlesList;

    public List<GameObject> PuzzlesList
    {
        get => m_PuzzlesList;
        set
        {
            m_PuzzlesList = value;
            Debug.Log("Puzzles : " + m_PuzzlesList.Count);
        }
    }

    private string data = "";
    
    public static PuzzleInfoSender Instance { get; private set; }

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
        m_GameManager = GameManager.Instance;
        
        m_GameManager.ClientConnectedEvent.AddListener(SendPuzzlesData);
    }

    public void SendPuzzlesData()
    {
        Transform parentTransform = transform.parent;
        
        Debug.Log("PuzzleInfoSender: Sending puzzle data");
        foreach (var puzzle in m_PuzzlesList)
        {
            string positionData = SerializeFloat(parentTransform.position.x) + "/" +
                                  SerializeFloat(parentTransform.position.y) + "/" +
                                  SerializeFloat(parentTransform.position.z) + "/";
            string clueData = puzzle.GetComponent<Puzzle>().Clue;
            
            data = (int)DataLabel.PuzzleInfo + "|" + positionData + clueData;
            Debug.Log(data);
            
            ServerGameManager.Instance.DataReliableSendEvent.Invoke(data, 0);
        }
    }
    
    public string SerializeFloat(float newData)
    {
        return Math.Round(newData, 5).ToString(CultureInfo.InvariantCulture);
    }
}
