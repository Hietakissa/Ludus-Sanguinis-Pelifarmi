using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(ConditionalFieldAttribute))]
public class ConditionalFieldDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        ConditionalFieldAttribute conditionalField = attribute as ConditionalFieldAttribute;
        SerializedProperty foundProperty = property.serializedObject.FindProperty(conditionalField.ObjectName);

        if (foundProperty != null && foundProperty.propertyType == SerializedPropertyType.Boolean)
        {
            if (foundProperty.boolValue == conditionalField.TargetValue) EditorGUI.PropertyField(position, property, label);
        }
        else
        {
            EditorGUI.LabelField(position, $"Could not find given field or it was not a boolean.");
        }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        ConditionalFieldAttribute conditionalField = attribute as ConditionalFieldAttribute;
        SerializedProperty boolProperty = property.serializedObject.FindProperty(conditionalField.ObjectName);

        if (boolProperty != null && boolProperty.propertyType == SerializedPropertyType.Boolean)
        {
            if (boolProperty.boolValue == conditionalField.TargetValue) return 16f;
            else return 0f;
        }
        return 16f;
    }
}
