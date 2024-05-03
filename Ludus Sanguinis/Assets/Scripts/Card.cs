using System.Collections;
using HietakissaUtils;

using TMPro;

using UnityEngine;

public class Card : MonoBehaviour
{
    public Vector3 StartPos { get; private set; }
    public Vector3 StartForward { get; private set; }
    public Vector3 StartUp { get; private set; }

    [HideInInspector] public bool IsInteractable = true;
    [HideInInspector] public bool IsHoverable = true;

    Vector3 startScale;

    [SerializeField] TextMeshPro debugText;

    void Awake()
    {
        StartPos = transform.position;
        StartForward = transform.forward;
        StartUp = transform.up;

        startScale = transform.localScale;
    }

    void Update()
    {
        if (debugText) debugText.text = $"Interactable: {IsInteractable}\nHoverable: {IsHoverable}";
    }


    public void ForceStopAnimations()
    {
        IsInteractable = true;
        StopAllCoroutines();
    }

    public void MoveToTransform(Transform t)
    {
        StartCoroutine(MoveToTransformCor(t));
    }

    public void StartHover()
    {
        Debug.Log($"start hover");
        StopAllCoroutines();
        StartCoroutine(AnimateScaleAndPosCor(startScale * 1.1f, StartPos + (transform.up * 0.05f)));

        IsHoverable = false;
    }

    public void EndHover()
    {
        StopAllCoroutines();
        StartCoroutine(AnimateScaleAndPosCor(startScale, StartPos));

        IsHoverable = true;
    }

    IEnumerator AnimateScaleAndPosCor(Vector3 targetScale, Vector3 targetPos)
    {
        Vector3 lerpStartScale = transform.localScale;
        Vector3 lerpStartPos = transform.position;
        float t = 0f;

        while (true)
        {
            t += Time.deltaTime * 2f;
            float ease = Maf.Easing.EaseOutCubic(t);

            //Debug.Log($"hover coroutine, t: {t}");

            transform.localScale = Vector3.Lerp(lerpStartScale, targetScale, ease);
            //transform.position = Vector3.Lerp(lerpStartPos, targetPos, ease);
            

            if (t >= 1f) break;
            else yield return null;
        }
    }

    IEnumerator MoveToTransformCor(Transform transform)
    {
        IsInteractable = false;
        IsHoverable = false;

        float t = 0f;

        Vector3 from = this.transform.position;
        Vector3 to = transform.position;

        Quaternion startRot = this.transform.rotation;

        while (true)
        {
            t += Time.deltaTime;
            float ease = Maf.Easing.EaseOutCubic(t);

            Debug.Log($"moving to transform cor, t: {t}");

            this.transform.position = Vector3.Lerp(from, to, ease);

            Quaternion target = Quaternion.LookRotation(transform.forward, transform.up);
            this.transform.rotation = Quaternion.Slerp(startRot, target, ease);

            if (t >= 1f) break;
            else yield return null;
        }

        IsInteractable = true;
    }
}
