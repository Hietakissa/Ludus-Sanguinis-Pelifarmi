using UnityEngine;

public interface IInteractable
{
    public abstract Transform GetHoverCopyTransform();

    public abstract void Interact();
}
