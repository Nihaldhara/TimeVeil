using UnityEngine;

public class RoomReference : MonoBehaviour
{
    public static RoomReference Instance { get; private set; }
    public Transform ReferenceAnchor { get; private set; }
    
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    
    public void SetReference(Transform anchor)
    {
        ReferenceAnchor = anchor;
        Debug.Log($"RoomReference: Set reference anchor at {anchor.position}");
    }
    
    /// <summary>
    /// Converts world position to top-down relative position.
    /// VR (X, Y, Z) -> Top-down (X, Z, Y)
    /// </summary>
    public Vector3 GetRelativePosition(Vector3 worldPos)
    {
        if (ReferenceAnchor == null) 
        {
            Debug.LogWarning("RoomReference: No reference anchor set!");
            return worldPos;
        }
        
        // Get position relative to floor anchor
        Vector3 relativePos = ReferenceAnchor.InverseTransformPoint(worldPos);
        
        // Remap for top-down view: X stays, Z becomes Y, Y becomes Z (height, mostly ignored)
        return new Vector3(relativePos.x, relativePos.z, relativePos.y);
    }
    
    /// <summary>
    /// Converts world rotation to top-down relative rotation.
    /// Only preserves yaw (Y-axis rotation) mapped to Z-axis rotation for top-down.
    /// </summary>
    public Quaternion GetRelativeRotation(Quaternion worldRot)
    {
        if (ReferenceAnchor == null) 
        {
            Debug.LogWarning("RoomReference: No reference anchor set!");
            return worldRot;
        }
        
        // Get rotation relative to floor anchor
        Quaternion relativeRot = Quaternion.Inverse(ReferenceAnchor.rotation) * worldRot;
        
        // Extract euler angles and remap for top-down
        // Y-axis rotation (yaw) becomes Z-axis rotation when viewed from above
        Vector3 euler = relativeRot.eulerAngles;
        return Quaternion.Euler(0, 0, -euler.y);
    }
    
    /// <summary>
    /// Simplified position for entities that only need X/Y on the top-down plane (player, sentinel).
    /// Returns a Vector2 for convenience.
    /// </summary>
    public Vector2 GetRelativePosition2D(Vector3 worldPos)
    {
        Vector3 pos3D = GetRelativePosition(worldPos);
        return new Vector2(pos3D.x, pos3D.y);
    }
}