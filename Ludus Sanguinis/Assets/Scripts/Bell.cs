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
        Debug.Log($"ended hover");
        hovering = false;
        endTurnText.SetActive(false);
    }

    public Transform GetHoverCopyTransform()
    {
        return copyTransform;
    }


    void Update()
    {
        Debug.Log($"hovering: {hovering}. player played cards empty: {GameManager.Instance.Table.PlayerCards.IsEmpty()}");
        if (hovering && !GameManager.Instance.Table.PlayerCards.IsEmpty())
        {
            Debug.Log($"set endturn text to active");
            endTurnText.SetActive(true);
        }
    }
}
