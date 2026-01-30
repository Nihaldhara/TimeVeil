using Meta.XR;
using Meta.XR.MRUtilityKit;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public enum AnchorType
{
    Puzzle1,
    Static,
    Sentinel,
    Target
}

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
    //[SerializeField] private AnchorType m_AnchorType;
    private int m_AnchorTypeIndex = 0;

    /// <summary>
    /// Current type of raycast use to find a hit
    /// </summary>
    [SerializeField] private RaycastType m_RaycastType;

    /// <summary>
    /// Reference prefab use to spawn an object on hit pose
    /// </summary>
    [SerializeField] private GameObject m_AnchorPrefab;

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

    private OVRSpatialAnchor m_PendingSpatialAnchor;
    private GameObject m_PendingGameObject;

    private List<GameObject> m_CreatedAnchors;

    [SerializeField] private List<GameObject> m_typesList;
    private GameObject m_SelectedType;

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

        m_PendingSpatialAnchor = null;
        m_PendingGameObject = null;
        m_CreatedAnchors = new List<GameObject>();
        m_SelectedType = m_typesList[m_AnchorTypeIndex];

        // Bind Input on Place function
        m_InputActions.Player.Place.performed += Place;
        m_InputActions.Player.Save.performed += OnSaveButtonPressed;
        m_InputActions.Player.Cancel.performed += OnCancelButtonPressed;
        m_InputActions.Player.Clear.performed += ClearAllSavedAnchors;
        m_InputActions.Player.NextScene.performed += GoToNextScene;
        m_InputActions.Player.NextAnchorType.performed += NextAnchorType;

        LoadAnchorsByUuid(LoadAnchorsUuids());
    }

    void Update()
    {
        DisplayRaycast();
        m_SelectedType = m_typesList[m_AnchorTypeIndex];
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
            GameObject anchor;
            GameObject anchorObject;
            if (m_AnchorTypeIndex == Convert.ToInt32(AnchorType.Sentinel))
            {
                anchor = Instantiate(m_AnchorPrefab, _hit.point/* + new Vector3(0,0.4f,0)*/, Quaternion.LookRotation(_hit.point));
            }
            else
            {
                anchor = Instantiate(m_AnchorPrefab, _hit.point, Quaternion.LookRotation(-_hit.point));
            }
            
            anchorObject = Instantiate(m_SelectedType, _hit.point, Quaternion.LookRotation(-_hit.point));
            OVRSpatialAnchor ovrSpatialAnchor = anchor.AddComponent<OVRSpatialAnchor>();
            anchorObject.transform.parent = ovrSpatialAnchor.transform;

            // Wait for the async creation
            new WaitUntil(() => ovrSpatialAnchor.Created);

            Debug.Log($"[My Debug] Created anchor {ovrSpatialAnchor.Uuid}");

            anchor.name = $"Anchor : {ovrSpatialAnchor.Uuid}";

            m_PendingSpatialAnchor = ovrSpatialAnchor;
            m_PendingGameObject = anchor;
        }
        else
        {
            Debug.Log(
                "[My Debug] You already have an unsaved anchor pending - save the previous one before creating another");
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

                PlayerPrefs.SetInt(anchor.Uuid.ToString(), m_AnchorTypeIndex);

                Debug.Log($"[My Debug] Anchor {anchor.Uuid} saved successfully.");
                m_CreatedAnchors.Add(anchor.gameObject);
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

    // This reusable buffer helps reduce pressure on the garbage collector
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
                    Debug.Log($"[My Debug] Localization callback fired for {anchor.Uuid}, success: {success}");

                    if (success)
                    {
                        var anchorType = PlayerPrefs.GetInt(anchor.Uuid.ToString());
                        GameObject anchorGameObject = m_typesList[anchorType];

                        var spatialAnchor = new GameObject($"[My Debug] Anchor {unboundAnchor.Uuid}")
                            .AddComponent<OVRSpatialAnchor>();
                        m_CreatedAnchors.Add(spatialAnchor.gameObject);

                        unboundAnchor.BindTo(spatialAnchor);

                        GameObject anchorObject = Instantiate(anchorGameObject, spatialAnchor.transform);
                        anchorObject.transform.parent = spatialAnchor.transform;
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
    }

    private void ClearAllSavedAnchors(InputAction.CallbackContext callbackContext)
    {
        PlayerPrefs.DeleteKey("SavedAnchors");
        foreach (var gameObject in m_CreatedAnchors)
        {
            Destroy(gameObject);
        }

        Debug.Log("[My Debug] All saved anchors cleared");
    }

    private void GoToNextScene(InputAction.CallbackContext callbackContext)
    {
        SceneManager.LoadScene(1);
    }
    
    private void NextAnchorType(InputAction.CallbackContext callbackContext)
    {
        if (m_AnchorTypeIndex < m_typesList.Count - 1)
        {
            m_AnchorTypeIndex++;
        }
        else
        {
            m_AnchorTypeIndex = 0;
        }
    }
}