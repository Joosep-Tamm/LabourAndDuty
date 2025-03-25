using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR
[CustomEditor(typeof(RhythmManager))]
public class RhythmManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        RhythmManager manager = (RhythmManager)target;

        // Draw default inspector
        DrawDefaultInspector();

        // Add buttons for creating new actions
        if (GUILayout.Button("Add Nail Action"))
        {
            var sequence = manager.sequences[manager.currentSequenceIndex];
            var newAction = new NailRhythmAction
            {
                actionType = ActionType.NailHit,
                correctAngleThreshold = 30f,
                correctHitSpeed = 0.7f,
                hitsNeeded = 1f,
                length = 0.1f,
                autoSpaceHits = true,
                hitSpacing = 1.0f,
                // Local times: 0 is the action start, then 1 second later, then 2 seconds later
                hitTimings = new NailHitTiming[] {}  // Default manual timings
            };
            ArrayUtility.Add(ref sequence.actions, newAction);
            EditorUtility.SetDirty(manager);
        }

        if (GUILayout.Button("Add Weld Action"))
        {
            var sequence = manager.sequences[manager.currentSequenceIndex];
            var newAction = new WeldRhythmAction
            {
                actionType = ActionType.Weld,
                isCurved = false,
                points = new Vector3[2]
            };
            ArrayUtility.Add(ref sequence.actions, newAction);
            EditorUtility.SetDirty(manager);
        }

        if (GUILayout.Button("Add Wrench Place Action"))
        {
            var sequence = manager.sequences[manager.currentSequenceIndex];
            var newAction = new WrenchRhythmAction
            {
                actionType = ActionType.WrenchPlace,
                isPlacement = true
            };
            ArrayUtility.Add(ref sequence.actions, newAction);
            EditorUtility.SetDirty(manager);
        }

        if (GUILayout.Button("Add Wrench Rotate Action"))
        {
            var sequence = manager.sequences[manager.currentSequenceIndex];
            var newAction = new WrenchRhythmAction
            {
                actionType = ActionType.WrenchRotate,
                isPlacement = false
            };
            ArrayUtility.Add(ref sequence.actions, newAction);
            EditorUtility.SetDirty(manager);
        }

        if (GUILayout.Button("Add Placement Action"))
        {
            var sequence = manager.sequences[manager.currentSequenceIndex];
            var newAction = new PlacementRhythmAction
            {
                actionType = ActionType.Placement,
                requiredTag = ""
            };
            ArrayUtility.Add(ref sequence.actions, newAction);
            EditorUtility.SetDirty(manager);
        }

        if (GUILayout.Button("Add Object Drop Off Action"))
        {
            var sequence = manager.sequences[manager.currentSequenceIndex];
            var newAction = new ObjectDropOff
            {
                actionType = ActionType.DropOff,
                moveTime = 0f
            };
            ArrayUtility.Add(ref sequence.actions, newAction);
            EditorUtility.SetDirty(manager);
        }

        serializedObject.ApplyModifiedProperties();
    }
}
#endif