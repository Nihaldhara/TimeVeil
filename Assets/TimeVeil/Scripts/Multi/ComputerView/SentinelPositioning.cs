using System.Globalization;
using UnityEngine;

public class SentinelPositioning : MonoBehaviour
{
    [SerializeField] private Transform sentinelTransform;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ClientGameManager.Instance.SentinelPositionReceiveEvent.AddListener(PlaceSentinel);
    }

    private void PlaceSentinel(string data)
    {
        Debug.Log(data);
        string[] parsedData = data.Split('/');
        Vector3 playerPosition = new Vector3(
            float.Parse(parsedData[0], CultureInfo.InvariantCulture), 
            float.Parse(parsedData[1], CultureInfo.InvariantCulture),
            float.Parse(parsedData[2], CultureInfo.InvariantCulture));

        sentinelTransform.position = playerPosition;
    }
}
