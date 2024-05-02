using HietakissaUtils;
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
            if (Physics.Raycast(mouseRay, out RaycastHit hit, 10f, interactMask) && hit.transform.TryGetComponent(out Card card))
            {
                grabbedCard = card;
            }
        }
        else if (Input.GetMouseButtonUp(0)) grabbedCard = null;


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
}
