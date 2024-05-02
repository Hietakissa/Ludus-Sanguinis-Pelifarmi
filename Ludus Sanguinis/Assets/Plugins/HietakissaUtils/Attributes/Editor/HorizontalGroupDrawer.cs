using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(HorizontalGroupAttribute))]
public class HorizontalGroupDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        HorizontalGroupAttribute horizontalGroup = attribute as HorizontalGroupAttribute;
        int groupSize = horizontalGroup.GroupSize;

        DrawGroup(position, property, label, groupSize);
    }

    void DrawGroup(Rect position, SerializedProperty property, GUIContent label, int groupSize)
    {
        EditorGUIUtility.labelWidth = 0f;

        int longest = 0;

        EditorGUILayout.BeginHorizontal();

        for (int i = 0; i < groupSize; i++)
        {
            int reservedPixelsPerCharacter = 8;

            /*if (property.type == "bool") EditorGUIUtility.fieldWidth = (position.width - longest * reservedPixelsPerCharacter) / groupSize * 0.1f;
            else */
            

            if (property.type == "bool")
            {
                EditorGUIUtility.labelWidth = property.displayName.Length * reservedPixelsPerCharacter * 0.85f;
                EditorGUILayout.PropertyField(property, true, GUILayout.Width(property.displayName.Length * reservedPixelsPerCharacter + 20f));
            }
            else
            {
                EditorGUIUtility.labelWidth = property.displayName.Length * reservedPixelsPerCharacter;
                EditorGUIUtility.fieldWidth = (position.width - longest * reservedPixelsPerCharacter) / groupSize;
                EditorGUILayout.PropertyField(property, true);
            }

            if (!property.Next(false)) break;
        }
        
        EditorGUILayout.EndHorizontal();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return 0f;
    }
}
