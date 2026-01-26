using System;
using System.Collections.Generic;
using Oculus.Interaction;
using UnityEngine;

public class Puzzle : MonoBehaviour
{
    private GameObject m_Key;

    public GameObject Key
    {
        get => m_Key;
        set
        {
            m_Key = value;
            Debug.Log("[My Debug] A key was assigned to this puzzle: " + this);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == m_Key && other.gameObject.GetComponent<Key>().Grabbed)
        {
            Debug.Log("[My Debug] You found the correct key !");
            PuzzleManager.Instance.FinishPuzzle(gameObject);
            other.gameObject.SetActive(false);
            gameObject.SetActive(false);
        }
        
        /*if (other.CompareTag("Player"))
        {
            List<GameObject> playerKeys = other.GetComponent<PlayerController>().GetCollectedKeys();
            if (playerKeys.Contains(m_Key))
            {
                Debug.Log("[My Debug] You found the correct key !");
                other.GetComponent<PlayerController>().ConsumeKey(m_Key);
                PuzzleManager.Instance.FinishPuzzle(gameObject);
                gameObject.SetActive(false);
            }
            else
            {
                Debug.Log("[My Debug] You don't have the right key...");
            }
        }*/
    }
}
