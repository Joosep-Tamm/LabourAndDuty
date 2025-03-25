using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactors.Visuals;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using System.Net;
using static UnityEngine.Rendering.DebugUI;

// First, create an enum for all possible action types
public enum ActionType
{
    WrenchPlace,
    WrenchRotate,
    NailHit,
    Weld,
    Placement,
    DropOff
}

[System.Serializable]
public class RhythmAction
{
    public ActionType actionType;
    public float actionTime;      // When this action should occur
    public float indicatorTime;   // How long before to show indicator
    public float window;          // Time window to complete action
    public string targetObjectID; // Reference to object action relates to
}

[System.Serializable]
public class ObjectDropOff : RhythmAction
{
    public float moveTime; // How long the object should take to exit the view
}

[System.Serializable]
public class PlacementRhythmAction : RhythmAction
{
    public string requiredTag; // Should match the compareTag in SnapToAssemblyPoint
}

[System.Serializable]
public class WeldRhythmAction : RhythmAction
{
    public Vector3[] points;
    public bool isCurved;
    public Vector3[] controlPoints;
}

[System.Serializable]
public struct NailHitTiming
{
    public float hitTime;        // Local time relative to action start
    public float indicatorTime;  // How long before the hit to show indicator
    public float window;         // Time window for this specific hit
}

[System.Serializable]
public class NailRhythmAction : RhythmAction
{
    public float correctAngleThreshold = 30f;
    public float correctHitSpeed = 0.7f;
    public float hitsNeeded = 1f;
    public float length;
    
    public bool autoSpaceHits = true;
    public float hitSpacing = 1.0f;    // Time between hits when auto-spacing
    public float defaultIndicatorTime = 0.5f; // Used for auto-spacing
    public float defaultWindow = 0.3f;        // Used for auto-spacing
    
    public NailHitTiming[] hitTimings;  // Used when not auto-spacing

    public NailHitTiming GetHitTiming(int hitIndex)
    {
        if (autoSpaceHits)
        {
            return new NailHitTiming
            {
                hitTime = hitIndex * hitSpacing,
                indicatorTime = defaultIndicatorTime,
                window = defaultWindow
            };
        }
        else
        {
            return hitTimings[hitIndex];
        }
    }
}

[System.Serializable]
public class WrenchRhythmAction : RhythmAction
{
    public bool isPlacement; // true = place wrench, false = rotate wrench
}

[System.Serializable]
public class SpawnableObject
{
    public Vector3 position;
    public Vector3 rotation;
    public Vector3 scale;
    public GameObject prefab;  // The object to spawn (bolt, nail, etc.)
    public float spawnTime;   // When to spawn this object
    public float beltTime;  // How long the object should take on the belt to reach the player
    public string objectID; // Object to be spawned
    public string parentID; // Parent object (if it has one) e.g box being aprent of nail
}

[System.Serializable]
public class LevelSequence
{
    public string sequenceName;
    public SpawnableObject[] spawnableObjects;  // List of objects to spawn and when
    [SerializeReference]
    public RhythmAction[] actions;    // List of actions to perform on those objects
}

// Main rhythm manager
public class RhythmManager : MonoBehaviour
{
    [Header("Sequences")]
    [SerializeField] public LevelSequence[] sequences;
    [SerializeField] public int currentSequenceIndex = 0;

    private float songStartTime;
    private List<RhythmAction> activeActions = new List<RhythmAction>();
    private Dictionary<RhythmAction, GameObject> actionIndicators = new Dictionary<RhythmAction, GameObject>();
    private Dictionary<GameObject, (Indicator placeIndicator, Indicator rotateIndicator)> boltIndicators = new Dictionary<GameObject, (Indicator, Indicator)>();
    private Dictionary<string, (WeldGuideSystem guideSystem, WeldPaintSystem paintSystem)> weldSystems = new Dictionary<string, (WeldGuideSystem, WeldPaintSystem)>();
    private Dictionary<string, (NailInteraction nail, NailRhythmAction action, GameObject indicator)> activeNails = new Dictionary<string, (NailInteraction, NailRhythmAction, GameObject indicator)>();
    private Dictionary<(string objectId, ActionType actionType), (BoltInteraction bolt, WrenchRhythmAction action, GameObject indicator)> activeBolts = new Dictionary<(string, ActionType), (BoltInteraction, WrenchRhythmAction, GameObject)>();
    private Dictionary<string, (SnapToAssemblyPoint snap, PlacementRhythmAction action, GameObject indicator)> activePlacements = new Dictionary<string, (SnapToAssemblyPoint, PlacementRhythmAction, GameObject)>();
    private bool outOfActions = false;

