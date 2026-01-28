using System.Collections.Generic;
using Meta.XR.MRUtilityKit;
using Unity.VisualScripting;
using UnityEngine;

public class PathGridMRUKSetup : MonoBehaviour
{
    [SerializeField] private PathGrid m_PathGrid;
    
    void Start()
    {
        // Subscribe to room creation
        MRUK.Instance.RegisterSceneLoadedCallback(OnSceneLoaded);
        
        // If room already exists
        if (MRUK.Instance.GetCurrentRoom() != null)
        {
            OnSceneLoaded();
        }
    }
    
    void OnSceneLoaded()
    {
        MRUKRoom room = MRUK.Instance.GetCurrentRoom();
        
        MRUKAnchor floor = room.FloorAnchor;

        if (room == null)
        {
            Debug.Log("[My Debug] No room detected");
        }
        
        if (floor != null)
        {
            // Get floor bounds
            Vector2 floorSize = floor.PlaneRect.Value.size;
            
            // Add a box collider if none exists
            if (!floor.GetComponent<Collider>())
            {
                floor.AddComponent<BoxCollider>();
            }
            
            // Position grid at floor center
            m_PathGrid.transform.position = floor.transform.position;
            float yRotation = floor.transform.eulerAngles.y;
            m_PathGrid.transform.rotation = Quaternion.Euler(0, yRotation, 0);
            
            m_PathGrid.SetGridWorldSize(new Vector3(floorSize.x, 1.0f, floorSize.y));
            m_PathGrid.transform.parent = room.transform;
            
            m_PathGrid.Initialize();
        }
    }
}