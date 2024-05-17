using UnityEngine;

public class Item : PlayableItem, IInteractable
{
    [SerializeField] Transform hoverCopy;
    public ItemType Type;
    public CardState State = CardState.InHand;

    public Transform GetHoverCopyTransform() => hoverCopy;
    public void Interact()
    {
        GameManager.Instance.Table.PlayItem(GameManager.Instance.Player, this);
    }

    void Awake()
    {
        startScale = transform.localScale;
        targetScale = startScale;
    }

    void Update()
    {
        Vector3 targetPos = TargetTransform.position + posOffset;
        transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref posVel, posSmoothTime);

        Quaternion target = Quaternion.LookRotation(TargetTransform.forward, TargetTransform.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, target, rotateSmoothing * Time.deltaTime);

        transform.localScale = Vector3.SmoothDamp(transform.localScale, targetScale, ref scaleVel, scaleSmoothTime);
    }
}
