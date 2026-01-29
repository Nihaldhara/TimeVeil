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

    private bool m_RoomReady = false;
    private bool m_ClientConnected = false;
    
    void Start()
    {
        m_DataSerializer = DataSerializer.Instance;
        m_GameManager = GameManager.Instance;
    
        m_GameManager.ClientConnectedEvent.AddListener(OnClientConnected);
    
        // Always register the callback
        MRUK.Instance.RegisterSceneLoadedCallback(OnRoomLoaded);
    
        // But ALSO check if room already exists (callback might have already fired)
        if (MRUK.Instance.GetCurrentRoom() != null)
        {
            Debug.Log("WallTransformSender: Room already exists at Start");
            m_RoomReady = true;
        }
        else
        {
            Debug.Log("WallTransformSender: No room yet, waiting for callback");
        }
    }
    
    void Update()
    {
        if (!m_RoomReady)
        {
            var room = MRUK.Instance?.GetCurrentRoom();
            if (room != null)
            {
                m_RoomReady = true;
                TrySendRoomData();
            }
        }
    }

    void OnRoomLoaded()
    {
        Debug.Log("WallTransformSender: Room loaded");
        m_RoomReady = true;
        TrySendRoomData();
    }

    void OnClientConnected()
    {
        Debug.Log("WallTransformSender: Client connected");
        m_ClientConnected = true;
        TrySendRoomData();
    }

    void TrySendRoomData()
    {
        if (!m_RoomReady || !m_ClientConnected)
        {
            Debug.Log($"WallTransformSender: Not ready yet. Room: {m_RoomReady}, Client: {m_ClientConnected}");
            return;
        }

        MRUKRoom room = MRUK.Instance.GetCurrentRoom();
        List<MRUKAnchor> walls = room.WallAnchors;

        Debug.Log($"WallTransformSender: Sending {walls.Count} walls");
        
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
            
            Debug.Log("WallTransformSender: Sending " + data);
            
            ServerGameManager.Instance.DataReliableSendEvent.Invoke(data, 0);
        }
    }
}