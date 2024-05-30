using UnityEngine;

[CreateAssetMenu(menuName = "Game/Dialogue", fileName = "New Dialogue")]
public class TextCollectionSO : ScriptableObject
{
    [SerializeField] TextCollectionMode mode;
    [SerializeField] DialogueElement[] dialogue;

    public TextCollectionMode Mode => mode;
    public DialogueElement[] Dialogue => dialogue;
}

[System.Serializable]
public class DialogueElement
{
    [SerializeField][TextArea(1, 5)] string text;
    public string Text => text;
}

public enum TextCollectionMode
{
    Random,
    Sequential
}