    private Dictionary<string, GameObject> spawnedObjectsById = new Dictionary<string, GameObject>();

    private bool isPlaying = false;
    private float sequenceStartTime = 0f;
    private float pausedTime = 0f;
    private bool isPaused = false;

    [SerializeField] private SequenceSelectionUI selectionUI;
    private bool isInitialized = false;

    [SerializeField] private AudioSource audioSource;
    [SerializeField] private SpawnPoint spawnPoint;
    [SerializeField] private string[] parentObjectTags;

    [SerializeField] private XRRayInteractor leftInteractor;
    [SerializeField] private XRInteractorLineVisual leftInteractorVisual;
    [SerializeField] private XRRayInteractor rightInteractor;
    [SerializeField] private XRInteractorLineVisual rightInteractorVisual;

    void Start()
    {
        InitializeSequence();
        isInitialized = true;
    }

    public void ShowSequenceSelector()
    {
        if (isPlaying)
        {
            PauseSequence();
        }
        selectionUI.Show();
        leftInteractor.enabled = true;
        rightInteractor.enabled = true;
        leftInteractorVisual.enabled = true;
        rightInteractorVisual.enabled = true;
    }

    public void CloseSequenceSelector()
    {
        selectionUI.Hide();
        leftInteractor.enabled = false;
        rightInteractor.enabled = false;
        leftInteractorVisual.enabled = false;
        rightInteractorVisual.enabled = false;
    }

    private void InitializeSequence()
    {
        // Initialize any necessary components without starting the timing
        isPlaying = false;
        activeActions.Clear();
        // ... any other initialization
    }

    public void StartSequence()
    {
        StopAllCoroutines();

        leftInteractor.enabled = false;
        rightInteractor.enabled = false;
        leftInteractorVisual.enabled = false;
        rightInteractorVisual.enabled = false;

        if (!isInitialized)
        {
            InitializeSequence();
            isInitialized = true;
        }

        sequenceStartTime = (float)Time.timeAsDouble;
        isPlaying = true;
        audioSource.Play();
        StartCoroutine(SpawnScheduler());
        StartCoroutine(ActionScheduler());
        isInitialized = false;
    }

    public void PauseSequence()
    {
        if (!isPlaying || isPaused) return;
        isPaused = true;
        audioSource.Pause();
        pausedTime = (float)Time.timeAsDouble;
    }

    public void ResumeSequence()
    {
        if (!isPaused) return;
        isPaused = false;
        audioSource.UnPause();
        sequenceStartTime += ((float)Time.timeAsDouble - pausedTime);
    }

    private float GetSequenceTime()
    {
        if (!isPlaying) return -1f;
        if (isPaused) return (float)(Time.timeAsDouble - pausedTime);
        return (float)(Time.timeAsDouble - sequenceStartTime);
    }

    private IEnumerator SpawnScheduler()
    {
        if (sequences.Length == 0 || currentSequenceIndex >= sequences.Length) yield break;

        LevelSequence currentSequence = sequences[currentSequenceIndex];
        int nextSpawnIndex = 0;

        while (nextSpawnIndex < currentSequence.spawnableObjects.Length)
        {
            if (!isPlaying) yield return null;

            float currentTime = GetSequenceTime();
            SpawnableObject nextSpawn = currentSequence.spawnableObjects[nextSpawnIndex];

            if (currentTime >= nextSpawn.spawnTime)
            {
                SpawnObject(nextSpawn);
                nextSpawnIndex++;
            }

            yield return null;
        }
    }

