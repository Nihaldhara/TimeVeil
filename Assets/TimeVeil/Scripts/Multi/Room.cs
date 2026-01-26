using System.Collections.Generic;
using Meta.XR.MRUtilityKit;
using NUnit.Framework;
using UnityEngine;

public class Room : MonoBehaviour
{
    [SerializeField] private GameObject m_WallPrefab;
    
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
        List<MRUKAnchor> walls = room.WallAnchors;

        foreach (var wall in walls)
        {
            Vector3 wallSize = new Vector3(wall.PlaneRect.Value.size.x, wall.PlaneRect.Value.size.y, 0.1f);
            m_WallPrefab.transform.localScale = wallSize;
            GameObject newWall = Instantiate(m_WallPrefab, wall.transform.position, wall.transform.rotation);
            Debug.Log("[Room Testing]" + wall.PlaneRect.Value.size);
        }
    }
}
