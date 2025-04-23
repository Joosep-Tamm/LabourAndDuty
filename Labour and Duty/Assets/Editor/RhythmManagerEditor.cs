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

        // Add buttons for each sequence
        for (int i = 0; i < manager.sequences.Length; i++)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField($"Add actions to Sequence {i}", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Nail"))
            {
                AddNailAction(manager, i);
            }
            if (GUILayout.Button("Weld"))
            {
                AddWeldAction(manager, i);
            }
            if (GUILayout.Button("Wrench Place"))
            {
                AddWrenchPlaceAction(manager, i);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Wrench Rotate"))
            {
                AddWrenchRotateAction(manager, i);
            }
            if (GUILayout.Button("Placement"))
            {
                AddPlacementAction(manager, i);
            }
            if (GUILayout.Button("Drop Off"))
            {
                AddDropOffAction(manager, i);
            }
            EditorGUILayout.EndHorizontal();
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void AddNailAction(RhythmManager manager, int sequenceIndex)
    {
        var sequence = manager.sequences[sequenceIndex];
        var newAction = new NailRhythmAction
        {
            actionType = ActionType.NailHit,
            correctAngleThreshold = 30f,
            correctHitSpeed = 0.7f,
            hitsNeeded = 1,
            length = 0.1f,
            autoSpaceHits = true,
            hitSpacing = 1.0f,
            hitTimings = new NailHitTiming[] { }
        };
        ArrayUtility.Add(ref sequence.actions, newAction);
        EditorUtility.SetDirty(manager);
    }

    private void AddWeldAction(RhythmManager manager, int sequenceIndex)
    {
        var sequence = manager.sequences[sequenceIndex];
        var newAction = new WeldRhythmAction
        {
            actionType = ActionType.Weld,
            isCurved = false,
            points = new Vector3[2]
        };
        ArrayUtility.Add(ref sequence.actions, newAction);
        EditorUtility.SetDirty(manager);
    }

    private void AddWrenchPlaceAction(RhythmManager manager, int sequenceIndex)
    {
        var sequence = manager.sequences[sequenceIndex];
        var newAction = new WrenchRhythmAction
        {
            actionType = ActionType.WrenchPlace,
            isPlacement = true
        };
        ArrayUtility.Add(ref sequence.actions, newAction);
        EditorUtility.SetDirty(manager);
    }

    private void AddWrenchRotateAction(RhythmManager manager, int sequenceIndex)
    {
        var sequence = manager.sequences[sequenceIndex];
        var newAction = new WrenchRhythmAction
        {
            actionType = ActionType.WrenchRotate,
            isPlacement = false
        };
        ArrayUtility.Add(ref sequence.actions, newAction);
        EditorUtility.SetDirty(manager);
    }

    private void AddPlacementAction(RhythmManager manager, int sequenceIndex)
    {
        var sequence = manager.sequences[sequenceIndex];
        var newAction = new PlacementRhythmAction
        {
            actionType = ActionType.Placement,
            requiredTag = ""
        };
        ArrayUtility.Add(ref sequence.actions, newAction);
        EditorUtility.SetDirty(manager);
    }

    private void AddDropOffAction(RhythmManager manager, int sequenceIndex)
    {
        var sequence = manager.sequences[sequenceIndex];
        var newAction = new ObjectDropOff
        {
            actionType = ActionType.DropOff,
            moveTime = 0f
        };
        ArrayUtility.Add(ref sequence.actions, newAction);
        EditorUtility.SetDirty(manager);
    }
}
#endif