    private void SpawnObject(SpawnableObject objectToSpawn)
    {
        if (objectToSpawn.objectID.Contains("Wheel"))
        {
            GameObject wheel = Instantiate(objectToSpawn.prefab, objectToSpawn.position, Quaternion.Euler(objectToSpawn.rotation));
            spawnedObjectsById[objectToSpawn.objectID] = wheel;
            return;
        }
        if (parentObjectTags.Contains(objectToSpawn.objectID))
        {
            GameObject spawned = spawnPoint.Spawn(objectToSpawn.prefab, objectToSpawn.beltTime);
            spawnedObjectsById[objectToSpawn.objectID] = spawned;
            return;
        }
        if (!spawnedObjectsById.ContainsKey(objectToSpawn.parentID))
        {
            return;
        }
        Transform parentTransform = spawnedObjectsById[objectToSpawn.parentID].transform;

        GameObject spawnedObject = Instantiate(objectToSpawn.prefab, parentTransform);
        spawnedObject.transform.localPosition = objectToSpawn.position;
        spawnedObject.transform.localRotation = Quaternion.Euler(objectToSpawn.rotation);
        spawnedObject.transform.localScale = objectToSpawn.scale;
        Debug.Log("Spawning object: " + objectToSpawn.objectID + ", time: " + Time.timeAsDouble);
        spawnedObjectsById[objectToSpawn.objectID] = spawnedObject;
        Debug.Log("Item " + objectToSpawn.prefab.name + " spawned at local pos: " + spawnedObject.transform.localPosition);

        // If this is a weld surface, store its systems
        WeldGuideSystem guideSystem = spawnedObject.GetComponentInChildren<WeldGuideSystem>();
        WeldPaintSystem paintSystem = spawnedObject.GetComponent<WeldPaintSystem>();
        if (guideSystem != null && paintSystem != null)
        {
            guideSystem.HideGuideLine();
            paintSystem.DisableWelding();
            weldSystems[objectToSpawn.objectID] = (guideSystem, paintSystem);
        }

        // If this is a bolt, store its indicators
        Transform placeIndicator = spawnedObject.transform.Find("PlacementIndicator");
        Transform rotateIndicator = spawnedObject.transform.Find("RotationIndicator");

        if (placeIndicator != null && rotateIndicator != null)
        {
            var placeInd = placeIndicator.GetComponent<Indicator>();
            var rotateInd = rotateIndicator.GetComponent<Indicator>();

            if (placeInd != null && rotateInd != null)
            {
                boltIndicators[spawnedObject] = (placeInd, rotateInd);
                placeInd.gameObject.SetActive(false);
                rotateInd.gameObject.SetActive(false);
            }
        }
    }

    void Update()
    {
        if (!isPlaying) return;

        float currentTime = GetSequenceTime();
        ProcessActiveActions(currentTime);
    }

    private IEnumerator ActionScheduler()
    {
        if (sequences.Length == 0 || currentSequenceIndex >= sequences.Length) yield break;

        LevelSequence currentSequence = sequences[currentSequenceIndex];
        int nextActionIndex = 0;

        while (nextActionIndex < currentSequence.actions.Length)
        {
            if (!isPlaying) yield return null;

            float currentTime = GetSequenceTime();
            RhythmAction nextAction = currentSequence.actions[nextActionIndex];

            if (currentTime >= nextAction.actionTime - nextAction.indicatorTime)
            {
                SpawnNewAction(nextAction);
                nextActionIndex++;
            }

            yield return null;
        }
        outOfActions = true;
    }

