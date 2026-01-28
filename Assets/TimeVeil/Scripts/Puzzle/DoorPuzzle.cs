using System;
using UnityEngine;

public class DoorPuzzle : Puzzle
{
    private Animator m_DoorAnimator;
    
    protected override void TriggerPuzzle()
    {
        m_DoorAnimator = GetComponentInChildren<Animator>();
        m_DoorAnimator.SetTrigger("Open");
    }
}
