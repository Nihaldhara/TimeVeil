using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public class PuzzleInfoSender : MonoBehaviour
{
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
    
    public void SendPuzzlesData()
    {
        Transform parentTransform = transform.parent;
        
        Debug.Log("No puzzle...");
        foreach (var puzzle in m_PuzzlesList)
        {
            Debug.Log("Puzzle!!!!!!!!!!!!!");
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