    private void SpawnNewAction(RhythmAction action)
    {
        Debug.Log("Spawning action: " + action.actionType + ", " + action.targetObjectID);
        // Find the target object this action applies to
        GameObject targetObject;
        if (!spawnedObjectsById.TryGetValue(action.targetObjectID, out targetObject))
        {
            Debug.Log("Could not find object " + action.targetObjectID);
            return;
        }

        switch (action.actionType)
        {
            case ActionType.WrenchPlace:
                HandleWrenchAction(targetObject, action as WrenchRhythmAction, "PlacementIndicator");
                break;

            case ActionType.WrenchRotate:
                HandleWrenchAction(targetObject, action as WrenchRhythmAction, "RotationIndicator");
                break;

            case ActionType.NailHit:
                var nailAction = action as NailRhythmAction;
                if (nailAction != null)
                {
                    var nailInteraction = targetObject.GetComponent<NailInteraction>();
                    if (nailInteraction != null)
                    {
                        // Set up nail hit callback
                        nailInteraction.onNailHit += (angle, speed, hammer) => HandleNailHit(action.targetObjectID, angle, speed, hammer);

                        var indicator = targetObject.GetComponentInChildren<Indicator>();
                        if (indicator != null)
                        {
                            indicator.gameObject.SetActive(false); // Start disabled
                        }

                        // Store nail information
                        activeNails[action.targetObjectID] = (nailInteraction, nailAction, indicator.gameObject);

                        StartCoroutine(HandleNailIndicators(action.targetObjectID));
                    }
                }
                break;

            case ActionType.Weld:
                var weldAction = action as WeldRhythmAction;
                if (weldAction != null && weldSystems.ContainsKey(action.targetObjectID))
                {
                    var (guideSystem, paintSystem) = weldSystems[action.targetObjectID];
                    if (weldAction.isCurved && weldAction.controlPoints.Length >= 4)
                    {
                        guideSystem.SetBezierCurve(
                            weldAction.controlPoints[0],
                            weldAction.controlPoints[1],
                            weldAction.controlPoints[2],
                            weldAction.controlPoints[3]
                        );
                    }
                    else if (weldAction.points.Length >= 2)
                    {
                        guideSystem.SetStraightLine(weldAction.points[0], weldAction.points[1]);
                    }
                    // Set the time to hit for the guide line
                    guideSystem.SetTimeToHit(weldAction.indicatorTime);

                    guideSystem.ShowGuideLine();
                    paintSystem.DisableWelding();
                }
                break;

            case ActionType.DropOff:
                var dropOffAction = action as ObjectDropOff;
                GameObject objectToMove = spawnedObjectsById[dropOffAction.targetObjectID];
                MovementOnBelt movement = objectToMove.GetComponent<MovementOnBelt>();
                if (movement != null)
                {
                    movement.MoveToDropOff(action.window);
                }
                break;

            case ActionType.Placement:
                Debug.Log("Spawning placement action");
                var placementAction = action as PlacementRhythmAction;
                if (placementAction != null)
                {
                    HandlePlacementAction(targetObject, placementAction);
                }
                else
                {
                    Debug.Log(placementAction);
                }
                break;
        }

        activeActions.Add(action);
    }

    private void ProcessActiveActions(float currentTime)
    {
        for (int i = activeActions.Count - 1; i >= 0; i--)
        {
            var action = activeActions[i];

            // Special handling for nail actions
            if (action.actionType == ActionType.NailHit)
            {
                // Skip the general window check for nails
                // They are handled by their own timing system
                continue;
            }

            // Check if action window has passed
            if (currentTime > action.actionTime + action.window)
            {
                //Debug.Log($"Action window expired: {action.actionType} on {action.targetObjectID} at time {currentTime}");
                switch (action.actionType)
                {
                    case ActionType.WrenchPlace:
                    case ActionType.WrenchRotate:
                        if (activeBolts.ContainsKey((action.targetObjectID, action.actionType)))
                        {
                            var (_, _, indicator) = activeBolts[(action.targetObjectID, action.actionType)];
                            if (indicator != null)
                            {
                                indicator.SetActive(false);
                            }
                            activeBolts.Remove((action.targetObjectID, action.actionType));
                        }
                        break;

                    case ActionType.Weld:
                        if (weldSystems.ContainsKey(action.targetObjectID))
                        {
                            weldSystems[action.targetObjectID].guideSystem.HideGuideLine();
                            weldSystems[action.targetObjectID].paintSystem.DisableWelding();
                        }
                        break;
                }

                // Clean up any remaining indicators
                if (actionIndicators.ContainsKey(action))
                {
                    var indicator = actionIndicators[action];
                    if (indicator != null)
                    {
                        indicator.SetActive(false);
                    }
                    actionIndicators.Remove(action);
                }

                HandleMissedAction(action);
                activeActions.RemoveAt(i);
            }
            // Check if action is complete within window
            else if (IsWithinWindow(currentTime, action.actionTime, action.window))
            {
                switch (action.actionType)
                {
                    case ActionType.Weld:
                        if (weldSystems.ContainsKey(action.targetObjectID))
                        {
                            weldSystems[action.targetObjectID].paintSystem.EnableWelding();
                        }
                        break;
                    case ActionType.Placement:
                        if (activePlacements.ContainsKey(action.targetObjectID))
                        {
                            activePlacements[action.targetObjectID].snap.EnableSnap();
                        }
                        break;
                }
                bool actionComplete = CheckActionCompletion(action);
                if (actionComplete)
                {
                    HandleSuccessfulAction(action);
                    activeActions.RemoveAt(i);
                }
            }
        }

        if (outOfActions && !activeActions.Any())
        {
            HandleSequenceComplete();
        }
    }

