#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(RhythmAction))]
public class RhythmActionDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        if (property == null) return EditorGUIUtility.singleLineHeight;

        float lineHeight = EditorGUIUtility.singleLineHeight;
        float spacing = EditorGUIUtility.standardVerticalSpacing;
        float totalHeight = lineHeight * 5 + spacing * 4; // Base properties

        var actionTypeProp = property.FindPropertyRelative("actionType");
        if (actionTypeProp != null)
        {
            var actionType = (ActionType)actionTypeProp.enumValueIndex;
            switch (actionType)
            {
                case ActionType.NailHit:
                    totalHeight += (lineHeight + spacing) * 4; // correctAngleThreshold, correctHitSpeed, hitsNeeded, length
                    totalHeight += (lineHeight + spacing) * 1; // autoSpaceHits toggle

                    // Check if auto spacing is enabled
                    var autoSpaceProp = property.FindPropertyRelative("autoSpaceHits");
                    if (autoSpaceProp.boolValue)
                    {
                        totalHeight += (lineHeight + spacing) * 3; // hitSpacing, defaultIndicatorTime, defaultWindow
                    }
                    else
                    {
                        // For the hitTimings array
                        var timingsArray = property.FindPropertyRelative("hitTimings");
                        if (timingsArray != null)
                        {
                            totalHeight += (lineHeight + spacing) * 1; // Array header
                            totalHeight += (lineHeight + spacing) * timingsArray.arraySize * 4; // Each timing entry (size, foldout, properties)
                            totalHeight += (lineHeight + spacing) * 1; // Array footer
                        }
                    }
                    break;

                case ActionType.Weld:
                    totalHeight += lineHeight + spacing; // isCurved property

                    // Points array
                    var pointsProp = property.FindPropertyRelative("points");
                    totalHeight += lineHeight + spacing; // Array header
                    if (pointsProp.isExpanded)
                    {
                        totalHeight += (lineHeight + spacing) * pointsProp.arraySize * 2;
                    }

                    // Control points array
                    var isCurvedProp = property.FindPropertyRelative("isCurved");
                    if (isCurvedProp.boolValue)
                    {
                        var controlPointsProp = property.FindPropertyRelative("controlPoints");
                        totalHeight += lineHeight + spacing; // Array header
                        if (controlPointsProp.isExpanded)
                        {
                            totalHeight += (lineHeight + spacing) * controlPointsProp.arraySize * 2;
                        }
                    }
                    break;
                //
                case ActionType.WrenchPlace:
                case ActionType.WrenchRotate:
                    totalHeight += (lineHeight + spacing) * 1;
                    break;
            }
        }

        return totalHeight;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (property == null) return;

        EditorGUI.BeginProperty(position, label, property);

        float lineHeight = EditorGUIUtility.singleLineHeight;
        float spacing = EditorGUIUtility.standardVerticalSpacing;

        Rect currentRect = new Rect(position.x, position.y, position.width, lineHeight);

        // Draw base properties
        var actionTypeProp = property.FindPropertyRelative("actionType");
        if (actionTypeProp != null)
        {
            EditorGUI.PropertyField(currentRect, actionTypeProp);
            currentRect.y += lineHeight + spacing;

            EditorGUI.PropertyField(currentRect, property.FindPropertyRelative("actionTime"));
            currentRect.y += lineHeight + spacing;

            EditorGUI.PropertyField(currentRect, property.FindPropertyRelative("indicatorTime"));
            currentRect.y += lineHeight + spacing;

            EditorGUI.PropertyField(currentRect, property.FindPropertyRelative("window"));
            currentRect.y += lineHeight + spacing;

            EditorGUI.PropertyField(currentRect, property.FindPropertyRelative("targetObjectID"));
            currentRect.y += lineHeight + spacing;

            // Draw type-specific properties
            var actionType = (ActionType)actionTypeProp.enumValueIndex;

            if (actionType == ActionType.NailHit)
            {
                // Draw nail properties in specific order with conditional display
                DrawNailProperties(currentRect, property, ref lineHeight, ref spacing);
            }
            else if (actionType == ActionType.Weld)
            {
                DrawWeldProperties(currentRect, property, ref lineHeight, ref spacing);
            }
            else
            {
                // Handle other action types as before
                var derivedProps = GetDerivedTypeProperties(property, actionType);
                if (derivedProps != null)
                {
                    foreach (var propName in derivedProps)
                    {
                        var prop = property.FindPropertyRelative(propName);
                        if (prop != null)
                        {
                            EditorGUI.PropertyField(currentRect, prop);
                            currentRect.y += lineHeight + spacing;
                        }
                    }
                }
            }
        }

        EditorGUI.EndProperty();
    } 

    private void DrawNailProperties(Rect currentRect, SerializedProperty property, ref float lineHeight, ref float spacing)
    {
        EditorGUI.PropertyField(currentRect, property.FindPropertyRelative("correctAngleThreshold"));
        currentRect.y += lineHeight + spacing;

        EditorGUI.PropertyField(currentRect, property.FindPropertyRelative("correctHitSpeed"));
        currentRect.y += lineHeight + spacing;

        EditorGUI.PropertyField(currentRect, property.FindPropertyRelative("hitsNeeded"));
        currentRect.y += lineHeight + spacing;

        EditorGUI.PropertyField(currentRect, property.FindPropertyRelative("length"));
        currentRect.y += lineHeight + spacing;

        EditorGUI.PropertyField(currentRect, property.FindPropertyRelative("autoSpaceHits"));
        currentRect.y += lineHeight + spacing;

        var autoSpaceProp = property.FindPropertyRelative("autoSpaceHits");
        if (autoSpaceProp.boolValue)
        {
            EditorGUI.PropertyField(currentRect, property.FindPropertyRelative("hitSpacing"));
            currentRect.y += lineHeight + spacing;
            EditorGUI.PropertyField(currentRect, property.FindPropertyRelative("defaultIndicatorTime"));
            currentRect.y += lineHeight + spacing;
            EditorGUI.PropertyField(currentRect, property.FindPropertyRelative("defaultWindow"));
        }
        else
        {
            EditorGUI.PropertyField(currentRect, property.FindPropertyRelative("hitTimings"));
        }
    }

    private void DrawWeldProperties(Rect currentRect, SerializedProperty property, ref float lineHeight, ref float spacing)
    {
        var isCurvedProp = property.FindPropertyRelative("isCurved");
        EditorGUI.PropertyField(currentRect, isCurvedProp);
        currentRect.y += lineHeight + spacing;

        // Draw points array
        var pointsProp = property.FindPropertyRelative("points");
        var pointsRect = new Rect(currentRect);
        EditorGUI.PropertyField(pointsRect, pointsProp, true);
        float pointsHeight = lineHeight;
        if (pointsProp.isExpanded)
        {
            pointsHeight += (lineHeight + spacing) * pointsProp.arraySize * 3;
        }
        currentRect.y += pointsHeight + spacing;

        // Draw control points array if curved
        if (isCurvedProp.boolValue)
        {
            var controlPointsProp = property.FindPropertyRelative("controlPoints");
            EditorGUI.PropertyField(currentRect, controlPointsProp, true);
        }
    }

    private string[] GetDerivedTypeProperties(SerializedProperty property, ActionType actionType)
    {
        switch (actionType)
        {
            case ActionType.Weld:
                return new[] { "isCurved", "points", "controlPoints" };
            case ActionType.WrenchPlace:
            case ActionType.WrenchRotate:
                return new[] { "isPlacement" };
            default:
                return null;
        }
    }
}
#endif