using HietakissaUtils;

using System;
using System.Collections;

using UnityEngine;
public class HandController : MonoBehaviour
{
    [SerializeField] Camera cam;
    [SerializeField] LayerMask interactMask;
    [SerializeField] LayerMask alignMask;

    [SerializeField] float cardDistanceOffset;
    [SerializeField] float cardMoveSpeed;
    [SerializeField] float cardPointSpeed;


    Vector3 cardVelocity;
    Vector3 cardDirVelocity;

    Card grabbedCard;

    void Update()
    {
        Vector3 clampedMousePos = new Vector3(Mathf.Clamp(Input.mousePosition.x, 0f, Screen.width), Mathf.Clamp(Input.mousePosition.y, 0f, Screen.height), 0f);
        //Ray mouseRay = cam.ScreenPointToRay(Input.mousePosition);
        Ray mouseRay = cam.ScreenPointToRay(clampedMousePos);

        if (Input.GetMouseButtonDown(0))
        {
            if (Physics.Raycast(mouseRay, out RaycastHit hit, 10f, interactMask) && hit.transform.TryGetComponent(out Card card) && card.PlayableByPlayer)
            {
                grabbedCard = card;
            }
        }
        else if (Input.GetMouseButtonUp(0) && grabbedCard)
        {
            StartCoroutine(MoveCardToStartPosCor(grabbedCard));
            grabbedCard = null;
        }


        if (grabbedCard)
        {
            Vector3 targetPos = Vector3.zero;
            Vector3 targetDir = Vector3.zero;

            bool hitSurface = false;

            if (Physics.Raycast(mouseRay, out RaycastHit hit, 3f, alignMask))
            {
                targetPos = hit.point + hit.normal * 0.01f;
                targetDir = hit.normal;

                hitSurface = true;
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

            grabbedCard.transform.position = Vector3.SmoothDamp(grabbedCard.transform.position, targetPos, ref cardVelocity, cardMoveSpeed);

            Quaternion target = Quaternion.LookRotation(targetDir, hitSurface ? cam.transform.forward.SetY(grabbedCard.transform.position.y) : Vector3.up);
            grabbedCard.transform.rotation = Quaternion.Slerp(grabbedCard.transform.rotation, target, cardPointSpeed * Time.deltaTime);
        }
    }

    IEnumerator MoveCardToStartPosCor(Card card)
    {
        card.PlayableByPlayer = false;

        float t = 0f;

        Vector3 from = card.transform.position;
        Vector3 to = card.StartPos;

        Quaternion startRot = card.transform.rotation;

        Vector3 velocity = Vector3.zero;
        Vector3 rotVelocity = Vector3.zero;

        //while (Vector3.Distance(card.transform.position, to) > 0.05f)
        while (true)
        {
            t += Time.deltaTime;
            float ease = EaseOutCubic(t);

            //card.transform.position = Vector3.Lerp(from, to, t);
            //card.transform.position = Vector3.SmoothDamp(card.transform.position, to, ref velocity, 0.4f);
            card.transform.position = Vector3.Slerp(from, to, ease);

            Quaternion target = Quaternion.LookRotation(card.StartForward, card.StartUp);
            card.transform.rotation = Quaternion.Slerp(startRot, target, ease);

            if (t >= 1f) break;
            else yield return null;
        }

        card.PlayableByPlayer = true;
    }

    float EaseOutCubic(float t)
    {
        return 1f - Mathf.Pow(1f - t, 3f);
    }
     //function easeOutCubic(x: number): number
     //{
     //return 1 - Math.pow(1 - x, 3);
     //}
}
