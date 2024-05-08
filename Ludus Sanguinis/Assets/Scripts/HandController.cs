using System;
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
    [SerializeField] Transform hand;
    [SerializeField] Vector3 cardHandOffset;
    [SerializeField] float handMoveTime = 0.1f;
    [SerializeField] float handRotSpeed = 12f;

    Table table;
    Vector3 cardVelocity;
    Vector3 handVelocity;

    Card hoveredCard;
    Card grabbedCard;

    Plane viewPlane;

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
        //else
        //{
        //    hand.rotation = Quaternion.LookRotation(-mouseRay.direction, transform.up);
        //    hand.position = mouseRay.origin + mouseRay.direction * cardDistanceOffset + hand.InverseTransformPoint(idleHandOffset);
        //}

        MoveHand();


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
                //Plane viewPlane = new Plane(-cam.transform.forward, cam.transform.position.magnitude + cardDistanceOffset);
                //viewPlane.SetNormalAndPosition(cam.transform.forward, cam.transform.forward * cardDistanceOffset);
                //if (viewPlane.Raycast(mouseRay, out float distance))
                //{
                //    targetPos = mouseRay.origin + mouseRay.direction * distance;
                //    targetDir = Maf.Direction(grabbedCard.transform.position, cam.transform.position);
                //}

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

            Card targetCard = grabbedCard ?? hoveredCard;

            if (targetCard)
            {
                //hand.position = /*grabbedCard.transform.position +*/ grabbedCard.transform.TransformPoint(cardHandOffset);
                //hand.rotation = Quaternion.LookRotation(-grabbedCard.transform.forward, grabbedCard.transform.up);
                targetPos = targetCard.transform.TransformPoint(cardHandOffset);
                targetRot = Quaternion.LookRotation(-targetCard.transform.forward, targetCard.transform.up);
            }
            else
            {
                //hand.position = mouseRay.origin + mouseRay.direction * cardDistanceOffset;
                targetPos = mouseRay.origin + mouseRay.direction * cardDistanceOffset;
                //hand.transform.forward = mouseRay.direction;
                //hand.transform.rotation = Quaternion.LookRotation(mouseRay.direction);
                targetRot = Quaternion.LookRotation(mouseRay.direction);
                moveTime = handMoveTime;
            }

            //hand.position = targetPos;
            hand.position = Vector3.SmoothDamp(hand.position, targetPos, ref handVelocity, moveTime);
            //hand.rotation = targetRot;
            hand.rotation = Quaternion.Slerp(hand.rotation, targetRot, handRotSpeed * Time.deltaTime);
        }
    }
}
