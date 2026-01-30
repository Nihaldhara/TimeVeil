using System;
using UnityEngine;

public class DoorPuzzle : Puzzle
{
    private Animator m_DoorAnimator;
    
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
    
    protected override void TriggerPuzzle()
    {
        m_DoorAnimator = GetComponentInChildren<Animator>();
        m_DoorAnimator.SetTrigger("Open");
    }
}
