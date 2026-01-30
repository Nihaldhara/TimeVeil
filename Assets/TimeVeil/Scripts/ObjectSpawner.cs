using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class ObjectSpawner : MonoBehaviour
{
    /// <summary>
    /// Reference prefab use to spawn an object on hit pose
    /// </summary>
    [SerializeField] private GameObject m_AnchorPrefab;

    [SerializeField] private List<GameObject> m_typesList;

    [SerializeField] private PathGrid m_PathGrid;
    
    void Start()
    {
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

            List<GameObject> puzzles = new List<GameObject>();
            List<GameObject> statics = await LoadAnchorsWithType(result.Value, AnchorType.Static);
            List<GameObject> chest = await LoadAnchorsWithType(result.Value, AnchorType.Chest);
            List<GameObject> crown = await LoadAnchorsWithType(result.Value, AnchorType.Crown);
            List<GameObject> bouquet = await LoadAnchorsWithType(result.Value, AnchorType.Flowers);
            List<GameObject> targets = await LoadAnchorsWithType(result.Value, AnchorType.Target);
            List<Transform> targetTransform = new List<Transform>();
            foreach (var target in targets)
            {
                targetTransform.Add(target.transform);
            }
            List<GameObject> puzzle1 = await LoadAnchorsWithType(result.Value, AnchorType.Door);
            puzzle1[0].GetComponent<Puzzle>().Clue = "William The Conqueror's mother was but a child of a modest tanner." +
                                                     "The young king was given birth to here, among the tools and works of his grandfather, between 1027 and 1028." +
                                                     "You could say that is where the KEY to Normandy's glory came to life...";
            puzzles.Add(puzzle1[0]);
            List<GameObject> puzzle2 = await LoadAnchorsWithType(result.Value, AnchorType.Throne);
            puzzle1[0].GetComponent<Puzzle>().Clue = "William The Conqueror's mother was but a child of a modest tanner." +
                                                     "The young king was given birth to here, among the tools and works of his grandfather" +
                                                     "You could say that is where the KEY to Normandy's glory came to life...";
            puzzles.Add(puzzle2[0]);
            List<GameObject> puzzle3 = await LoadAnchorsWithType(result.Value, AnchorType.Coffin);
            puzzle1[0].GetComponent<Puzzle>().Clue = "William The Conqueror's mother was but a child of a modest tanner." +
                                                     "The young king was given birth to here, among the tools and works of his grandfather." +
                                                     "You could say that is where the KEY to Normandy's glory came to life...";
            puzzles.Add(puzzle3[0]);

            PuzzleInfoSender.Instance.PuzzlesList = puzzles;
            GameManager.Instance.PuzzlesList = puzzles;
            PuzzleInfoSender.Instance.SendPuzzlesData();
            List<GameObject> sentinels = await LoadAnchorsWithType(result.Value, AnchorType.Sentinel);
            foreach (var sentinel in sentinels)
            {
                Pathfinding sentinelPathfinding = sentinel.GetComponent<Pathfinding>();
                sentinelPathfinding.Grid = m_PathGrid;
                SmartAgent sentinelSmartAgent = sentinel.GetComponent<SmartAgent>();
                sentinelSmartAgent.TargetsList = targetTransform;
            }
        }
        else
        {
            Debug.LogError($"Load failed with error {result.Status}.");
        }
    }

    private async Task<List<GameObject>> LoadAnchorsWithType(List<OVRSpatialAnchor.UnboundAnchor> anchors, AnchorType anchorType)
    {
        List<GameObject> objects = new List<GameObject>();
        List<Task<GameObject>> localizationTasks = new List<Task<GameObject>>();

        foreach (var unboundAnchor in anchors)
        {
            var objectType = PlayerPrefs.GetInt(unboundAnchor.Uuid.ToString());
            if (objectType != Convert.ToInt32(anchorType))
                continue;

            var tcs = new TaskCompletionSource<GameObject>();
        
            unboundAnchor.LocalizeAsync().ContinueWith((success, anchor) =>
            {
                if (success)
                {
                    Debug.Log($"ObjectType {objectType} and typesList {m_typesList.Count}");
                    
                    if (objectType > m_typesList.Count - 1)
                        return;
                    
                    GameObject gameObjectType = m_typesList[objectType];
                    var spatialAnchor = new GameObject($"Anchor {unboundAnchor.Uuid}")
                        .AddComponent<OVRSpatialAnchor>();

                    unboundAnchor.BindTo(spatialAnchor);

                    GameObject anchorObject = Instantiate(gameObjectType, spatialAnchor.transform);
                    anchorObject.transform.parent = spatialAnchor.transform;

                    tcs.SetResult(anchorObject);
                }
                else
                {
                    Debug.LogError($"Localization failed for anchor {unboundAnchor.Uuid}");
                    tcs.SetResult(null);
                }
            }, unboundAnchor);

            localizationTasks.Add(tcs.Task);
        }

        var results = await Task.WhenAll(localizationTasks);
        return results.Where(obj => obj != null).ToList();
    }
}