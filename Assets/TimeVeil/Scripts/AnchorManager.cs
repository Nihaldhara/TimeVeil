using Meta.XR;
using Meta.XR.MRUtilityKit;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

/// <summary>
/// Enumeration to identify what type of raycast use to find a hit
/// </summary>
public enum RaycastType
{
    LabelFilter,
    DepthEnv
};

/// <summary>
/// Class you can use when you need to instantiate object when you it MRUK Anchor
/// </summary>
public class AnchorManager : MonoBehaviour
{
    /// <summary>
    /// Current type of raycast use to find a hit
    /// </summary>
    [SerializeField] private RaycastType m_RaycastType;

    /// <summary>
    /// Reference prefab use to spawn an object on hit pose
    /// </summary>
    [SerializeField] private GameObject m_AnchorPrefab;

    /// <summary>
    /// Instance of the instantiate prefab when detect hit pose
    /// </summary>
    private GameObject m_Object;

    /// <summary>
    /// Instance of raycast manager use for the Depth Env raycast
    /// </summary>
    private EnvironmentRaycastManager m_RayCastManager;

    /// <summary>
    /// Hand reference for origin of raycast
    /// </summary>
    [SerializeField] private Transform m_ControllerTarget;

    /// <summary>
    /// Visual ray for the raycast
    /// </summary>
    [SerializeField] private LineRenderer m_LineRenderer;

    /// <summary>
    /// Label Filter use as a layer for the raycast
    /// </summary>
    [SerializeField] private MRUKAnchor.SceneLabels m_LabelFilter;

    /// <summary>
    /// Instance of Input Actions of the Meta Player
    /// </summary>
    private MetaPlayerInputActions m_InputActions;

    private ObjectSpawner m_ObjectSpawner;

    private OVRSpatialAnchor m_PendingSpatialAnchor;
    private GameObject m_PendingGameObject;

    private List<GameObject> m_CreatedGameObjects;

    public static AnchorManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        m_InputActions = InputActionsManager.InstanceInputActions;

        m_RayCastManager = FindFirstObjectByType<EnvironmentRaycastManager>();

        m_ObjectSpawner = ObjectSpawner.Instance;

        m_PendingSpatialAnchor = null;
        m_PendingGameObject = null;
        m_CreatedGameObjects = new List<GameObject>();

