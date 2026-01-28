using System.Globalization;
using UnityEngine;

public class PuzzleInfoReceiver : MonoBehaviour
{
    [SerializeField] private GameObject puzzlePrefab;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ClientGameManager.Instance.PuzzleInfoReceiveEvent.AddListener(InitializePuzzles);
    }

    private void InitializePuzzles(string data)
    {
        Debug.Log(data);
        string[] parsedData = data.Split('/');
        Vector3 position = new Vector3(
            float.Parse(parsedData[0], CultureInfo.InvariantCulture), 
            float.Parse(parsedData[1], CultureInfo.InvariantCulture), 
            float.Parse(parsedData[2], CultureInfo.InvariantCulture));
        string clue = parsedData[3];
        
        GameObject newPuzzle = Instantiate(puzzlePrefab, position, Quaternion.Euler(0,0,0));
        newPuzzle.GetComponent<ComputerPuzzle>().Clue = clue;
    }
}
