using System.Collections;
using HietakissaUtils;
using UnityEngine;
using TMPro;

public class Card : MonoBehaviour
{
    public Vector3 StartPos { get; private set; }
    public Vector3 StartForward { get; private set; }
    public Vector3 StartUp { get; private set; }

    [HideInInspector] public bool IsInteractable = true;
    [HideInInspector] public bool CanStartHover = true;


    [SerializeField] TextMeshPro debugText;

    public Transform StartTargetTransform { get; private set; }
    public Transform TargetTransform { get; private set; }
    Vector3 posOffset;

    Vector3 startScale;
    Vector3 targetScale;
    Vector3 scaleVel;
    Vector3 posVel;
    [SerializeField] float posSmoothTime = 0.1f;
    [SerializeField] float rotateSmoothing = 0.1f;
    [SerializeField] float scaleSmoothTime = 0.1f;

    public CardState State = CardState.InHand;

    void Awake()
    {
        StartPos = transform.position;
        StartForward = transform.forward;
        StartUp = transform.up;

        startScale = transform.localScale;
        targetScale = startScale;
    }

    void Start()
    {
        if (TargetTransform)
        {
            StartTargetTransform = TargetTransform;

            transform.position = TargetTransform.position;
            transform.rotation = TargetTransform.rotation;
        }
    }

    void Update()
    {
        if (debugText) debugText.text = $"Interactable: {IsInteractable}\nHoverable: {CanStartHover}\nTarget: {TargetTransform.name}";


        Vector3 targetPos = TargetTransform.position + posOffset;
        transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref posVel, posSmoothTime);

        Quaternion target = Quaternion.LookRotation(TargetTransform.forward, TargetTransform.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, target, rotateSmoothing * Time.deltaTime);
        
        transform.localScale = Vector3.SmoothDamp(transform.localScale, targetScale, ref scaleVel, scaleSmoothTime);


        Debug.DrawRay(transform.position, TargetTransform.forward * 0.1f, Color.blue);
        Debug.DrawRay(transform.position, TargetTransform.up * 0.1f, Color.green);
        Debug.DrawRay(transform.position, TargetTransform.right * 0.1f, Color.red);
    }


    public void SetTargetTransform(Transform target) => TargetTransform = target;

    public void StartHover()
    {
        targetScale = startScale * 1.1f;
        //localOffset = -Vector3.forward * 0.05f + Vector3.up * 0.02f;
        posOffset = transform.up * 0.04f;
    }

    public void EndHover()
    {
        targetScale = startScale;
        posOffset = Vector3.zero;
    }
}

public enum CardState
{
    InHand,
    OnTable,
    Drag
}
