using System.Collections;
using HietakissaUtils;
using UnityEngine;

public class Card : MonoBehaviour
{
    public Vector3 StartPos { get; private set; }
    public Vector3 StartForward { get; private set; }
    public Vector3 StartUp { get; private set; }

    [SerializeField] public bool PlayableByPlayer = true;

    Vector3 startScale;

    void Awake()
    {
        StartPos = transform.position;
        StartForward = transform.forward;
        StartUp = transform.up;

        startScale = transform.localScale;
    }


    public void ForceStopAnimations()
    {
        StopAllCoroutines();
    }

    public void StartHover()
    {
        StopAllCoroutines();
        StartCoroutine(AnimateScaleAndPosCor(startScale * 1.1f, StartPos + (transform.up * 0.05f)));
    }

    public void EndHover()
    {
        StopAllCoroutines();
        StartCoroutine(AnimateScaleAndPosCor(startScale, StartPos));
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

            transform.localScale = Vector3.Lerp(lerpStartScale, targetScale, ease);
            transform.position = Vector3.Lerp(lerpStartPos, targetPos, ease);
            

            if (t >= 1f) break;
            else yield return null;
        }
    }
}
