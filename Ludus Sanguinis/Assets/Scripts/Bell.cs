using UnityEngine;

public class Bell : MonoBehaviour, IInteractable
{
    [SerializeField] Transform copyTransform;

    public void Interact()
    {
        EventManager.RingBell();
    }

    public Transform GetHoverCopyTransform()
    {
        return copyTransform;
    }
}