        // Bind Input on Place function
        m_InputActions.Player.Place.performed += Place;
        m_InputActions.Player.Save.performed += OnSaveButtonPressed;
        m_InputActions.Player.Cancel.performed += OnCancelButtonPressed;
        m_InputActions.Player.Clear.performed += ClearAllSavedAnchors;
    }

    void Update()
    {
        DisplayRaycast();
    }

    /// <summary>
    /// Function to display the current raycast
    /// </summary>
    private void DisplayRaycast()
    {
        Vector3 start = m_ControllerTarget.position;
        Vector3 end = start + m_ControllerTarget.forward.normalized * 100;

        m_LineRenderer.SetPosition(0, start);
        m_LineRenderer.SetPosition(1, end);
    }

    /// <summary>
    /// Function use to place anchor
    /// </summary>
    /// <param name="callbackContext">Get the metadata of the input</param>
    private void Place(InputAction.CallbackContext callbackContext)
    {
        Debug.Log("[My Debug] Trigger Pressed");

        MRUKAnchor trackable = null;

        RaycastHit hit = new RaycastHit();

        // Create the initial Ray
        Ray ray = new Ray(m_ControllerTarget.position, m_ControllerTarget.forward);

        // In case of LabelFilter use
        if (m_RaycastType == RaycastType.LabelFilter)
        {
            if (!Physics.Raycast(ray, out hit))
            {
                Debug.Log("[My Debug] No Raycast Found");
                return;
            }

            // Check if the ray hit MRUKAnchor
            trackable = hit.transform.gameObject.GetComponentInParent<MRUKAnchor>();

            if (trackable == null)
            {
                Debug.Log("[My Debug] No MRUK Object Hit");
                return;
            }

            if (!new LabelFilter(m_LabelFilter).PassesFilter(trackable.Label))
            {
                Debug.Log("[My Debug] Object encountered doesn't have right filter");
                return;
            }

            Debug.Log("[My Debug] MRUK Object Hit : " + trackable.Label);

            Debug.Log("[My Debug] Try to create Anchor");
        }
        // In case of Depth Env use
        else if (m_RaycastType == RaycastType.DepthEnv)
        {
            if (!m_RayCastManager.Raycast(ray, out EnvironmentRaycastHit _hit) ||
                _hit.status == EnvironmentRaycastHitStatus.Hit)
                return;

            hit.point = _hit.point;
            hit.normal = _hit.normal;
        }

        CreateSpatialAnchor(hit);
    }

    /// <summary>
    /// Function use to create anchor
    /// </summary>
    /// <param name="_hit">Get the data of the current hit point</param>
    /// <returns></returns>
    private void CreateSpatialAnchor(RaycastHit _hit)
    {
        if (m_PendingSpatialAnchor == null)
        {
            GameObject anchor = Instantiate(m_AnchorPrefab, _hit.point, Quaternion.LookRotation(_hit.normal));
            OVRSpatialAnchor ovrSpatialAnchor = anchor.AddComponent<OVRSpatialAnchor>();

            // Wait for the async creation
            new WaitUntil(() => ovrSpatialAnchor.Created);

            Debug.Log($"[My Debug] Created anchor {ovrSpatialAnchor.Uuid}");

            anchor.name = $"Anchor : {ovrSpatialAnchor.Uuid}";

            m_PendingSpatialAnchor = ovrSpatialAnchor;
            m_PendingGameObject = anchor;
        }
        else
        {
            Debug.Log("[My Debug] You already have an unsaved anchor pending - save this one before creating another");
        }
    }

    private async void OnSaveButtonPressed(InputAction.CallbackContext callbackContext)
    {
        OVRSpatialAnchor anchor = m_PendingSpatialAnchor;

        if (anchor != null)
        {
            var result = await anchor.SaveAnchorAsync();

            if (result.Success)
            {
                if (PlayerPrefs.GetString("SavedAnchors") != string.Empty)
                {
                    string savedAnchors = PlayerPrefs.GetString("SavedAnchors");
                    PlayerPrefs.SetString("SavedAnchors", savedAnchors + anchor.Uuid + ",");
                }
                else
                {
                    PlayerPrefs.SetString("SavedAnchors", anchor.Uuid + ",");
                }
                Debug.Log($"[My Debug] Anchor {anchor.Uuid} saved successfully.");
                m_CreatedGameObjects.Add(anchor.gameObject);
                m_PendingSpatialAnchor = null;
            }
            else
            {
                Debug.LogError($"[My Debug] Anchor {anchor.Uuid} failed to save with error {result.Status}");
            }
        }
        else
        {
            Debug.Log("[My Debug] No anchor pending to be saved");
        }
    }

    private void OnCancelButtonPressed(InputAction.CallbackContext callbackContext)
    {
        if (m_PendingSpatialAnchor != null)
        {
            Destroy(m_PendingSpatialAnchor);
            m_PendingSpatialAnchor = null;
            Destroy(m_PendingGameObject);
            Debug.Log("[My Debug] Cancelled anchor creation for last placed anchor");
        }
        else
        {
            Debug.Log("[My Debug] You don't have an anchor waiting to be saved");
        }
    }

    private void ClearAllSavedAnchors(InputAction.CallbackContext callbackContext)
    {
        PlayerPrefs.DeleteKey("SavedAnchors");
        foreach (var gameObject in m_CreatedGameObjects)
        {
            Destroy(gameObject);
        }
        ObjectSpawner.ClearObjects();
        Debug.Log("[My Debug] All saved anchors cleared");
    }
}