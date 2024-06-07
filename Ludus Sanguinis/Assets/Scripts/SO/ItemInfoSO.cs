using UnityEngine;

[CreateAssetMenu(menuName = "Game/Item Info", fileName = "New Item Info")]
public class ItemInfoSO : ScriptableObject
{
    [SerializeField] new string name;
    [SerializeField] [TextArea(2, 5)] string description;

    public string Name => name;
    public string Description => description;
}
