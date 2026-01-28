using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ComputerGameManager : MonoBehaviour
{
    [SerializeField] private float m_TotalTime = 60.0f;
    private float m_RemainingTime;

    [SerializeField] private TMP_Text m_TimerDisplay;
    
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
        Debug.Log("Your partner is lost forever in the 11th century, and will probably die at the hands of the Normands...");
    }
    
    private void RunTimer()
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
}
