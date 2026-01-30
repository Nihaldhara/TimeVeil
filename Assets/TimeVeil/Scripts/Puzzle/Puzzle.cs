using System;
using System.Collections.Generic;
using Oculus.Interaction;
using UnityEngine;

public abstract class Puzzle : MonoBehaviour
{
    private string m_Clue;

    public string Clue
    {
        get => m_Clue;
        set => m_Clue = value;
    }

    public bool Solved = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("OldDoorKey"))
        {
            Debug.Log("[My Debug] You found the correct key !");
            other.gameObject.SetActive(false);
            Solved = true;
            TriggerPuzzle();
        }
    }

    protected abstract void TriggerPuzzle();
}