    private IEnumerator HandleNailIndicators(string nailId)
    {
        if (!activeNails.ContainsKey(nailId)) yield break;

        var (nail, action, indicator) = activeNails[nailId];
        int expectedHit = 0;

        //Debug.Log($"Starting nail indicators for {nailId}, total hits needed: {action.hitsNeeded}");

        while (expectedHit < action.hitsNeeded)
        {
            var timing = action.GetHitTiming(expectedHit);
            float globalHitTime = action.actionTime + timing.hitTime;
            float indicatorTime = globalHitTime - timing.indicatorTime;

            //Debug.Log($"Hit {expectedHit}: Global hit time: {globalHitTime}, Indicator time: {indicatorTime}");
            //Debug.Log($"Current time: {GetSequenceTime()}");

            // Wait until it's time to show the indicator
            while (GetSequenceTime() < indicatorTime)
            {
                if (!activeNails.ContainsKey(nailId))
                {
                    //Debug.Log($"Nail {nailId} removed while waiting for indicator time");
                    yield break;
                }
                yield return null;
            }

            // Show indicator
            if (activeNails.ContainsKey(nailId) && indicator != null)
            {
                //Debug.Log($"Showing indicator for hit {expectedHit}");
                indicator.GetComponent<Indicator>().timeToHit = timing.indicatorTime;
                indicator.SetActive(true);
            }
            else
            {
                //Debug.Log($"Failed to show indicator - Nail exists: {activeNails.ContainsKey(nailId)}, Indicator null: {indicator == null}");
            }

            // Wait until hit window is over
            while (GetSequenceTime() < globalHitTime + timing.window)
            {
                if (!activeNails.ContainsKey(nailId))
                {
                    //Debug.Log($"Nail {nailId} removed during hit window");
                    yield break;
                }
                yield return null;
            }

            // Window has passed without successful hit
            if (activeNails.ContainsKey(nailId) && nail.CurrentHit == expectedHit)
            {
                //Debug.Log($"Hit {expectedHit} window passed without success");
                if (indicator != null)
                {
                    indicator.SetActive(false);
                }
                HandleNailFailure(nailId);

                // Don't break here, allow for next hit
                if (!activeNails.ContainsKey(nailId))
                {
                    //Debug.Log("Nail removed after failure, ending indicator routine");
                    yield break;
                }
            }

            expectedHit++;
            //Debug.Log($"Moving to next hit: {expectedHit}");
        }

        //Debug.Log($"Completed all hits for {nailId}");
    }

