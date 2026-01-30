using UnityEngine;

public class ThronePuzzle : Puzzle
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Crown"))
        {
            Debug.Log("[My Debug] You found the correct key !");
            other.gameObject.SetActive(false);
            Solved = true;
            TriggerPuzzle();
        }
    }
    
    protected override void TriggerPuzzle()
    {
        
    }
}
