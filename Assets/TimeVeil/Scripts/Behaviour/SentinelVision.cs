using System;
using UnityEngine;
using UnityEngine.Events;

public class SentinelVision : MonoBehaviour
{
    private GameManager m_GameManager;

    private void Start()
    {
        m_GameManager = GameManager.Instance;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            m_GameManager.PlayerEnteredTriggerEvent.Invoke();
        }
    }
}