    private void HandleWrenchAction(GameObject targetObject, WrenchRhythmAction wrenchAction, string indicatorName)
    {
        if (wrenchAction == null)
        {
            Debug.LogError("WrenchAction is null");
            return;
        }


        var boltInteraction = targetObject.GetComponent<BoltInteraction>();
        if (boltInteraction == null)
        {
            Debug.LogError($"No BoltInteraction found on {targetObject.name}");
            return;
        }

        if (!boltIndicators.ContainsKey(targetObject))
        {
            Debug.LogError($"No indicators stored for {targetObject.name}");
            return;
        }

        if (boltInteraction != null && boltIndicators.ContainsKey(targetObject))
        {
            boltInteraction.onWrenchAction += (success) =>
                HandleBoltAction(wrenchAction.targetObjectID, wrenchAction.actionType, success);

            var indicators = boltIndicators[targetObject];
            GameObject indicatorObj;

            if (indicatorName == "PlacementIndicator")
            {
                indicatorObj = indicators.placeIndicator.gameObject;
            }
            else // "RotationIndicator"
            {
                indicatorObj = indicators.rotateIndicator.gameObject;
            }

            if (indicatorObj != null)
            {
                Debug.Log($"Setting up {indicatorName} for {wrenchAction.targetObjectID}");
                indicatorObj.GetComponent<Indicator>().timeToHit = wrenchAction.indicatorTime;
                indicatorObj.SetActive(true);
                activeBolts[(wrenchAction.targetObjectID, wrenchAction.actionType)] = (boltInteraction, wrenchAction, indicatorObj);
            }
        }
    }

    private void HandleBoltAction(string boltId, ActionType actionType, bool success)
    {
        if (!activeBolts.ContainsKey((boltId, actionType))) return;

        var (bolt, action, indicator) = activeBolts[(boltId, actionType)];
        float currentTime = GetSequenceTime();

        if (IsWithinWindow(currentTime, action.actionTime, action.window))
        {
            if (success)
            {
                // Hide indicator
                if (indicator != null)
                {
                    indicator.SetActive(false);
                }

                // Move bolt forward if it was a rotation action
                if (!action.isPlacement)
                {
                    bolt.transform.position += bolt.transform.forward * -0.03f;
                }

                // Remove from active actions and bolts
                var rhythmAction = activeActions.Find(a => a.targetObjectID == boltId && a.actionType == actionType);
                if (rhythmAction != null)
                {
                    activeActions.Remove(rhythmAction); 
                }
                activeBolts.Remove((boltId, actionType));
            }
        }
    }

    private void HandlePlacementAction(GameObject targetObject, PlacementRhythmAction placementAction)
    {
        Debug.Log("Handling placement for: " + placementAction.targetObjectID);
        var snapPoint = targetObject.GetComponent<SnapToAssemblyPoint>();
        if (snapPoint == null)
        {
            Debug.LogError($"No SnapToAssemblyPoint found on {targetObject.name}");
            return;
        }

        // Find and setup the indicator
        var indicator = targetObject.GetComponent<Indicator>();
        if (indicator != null)
        {
            indicator.timeToHit = placementAction.indicatorTime;
            indicator.gameObject.SetActive(true);
        }
        else
        {
            Debug.Log("Could not find indicator on " + targetObject.name);
        }

        // Add a trigger detection component to handle the snap event
        snapPoint.OnObjectSnapped += (success) => HandlePlacementComplete(placementAction.targetObjectID, success);

        // Store the placement information
        activePlacements[placementAction.targetObjectID] = (snapPoint, placementAction, indicator?.gameObject);
    }

    private bool CheckActionCompletion(RhythmAction action)
    {
        switch (action.actionType)
        {
            case ActionType.WrenchPlace:
                return false;
            case ActionType.WrenchRotate:
                return false;
            case ActionType.Weld:
                if (weldSystems.ContainsKey(action.targetObjectID))
                {
                    var (_, paintSystem) = weldSystems[action.targetObjectID];
                    var accuracy = paintSystem.CheckWeldProgress();
                    return accuracy >= 0.8f; // 80% accuracy threshold
                }
                return false;
            case ActionType.NailHit:
                // Nail completion is handled by its own system
                return false;
            default:
                return false;
        }
    }

