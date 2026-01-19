using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ObjectSpawner : MonoBehaviour
{
    /// <summary>
    /// Reference prefab use to spawn an object on hit pose
    /// </summary>
    [SerializeField] private GameObject m_AnchorPrefab;

    private static List<GameObject> m_GameObjectsPlaced;
    
    public static ObjectSpawner Instance { get; private set; }

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
        m_GameObjectsPlaced = new List<GameObject>();
        
        Debug.Log($"[My Debug] ObjectSpawner.Start() called in scene: {UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}");
        LoadAnchorsByUuid(LoadAnchorsUuids());
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
                        var spatialAnchor = new GameObject($"[My Debug] Anchor {unboundAnchor.Uuid}")
                            .AddComponent<OVRSpatialAnchor>();
                        m_GameObjectsPlaced.Add(spatialAnchor.gameObject);

                        unboundAnchor.BindTo(spatialAnchor);

                        GameObject gameObjectAnchor = Instantiate(m_AnchorPrefab, spatialAnchor.transform);
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

    public static void ClearObjects()
    {
        foreach (var gameObject in m_GameObjectsPlaced)
        {
            Destroy(gameObject);
        }
        m_GameObjectsPlaced.Clear();
    }
}
