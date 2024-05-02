using System.Reflection;
using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(ButtonAttribute))]
public class ButtonDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        ButtonAttribute button = attribute as ButtonAttribute;
        object buttonObject = property.serializedObject.targetObject;

        MethodInfo method = buttonObject.GetType().GetMethod(button.FunctionName);

        if (method == null || method.GetParameters().Length > 0) EditorGUILayout.LabelField("Method could not be found, or has parameters.");
        else if (GUI.Button(position, button.OverrideName == "" ? method.Name : button.OverrideName))
        {
            method.Invoke(buttonObject, null);
        }
    }
}