    private void HandleNailHit(string nailId, float angle, float speed, GameObject hammer)
    {
        if (!activeNails.ContainsKey(nailId)) return;

        var (nail, action, indicator) = activeNails[nailId];
        float currentTime = GetSequenceTime();

        var timing = action.GetHitTiming(nail.CurrentHit);
        float globalHitTime = action.actionTime + timing.hitTime;

        if (IsWithinWindow(currentTime, globalHitTime, timing.window))
        {
            if (angle < action.correctAngleThreshold && speed > action.correctHitSpeed)
            {
                // Successful hit
                var hammerhaptics = hammer.GetComponent<ToolHaptics>();
                if (hammerhaptics != null)
                {
                    hammerhaptics.TriggerHaptic();
                }

                float moveDistance = action.length * (1f / action.hitsNeeded);
                nail.MoveNail(moveDistance);
                nail.IncrementHit();

                // Hide indicator after successful hit
                if (indicator != null)
                {
                    indicator.SetActive(false);
                }

                if (nail.CurrentHit >= action.hitsNeeded)
                {
                    HandleNailSuccess(nailId);
                }
            }
        }
        else if (currentTime > globalHitTime + timing.window)
        {
            // Failed this hit
            HandleNailFailure(nailId);
        }
    }

    private void HandleNailSuccess(string nailId)
    {
        if (!activeNails.ContainsKey(nailId)) return;

        var (nail, action, indicator) = activeNails[nailId];

        // Find and handle the corresponding RhythmAction
        var rhythmAction = activeActions.Find(a => a.targetObjectID == nailId && a.actionType == ActionType.NailHit);
        if (rhythmAction != null)
        {
            if (actionIndicators.ContainsKey(rhythmAction))
            {
                actionIndicators.Remove(rhythmAction);
            }
            activeActions.Remove(rhythmAction);
        }

        if (indicator != null)
        {
            indicator.SetActive(false);
        }

        // Clean up nail
        nail.DisableNail();
        activeNails.Remove(nailId);
    }

    private void HandleNailFailure(string nailId)
    {
        if (!activeNails.ContainsKey(nailId)) return;

        var (nail, action, indicator) = activeNails[nailId];
        //Debug.Log($"Handling nail failure for {nailId}, current hit: {nail.CurrentHit}");

        // Hide the current indicator
        if (indicator != null)
        {
            indicator.SetActive(false);
        }

        // Only fully disable and remove if we've failed the last possible hit
        if (nail.CurrentHit >= action.hitsNeeded - 1)
        {
            //Debug.Log("Final hit failed, removing nail");
            // Find and remove from active actions
            var rhythmAction = activeActions.Find(a => a.targetObjectID == nailId);
            if (rhythmAction != null)
            {
                activeActions.Remove(rhythmAction);
            }

            // Disable the nail interaction
            nail.DisableNail();

            // Remove from active nails
            activeNails.Remove(nailId);
        }
        else
        {
            //Debug.Log($"Hit {nail.CurrentHit} failed, continuing to next hit");
            nail.IncrementHit();
        }
    }

    private void HandlePlacementComplete(string objectId, bool success)
    {
        if (!activePlacements.ContainsKey(objectId)) return;

        var (snap, action, indicator) = activePlacements[objectId];
        float currentTime = GetSequenceTime();

        if (IsWithinWindow(currentTime, action.actionTime, action.window))
        {
            if (success)
            {
                // Hide indicator
                if (indicator != null)
                {
                    //indicator.SetActive(false);
                }

                snap.DisableSnap();

                // Remove from active actions and placements
                var rhythmAction = activeActions.Find(a => a.targetObjectID == objectId && a.actionType == ActionType.Placement);
                if (rhythmAction != null)
                {
                    activeActions.Remove(rhythmAction);
                }
                activePlacements.Remove(objectId);
            }
        }
    }

