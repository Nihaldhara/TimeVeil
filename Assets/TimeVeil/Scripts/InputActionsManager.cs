using UnityEngine;

/// <summary>
/// Manage the current Input Actions
/// </summary>
public class InputActionsManager : MonoBehaviour
{
    /// <summary>
    /// Instance of the Meta Player Input Actions
    /// </summary>
    static private MetaPlayerInputActions m_InstanceInputActions;

    /// <summary>
    /// Get Instance of the Meta Player Input Actions
    /// </summary>
    static public MetaPlayerInputActions InstanceInputActions => m_InstanceInputActions;

    private void Awake()
    {
        // Instanciate Input Action
        m_InstanceInputActions = new MetaPlayerInputActions();
    }

    void Start()
    {
        // Enable Input Action
        m_InstanceInputActions.Enable();
    }
}
