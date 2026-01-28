using System;
using UnityEngine;

public class ChestScript : MonoBehaviour
{
    private Animator chestAnimator;

    private void Start()
    {
        chestAnimator = GetComponent<Animator>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player is looking at chest");
            chestAnimator.SetBool("Open", true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player is looking away from chest");
            chestAnimator.SetBool("Open", false);
        }
    }
}
