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
/// Enumeration to identify what type of object can by spawn
/// </summary>
public enum ObjectToSpawn
{
    Object,
    Anchor
};

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
    /// Current type of object that will spawn
    /// </summary>
    [SerializeField] private ObjectToSpawn m_ObjectToSpawn;

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

        // Bind Input on Place function
        m_InputActions.Player.Place.performed += Place;
        m_InputActions.Player.Clear.performed += ClearAllSavedAnchors;

        m_ObjectSpawner.LoadAnchorsByUuid(m_ObjectSpawner.LoadAnchorsUuids());
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
    /// Function use to place object or anchor
    /// </summary>
    /// <param name="callbackContext">Get the meta data of the input</param>
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

        StartCoroutine(CreateSpatialAnchor(hit));
    }

    /// <summary>
    /// Funciton use to create anchor
    /// </summary>
    /// <param name="_hit">Get the data of the current hit point</param>
    /// <returns></returns>
    IEnumerator CreateSpatialAnchor(RaycastHit _hit)
    {
        GameObject anchor = Instantiate(m_AnchorPrefab, _hit.point, Quaternion.LookRotation(_hit.normal));
        OVRSpatialAnchor ovrSpatialAnchor = anchor.AddComponent<OVRSpatialAnchor>();

        // Wait for the async creation
        yield return new WaitUntil(() => ovrSpatialAnchor.Created);

        Debug.Log($"[My Debug] Created anchor {ovrSpatialAnchor.Uuid}");

        anchor.name = $"Anchor : {ovrSpatialAnchor.Uuid}";

        OnSaveButtonPressed(ovrSpatialAnchor);
    }

    // Missing part to save and load anchor+

    public async void OnSaveButtonPressed(OVRSpatialAnchor anchor)
    {
        var result = await anchor.SaveAnchorAsync();

        if (result.Success)
        {
            Debug.Log($"[My Debug] Anchor {anchor.Uuid} saved successfully.");
        }
        else
        {
            Debug.LogError($"[My Debug] Anchor {anchor.Uuid} failed to save with error {result.Status}");
        }

        if (PlayerPrefs.GetString("SavedAnchors") != null)
        {
            string savedAnchors = PlayerPrefs.GetString("SavedAnchors");
            PlayerPrefs.SetString("SavedAnchors", savedAnchors + anchor.Uuid + ",");
        }
        else
        {
            PlayerPrefs.SetString("SavedAnchors", anchor.Uuid.ToString());
        }
    }

    /*// This reusable buffer helps reduce pressure on the garbage collector
    List<OVRSpatialAnchor.UnboundAnchor> _unboundAnchors = new();

    public IEnumerable<Guid> LoadAnchorsUuids()
    {
        string rawUuids = PlayerPrefs.GetString("SavedAnchors");
        if (string.IsNullOrEmpty(rawUuids))
            return new List<Guid>();

        string[] separatedRawUuids = rawUuids.Split(",", StringSplitOptions.RemoveEmptyEntries);

        IEnumerable<Guid> anchorsUuids = new List<Guid>();
        foreach (var rawUuid in separatedRawUuids)
        {
            Guid uuid = Guid.Parse(rawUuid);
            anchorsUuids = anchorsUuids.Append(uuid);
        }

        return anchorsUuids;
    }

    public async void LoadAnchorsByUuid(IEnumerable<Guid> uuids)
    {
        // Step 1: Load
        var result = await OVRSpatialAnchor.LoadUnboundAnchorsAsync(uuids, _unboundAnchors);

        if (result.Success)
        {
            Debug.Log($"[My Debug] Anchors loaded successfully.");

            // Note result.Value is the same as _unboundAnchors
            foreach (var unboundAnchor in result.Value)
            {
                // Step 2: Localize
                unboundAnchor.LocalizeAsync().ContinueWith((success, anchor) =>
                {
                    if (success)
                    {
                        // Create a new game object with an OVRSpatialAnchor component
                        var spatialAnchor = new GameObject($"[My Debug] Anchor {unboundAnchor.Uuid}")
                            .AddComponent<OVRSpatialAnchor>();
                        GameObject gameObjectAnchor = Instantiate(m_AnchorPrefab, spatialAnchor.transform);

                        // Step 3: Bind
                        // Because the anchor has already been localized, BindTo will set the
                        // transform component immediately.
                        unboundAnchor.BindTo(spatialAnchor);
                    }
                    else
                    {
                        Debug.LogError($"[My Debug] Localization failed for anchor {unboundAnchor.Uuid}");
                    }
                }, unboundAnchor);
            }
        }
        else
        {
            Debug.LogError($"Load failed with error {result.Status}.");
        }
    }*/

    private void ClearAllSavedAnchors(InputAction.CallbackContext callbackContext)
    {
        PlayerPrefs.DeleteKey("SavedAnchors");
        Debug.Log("[My Debug] All saved anchors cleared");
    }
}