using UnityEngine;

public class CoffinPuzzle : Puzzle
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Bouquet"))
        {
            Debug.Log("[My Debug] You found the correct key !");
            other.gameObject.SetActive(false);
            Solved = true;
            TriggerPuzzle();
        }
    }

    protected override void TriggerPuzzle()
    {
        throw new System.NotImplementedException();
    }
}
