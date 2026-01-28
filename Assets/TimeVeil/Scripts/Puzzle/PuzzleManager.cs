using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;

public class PuzzleManager : MonoBehaviour
{
    [Header("UI")]
    //[SerializeField] private TMP_Text m_TimerDisplay;
    //[SerializeField] private TMP_Text m_WonDisplay;

    [Header("Configuration")]
    [SerializeField] private float m_GameTime = 300.0f;
    
    [SerializeField] private List<GameObject> m_Puzzles;
    [SerializeField] private List<GameObject> m_Sentinels;

    [SerializeField] private GameObject m_Player;

    private float m_RemainingTime;
    private bool m_GameWon = false;

    public bool checkFirstPuzzleState = false;

    public static PuzzleManager Instance { get; private set; }

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
    
    void Start()
    {
        m_RemainingTime = m_GameTime;
    }

    void Update()
    {
        if (m_RemainingTime >= 0)
        {
            if (m_GameWon)
            {
                /*m_WonDisplay.gameObject.SetActive(true);
                m_TimerDisplay.gameObject.SetActive(false);*/
            }
            else
            {
                m_RemainingTime -= Time.deltaTime;
                
                /*float minutes = Mathf.Floor(m_RemainingTime / 60);
                float seconds = m_RemainingTime % 60;
                string niceTime = string.Format("{0:0}:{1:00}", minutes, seconds);
                
                m_TimerDisplay.text = niceTime;*/
            }
        }
        else
        {
            m_Player.gameObject.GetComponent<PlayerController>().Die();
        }
        
        /*if (m_Puzzles.Count <= 0)
        {
            Debug.Log("Congrats, you solved all the puzzles !");
            m_GameWon = true; 
            foreach (var sentinel in m_Sentinels)
            {
                sentinel.SetActive(false);
            }
        }*/
    }

    public void FinishPuzzle(GameObject puzzle)
    {
        checkFirstPuzzleState = true;
        
        if (m_Puzzles.Contains(puzzle))
        {
            m_Puzzles.Remove(puzzle);
        }
    }
}
