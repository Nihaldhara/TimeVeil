using System;
using System.Collections.Generic;
using System.Globalization;
using Meta.XR.MRUtilityKit;
using NUnit.Framework;
using UnityEngine;

public class WallTransformSender : MonoBehaviour
{ 
    [SerializeField] private GameObject m_WallPrefab;

    private string data = "";

    private DataSerializer m_DataSerializer;

    private GameManager m_GameManager;
    
    void Start()
    {
        m_DataSerializer = DataSerializer.Instance;
        m_GameManager = GameManager.Instance;
        
        // Subscribe to room creation
        MRUK.Instance.RegisterSceneLoadedCallback(OnSceneLoaded);
        //MRUK.Instance.RegisterSceneLoadedCallback(SendRoomData);

        // If room already exists
        if (MRUK.Instance.GetCurrentRoom() != null)
        {
            //m_GameManager.ClientConnectedEvent.AddListener(SendRoomData);
            OnSceneLoaded();
        }
    }

    void OnSceneLoaded()
    {
        Invoke(nameof(SendRoomData), 10f);
    }

    void SendRoomData()
    {
        MRUKRoom room = MRUK.Instance.GetCurrentRoom();
        List<MRUKAnchor> walls = room.WallAnchors;

        foreach (var wall in walls)
        {
            Vector3 wallSize = new Vector3(wall.PlaneRect.Value.size.x, wall.PlaneRect.Value.size.y, 0.1f);
            m_WallPrefab.transform.localScale = wallSize;
            string positionData = m_DataSerializer.SerializeFloat(wall.transform.position.x) + "/" +
                                  m_DataSerializer.SerializeFloat(wall.transform.position.y) + "/" +
                                  m_DataSerializer.SerializeFloat(wall.transform.position.z) + "/";
            string rotationData = m_DataSerializer.SerializeFloat(wall.transform.rotation.x) + "/" +
                                  m_DataSerializer.SerializeFloat(wall.transform.rotation.y) + "/" +
                                  m_DataSerializer.SerializeFloat(wall.transform.rotation.z) + "/" +
                                  m_DataSerializer.SerializeFloat(wall.transform.rotation.w) + "/";
            string scaleData = m_DataSerializer.SerializeFloat(wall.PlaneRect.Value.size.x) + "/" +
                               m_DataSerializer.SerializeFloat(wall.PlaneRect.Value.size.y) + "/0.1/";
            data = (int)DataLabel.WallTransform + "|" + positionData + rotationData + scaleData;
            
            ServerGameManager.Instance.DataReliableSendEvent.Invoke(data, 0);
        }
    }
}