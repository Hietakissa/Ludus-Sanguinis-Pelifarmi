using System.Collections;
using HietakissaUtils;
using UnityEngine;

public class HandController : MonoBehaviour
{
    [SerializeField] Player player;

    [SerializeField] Camera cam;
    [SerializeField] LayerMask interactMask;
    [SerializeField] LayerMask alignMask;

    [SerializeField] float cardDistanceOffset;
    [SerializeField] float cardMoveSpeed;
    [SerializeField] float cardPointSpeed;

    [SerializeField] Transform target;

    Table table;
    Vector3 cardVelocity;

    Card hoveredCard;
    Card grabbedCard;

    const float CONST_PLAY_DISTANCE = 10f;


    void Awake()
    {
        for (int i = 0; i < player.CardCollection.CardPositions.Length; i++)
        {
            Card card = player.CardCollection.CardPositions[i].Card;
            card.SetTargetTransform(player.CardCollection.CardPositions[i].Transform);
        }
    }

    void Update()
    {
        Vector3 clampedMousePos = new Vector3(Mathf.Clamp(Input.mousePosition.x, 0f, Screen.width), Mathf.Clamp(Input.mousePosition.y, 0f, Screen.height), 0f);
        Ray mouseRay = cam.ScreenPointToRay(clampedMousePos);

        if (Physics.Raycast(mouseRay, out RaycastHit hit, CONST_PLAY_DISTANCE, interactMask) && hit.transform.TryGetComponent(out Card card))
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


        if (Input.GetMouseButtonUp(0) && grabbedCard)
        {
            if (Physics.Raycast(mouseRay, out hit, CONST_PLAY_DISTANCE, alignMask) && hit.transform.TryGetComponent(out table))
            {
                Debug.Log($"playing card...");
                player.CardCollection.TakeCard(grabbedCard);
                table.PlayCard(player, grabbedCard);
            }
            else
            {
                //grabbedCard.SetTargetTransform(grabbedCard.StartTargetTransform);
                player.CardCollection.PlaceCard(grabbedCard);
                grabbedCard.State = CardState.InHand;
            }
            
            grabbedCard = null;
        }


        if (grabbedCard) MoveTarget();

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
                Plane viewPlane = new Plane(-cam.transform.forward, cam.transform.position.magnitude + cardDistanceOffset);
                if (viewPlane.Raycast(mouseRay, out float distance))
                {
                    targetPos = mouseRay.origin + mouseRay.direction * distance;
                    targetDir = Maf.Direction(grabbedCard.transform.position, cam.transform.position);
                }
            }

            target.position = targetPos;
            target.forward = -mouseRay.direction;
            grabbedCard.SetTargetTransform(target);

            target.rotation = Quaternion.LookRotation(targetDir, cam.transform.forward.SetY(target.position.y));


            //grabbedCard.transform.position = Vector3.SmoothDamp(grabbedCard.transform.position, targetPos, ref cardVelocity, cardMoveSpeed);
            //Quaternion targetRot = Quaternion.LookRotation(targetDir, hitSurface ? cam.transform.forward.SetY(grabbedCard.transform.position.y) : Vector3.up);
            //target.rotation = Quaternion.Slerp(target.rotation, targetRot, cardPointSpeed * Time.deltaTime);
        }
    }
}
