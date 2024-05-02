using UnityEngine;

public class ConditionalFieldAttribute : PropertyAttribute
{
    public readonly string ObjectName;
    public readonly bool TargetValue;


    public ConditionalFieldAttribute(string objectName, bool targetValue = true)
    {
        ObjectName = objectName;
        TargetValue = targetValue;
    }
}
