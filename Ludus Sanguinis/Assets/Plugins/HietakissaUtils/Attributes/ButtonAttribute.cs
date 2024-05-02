using UnityEngine;

public class ButtonAttribute : PropertyAttribute
{
    public readonly string FunctionName;
    public readonly string OverrideName;

    public ButtonAttribute(string functionName, string overrideName = "")
    {
        FunctionName = functionName;
        OverrideName = overrideName;
    }
}