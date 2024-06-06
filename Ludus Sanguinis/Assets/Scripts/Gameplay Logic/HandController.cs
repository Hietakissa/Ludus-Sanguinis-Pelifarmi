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

    [SerializeField] Table table;
    Vector3 handVelocity;

    PlayableItem hoveredPlayable;
    //Card hoveredCard;
    Card grabbedCard;

    Transform hoveredInteractable;
    RaycastHit hit;

    const float CONST_PLAY_DISTANCE = 5f;

    [SerializeField] [Range(0, 2)] int handType;
    [SerializeField] [Range(0, 1)] int handState;
    [SerializeField] Transform handTypeHolder;

    [SerializeField] SpriteRenderer leftHandFrontSpriteRend;
    [SerializeField] SpriteRenderer leftHandBackSpriteRend;
    [SerializeField] Sprite[] leftHandFrontSprites;
    [SerializeField] Sprite[] leftHandBackSprites;

    void Awake()
    {
        GameManager.Instance.Player = player;
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
                playArea.Table.PlayCard(player, grabbedCard);
            }
            else
            {
                player.CardCollection.PlaceCard(grabbedCard);
                grabbedCard.State = CardState.InHand;
                EventManager.PlayCard();
            }
            
            grabbedCard = null;
        }


        if (grabbedCard) MoveTarget();

        MoveHand();
        UpdateHandVisuals();


        void HandleCardGrabbingAndHovering()
        {
            if (Physics.Raycast(mouseRay, out hit, CONST_PLAY_DISTANCE, interactMask) && hit.transform.TryGetComponent(out PlayableItem playable) && playable.Owner == PlayerType.Player && playable.IsInteractable)
            {
                Card card = playable as Card;

                if (card && card.State == CardState.OnTable) handState = 1;
                else handState = 0;

                if (Input.GetMouseButtonDown(1) && card)
                {
                    if (card.State == CardState.OnTable)
                    {
                        table.FreeSpotForCard(player, card);
                        player.CardCollection.PlaceCard(card);
                        card.State = CardState.InHand;
                    }
                    else if (card.State == CardState.InHand)
                    {
                        player.CardCollection.TakeCard(card);
                        table.PlayCard(player, card);
                        card.State = CardState.OnTable;
                    }
                    return;
                }
                else if (Input.GetMouseButtonDown(0) && card)
                {
                    hoveredPlayable?.EndHover();

                    if (table.CouponActive)
                    {
                        table.CouponActive = false;
                        table.RerollCard(card);
                        return;
                    }

                    grabbedCard = card;

                    if (grabbedCard.State == CardState.OnTable) table.FreeSpotForCard(player, card);
                    else if (grabbedCard.State == CardState.InHand)
                    {
                        player.CardCollection.TakeCard(card);
                        EventManager.PlayCard();
                    }
                    grabbedCard.State = CardState.Drag;
                }
                else if (!grabbedCard)
                {
                    // Card under mouse, didn't try to grab

                    if (hoveredPlayable != playable)
                    {
                        hoveredPlayable?.EndHover();

                        hoveredPlayable = playable;
                        hoveredPlayable.StartHover();
                    }
                }
            }
            else
            {
                if (!grabbedCard && hoveredPlayable)
                {
                    hoveredPlayable.EndHover();
                    hoveredPlayable = null;
                }

                handState = 0;
            }

            // Interacting
            if (!grabbedCard)
            {
                if (Physics.Raycast(mouseRay, out hit, CONST_PLAY_DISTANCE, interactMask) && hit.transform.TryGetComponent(out IInteractable interactable))
                {
                    if (hit.transform.TryGetComponent(out PlayableItem playableItem) && (playableItem.Owner == PlayerType.Dealer && !table.HookActive)) return;

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

            Transform targetTransform;
            if (hoveredPlayable is Item) targetTransform = /*grabbedCard?.transform ?? */hoveredInteractable;
            else targetTransform = grabbedCard?.transform ?? hoveredPlayable?.transform ?? hoveredInteractable;

            //Debug.Log($"target transform: {(targetTransform == null ? "null" : targetTransform.name + (targetTransform.parent == null ? "" : $", parent: {targetTransform.parent.name}"))}");
            //Debug.Log($"hovered is item: {hoveredPlayable is Item}");

            if (targetTransform)
            {
                Vector3 offset = hoveredPlayable != null && targetTransform == hoveredPlayable.transform && hoveredPlayable is Item ? Vector3.zero : cardHandOffset;
                targetPos = targetTransform.TransformPoint(offset);
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

        void UpdateHandVisuals()
        {
            for (int i = 0; i < handTypeHolder.childCount; i++)
            {
                GameObject child = handTypeHolder.GetChild(i).gameObject;
                if (i == handType)
                {
                    child.SetActive(true);
                    for (int j = 0; j < child.transform.childCount; j++)
                    {
                        child.transform.GetChild(j).gameObject.SetActive(j == handState);
                    }
                }
                else child.SetActive(false);
            }

            leftHandFrontSpriteRend.sprite = leftHandFrontSprites[handType];
            leftHandBackSpriteRend.sprite = leftHandBackSprites[handType];
        }
    }


    void UpdateHandType(Player player, int health)
    {
        if (player == null || player.IsDealer) return;
        handType = Mathf.Clamp(3 - health, 0, 2);
    }


    void OnEnable()
    {
        EventManager.OnPlayerDamaged += UpdateHandType;
    }

    void OnDisable()
    {
        EventManager.OnPlayerDamaged -= UpdateHandType;
    }
}