    private void HandleSuccessfulAction(RhythmAction action)
    {
        if (actionIndicators.ContainsKey(action))
        {
            actionIndicators.Remove(action);
        }

        GameObject targetObject;
        if (spawnedObjectsById.TryGetValue(action.targetObjectID, out targetObject))
        {
            switch (action.actionType)
            {
                case ActionType.WrenchRotate:
                    break;
                case ActionType.Weld:
                    if (weldSystems.ContainsKey(action.targetObjectID))
                    {
                        var (guideSystem, paintSystem) = weldSystems[action.targetObjectID];
                        guideSystem.HideGuideLine();


                        //paintSystem.enabled = false;
                        paintSystem.DisableWelding();
                    }
                    break;
            }
        }
    }

    private void HandleMissedAction(RhythmAction action)
    {
        // Don't handle nail misses here
        if (action.actionType == ActionType.NailHit) return;

        // Clean up indicators
        if (actionIndicators.ContainsKey(action))
        {
            var indicator = actionIndicators[action];
            if (indicator != null)
            {
                indicator.SetActive(false);
            }
            actionIndicators.Remove(action);
        }

        // Additional cleanup based on action type
        switch (action.actionType)
        {
            case ActionType.WrenchPlace:
            case ActionType.WrenchRotate:
                var key = (action.targetObjectID, action.actionType);
                if (activeBolts.ContainsKey(key))
                {
                    var (bolt, boltAction, indicator) = activeBolts[key];
                    Debug.Log($"Handling missed action for {action.targetObjectID}, type: {action.actionType}");
                    Debug.Log($"Found indicator: {indicator != null}");
                    //Debug.Log($"Deactivating bolt indicator: {indicator != null}");
                    if (indicator != null)
                    {
                        Debug.Log($"Deactivating indicator for {action.targetObjectID}");
                        indicator.SetActive(false);
                    }
                    activeBolts.Remove(key);
                }
                break;

            case ActionType.Weld:
                if (weldSystems.ContainsKey(action.targetObjectID))
                {
                    var (guideSystem, paintSystem) = weldSystems[action.targetObjectID];
                    guideSystem.HideGuideLine();

                    
                    //paintSystem.enabled = false;
                    paintSystem.DisableWelding(); 
                }
                break;
            case ActionType.Placement:
                if (activePlacements.ContainsKey(action.targetObjectID))
                {
                    var (snap, placementAction, indicator) = activePlacements[action.targetObjectID];
                    if (indicator != null)
                    {
                        indicator.SetActive(false);
                    }
                    foreach (GameObject toDelete in GameObject.FindGameObjectsWithTag(placementAction.requiredTag)){
                        string item = spawnedObjectsById.First(kvp => kvp.Value == toDelete).Key;
                        spawnedObjectsById.Remove(item);
                        Destroy(toDelete);
                    }
                    activePlacements.Remove(action.targetObjectID);
                }
                break;
        }

        //Debug.Log($"Action missed: {action.actionType} on {action.targetObjectID}");
    }

    private void HandleSequenceComplete()
    {
        //Debug.Log("Sequence " + sequences[currentSequenceIndex].sequenceName + " complete");
        // Clean up all spawned objects
        foreach (var obj in spawnedObjectsById.Values)
        {
            if (obj != null)
                Destroy(obj);
        }
        spawnedObjectsById.Clear();
        boltIndicators.Clear();
        actionIndicators.Clear();
        weldSystems.Clear();
        activeNails.Clear();
        activePlacements.Clear();
        audioSource.Stop();

        // Move to next sequence or end
        currentSequenceIndex++;
        if (currentSequenceIndex < sequences.Length)
        {
            StartSequence();
        }
        else
        {
            enabled = false;
        }
    }

    public void ResetSequence()
    {
        foreach (var obj in spawnedObjectsById.Values)
        {
            if (obj != null)
                Destroy(obj);
        }
        isPlaying = false;
        isPaused = false;
        activeActions.Clear();
        spawnedObjectsById.Clear();
        boltIndicators.Clear();
        actionIndicators.Clear();
        weldSystems.Clear();
        activeNails.Clear();
        activePlacements.Clear();
    }

    private bool IsWithinWindow(float currentTime, float targetTime, float window)
    {
        return currentTime >= targetTime && currentTime <= targetTime + window;
    }
}