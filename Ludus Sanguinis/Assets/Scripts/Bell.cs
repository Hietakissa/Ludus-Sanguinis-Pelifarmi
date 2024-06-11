using HietakissaUtils;
using UnityEngine;
using TMPro;

public class Bell : MonoBehaviour, IInteractable
{
    [SerializeField] Transform copyTransform;
    [SerializeField] GameObject endTurnText;

    bool hovering;

    public void Interact()
    {
        EventManager.RingBell();
    }

    public void StartInteractHover()
    {
        hovering = true;
    }

    public void EndInteractHover()
    {
        hovering = false;
        endTurnText.SetActive(false);
    }

    public Transform GetHoverCopyTransform()
    {
        return copyTransform;
    }


    void Update()
    {
        if (hovering && !GameManager.Instance.Table.PlayerCards.IsEmpty())
        {
            endTurnText.SetActive(true);
        }
    }
}
