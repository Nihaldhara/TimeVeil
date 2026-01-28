using System;
using System.Globalization;
using UnityEngine;

public class PlayerPositioning : MonoBehaviour
{
    [SerializeField] private Transform playerTransform;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ClientGameManager.Instance.PlayerPositionReceiveEvent.AddListener(PlacePlayer);
    }

    private void PlacePlayer(string data)
    {
        Debug.Log(data);
        string[] parsedData = data.Split('/');
        Vector3 playerPosition = new Vector3(
            float.Parse(parsedData[0], CultureInfo.InvariantCulture), 
            float.Parse(parsedData[1], CultureInfo.InvariantCulture),
            float.Parse(parsedData[2], CultureInfo.InvariantCulture));

        playerTransform.position = playerPosition;
    }
}
