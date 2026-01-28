using System;
using System.Globalization;
using UnityEngine;

public class PositionTracker : MonoBehaviour
{
    private float timer = 0f;
    private float sendInterval = 0.1f;

    private void Update()
    {
        timer += Time.deltaTime;
    
        if (timer >= sendInterval)
        {
            timer = 0f;
            SendPlayerPosition();
        }
    }

    void SendPlayerPosition()
    {
        Transform parentTransform = transform.parent;
        
        if (parentTransform != null)
        {
            string playerPosition = SerializeFloat(parentTransform.position.x) + "/" +
                                    SerializeFloat(parentTransform.position.y) + "/" +
                                    SerializeFloat(parentTransform.position.z);
        
            string identifier = (int)DataLabel.PlayerPosition + "|";

            if (transform.parent.CompareTag("Sentinel"))
            {
                identifier = (int)DataLabel.SentinelPosition + "|";
            }
            
            string data = identifier + playerPosition;
        
            ServerGameManager.Instance.DataUnreliableSendEvent.Invoke(data, 0);
        }
    }
    
    public string SerializeFloat(float newData)
    {
        return Math.Round(newData, 5).ToString(CultureInfo.InvariantCulture);
    }
}