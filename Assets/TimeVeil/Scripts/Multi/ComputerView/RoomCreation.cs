using System.Globalization;
using UnityEngine;

public class RoomCreation : MonoBehaviour
{
    [SerializeField] private GameObject wallPrefab;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ClientGameManager.Instance.WallTransformReceiveEvent.AddListener(InitializeRoom);
    }

    private void InitializeRoom(string data)
    {
        Debug.Log(data);
        string[] parsedData = data.Split('/');
        Vector3 position = new Vector3(
            float.Parse(parsedData[0], CultureInfo.InvariantCulture), 
            float.Parse(parsedData[1], CultureInfo.InvariantCulture), 
            float.Parse(parsedData[2], CultureInfo.InvariantCulture));
        Quaternion rotation = new Quaternion(
            float.Parse(parsedData[3], CultureInfo.InvariantCulture), 
            float.Parse(parsedData[4], CultureInfo.InvariantCulture), 
            float.Parse(parsedData[5], CultureInfo.InvariantCulture),
            float.Parse(parsedData[6], CultureInfo.InvariantCulture));
        Vector3 scale = new Vector3(
            float.Parse(parsedData[7], CultureInfo.InvariantCulture), 
            float.Parse(parsedData[8], CultureInfo.InvariantCulture), 
            float.Parse(parsedData[9], CultureInfo.InvariantCulture));
        Debug.Log(position.ToString());
        Debug.Log(rotation.ToString());
        Debug.Log(scale.ToString());
        wallPrefab.transform.localScale = scale;
        GameObject newWall = Instantiate(wallPrefab, position, rotation);
    }
}
