using HietakissaUtils;
using UnityEngine;

public class HandController : MonoBehaviour
{
    [SerializeField] Player player;

    [SerializeField] Camera cam;
    [SerializeField] LayerMask interactMask;
    [SerializeField] LayerMask alignMask;

    [SerializeField] float cardDistanceOffset;

    [SerializeField] Transform target;
    [SerializeField] Transform hand;
    [SerializeField] Vector3 cardHandOffset;
    [SerializeField] float handMoveTime = 0.1f;
    [SerializeField] float handRotSpeed = 12f;

    Table table;
    Vector3 handVelocity;

    Card hoveredCard;
    Card grabbedCard;

    Transform hoveredInteractable;


    RaycastHit hit;

    const float CONST_PLAY_DISTANCE = 10f;


    void Awake()
    {
        GameManager.Instance.player = player;
    }

    void Update()
    {
        float xOffset = Mathf.Cos(Time.time * 0.15f) * 3f;
        float yOffset = Mathf.Sin(Time.time * 0.25f) * 15f;
        Vector3 clampedMousePos = new Vector3(Mathf.Clamp(Input.mousePosition.x + xOffset, 0f, Screen.width), Mathf.Clamp(Input.mousePosition.y + yOffset, 0f, Screen.height), 0f);
        Ray mouseRay = cam.ScreenPointToRay(clampedMousePos);

        HandleCardGrabbingAndHovering();


        if (Input.GetMouseButtonUp(0) && grabbedCard)
        {
            if (Physics.Raycast(mouseRay, out hit, CONST_PLAY_DISTANCE, alignMask) && hit.transform.TryGetComponent(out CardPlayArea playArea))
            {
                table = playArea.Table;
                table.PlayCard(player, grabbedCard);
            }
            else
            {
                player.CardCollection.PlaceCard(grabbedCard);
                grabbedCard.State = CardState.InHand;
            }
            
            grabbedCard = null;
        }


        if (grabbedCard) MoveTarget();

        MoveHand();


        void HandleCardGrabbingAndHovering()
        {
            if (Physics.Raycast(mouseRay, out hit, CONST_PLAY_DISTANCE, interactMask) && hit.transform.TryGetComponent(out Card card) && card.Owner == PlayerType.Player && card.IsInteractable)
            {
                if (Input.GetMouseButtonDown(0) && card.IsInteractable)
                {
                    hoveredCard?.EndHover();
                    grabbedCard = card;

                    if (grabbedCard.State == CardState.OnTable) table.FreeSpotForCard(player, card);
                    else if (grabbedCard.State == CardState.InHand) player.CardCollection.TakeCard(card);
                    grabbedCard.State = CardState.Drag;
                }
                else if (!grabbedCard)
                {
                    // Card under mouse, didn't try to grab

                    if (card)
                    {
                        if (card.CanStartHover || hoveredCard != card)
                        {
                            hoveredCard?.EndHover();

                            hoveredCard = card;
                            hoveredCard.StartHover();
                        }
                    }
                    else
                    {
                        hoveredCard?.EndHover();
                        hoveredCard = null;
                    }
                }
            }
            else
            {
                if (!grabbedCard && hoveredCard)
                {
                    hoveredCard.EndHover();
                    hoveredCard = null;
                }
            }

            // Interacting
            if (!grabbedCard && !hoveredCard)
            {
                if (Physics.Raycast(mouseRay, out hit, CONST_PLAY_DISTANCE, interactMask) && hit.transform.TryGetComponent(out IInteractable interactable))
                {
                    hoveredInteractable = interactable.GetHoverCopyTransform();
                    if (Input.GetMouseButtonDown(0)) interactable.Interact();
                }
                else hoveredInteractable = null;
            }
            else hoveredInteractable = null;
        }

        void MoveTarget()
        {
            Vector3 targetPos = Vector3.zero;
            Vector3 targetDir = Vector3.zero;

            if (Physics.Raycast(mouseRay, out hit, 3f, alignMask))
            {
                targetPos = hit.point + hit.normal * 0.01f;
                targetDir = hit.normal;
            }
            else
            {
                targetPos = mouseRay.origin + mouseRay.direction * cardDistanceOffset;
                targetDir = -mouseRay.direction;
            }

            target.position = targetPos;
            target.forward = -mouseRay.direction;
            grabbedCard.SetTargetTransform(target);

            target.rotation = Quaternion.LookRotation(targetDir, cam.transform.forward.SetY(target.position.y));
        }

        void MoveHand()
        {
            Vector3 targetPos = Vector3.zero;
            Quaternion targetRot = Quaternion.identity;
            float moveTime = 0f;

            Transform targetTransform = grabbedCard?.transform ?? hoveredCard?.transform ?? hoveredInteractable;

            if (targetTransform)
            {
                targetPos = targetTransform.TransformPoint(cardHandOffset);
                targetRot = Quaternion.LookRotation(-targetTransform.forward, targetTransform.up);
            }
            else
            {
                targetPos = mouseRay.origin + mouseRay.direction * cardDistanceOffset;
                targetRot = Quaternion.LookRotation(mouseRay.direction);
                moveTime = handMoveTime;
            }

            hand.position = Vector3.SmoothDamp(hand.position, targetPos, ref handVelocity, moveTime);
            hand.rotation = Quaternion.Slerp(hand.rotation, targetRot, handRotSpeed * Time.deltaTime);
        }
    }
}
