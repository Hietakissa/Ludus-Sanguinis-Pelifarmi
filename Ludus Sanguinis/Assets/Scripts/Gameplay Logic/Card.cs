using System.Collections;
using HietakissaUtils;
using UnityEngine;
using TMPro;

public class Card : MonoBehaviour
{
    [HideInInspector] public bool IsInteractable = true;
    [HideInInspector] public bool CanStartHover = true;

    [SerializeField] TextMeshPro debugText;

    [field: SerializeField] public PlayerType Owner { get; private set; }
    public Transform StartTargetTransform { get; private set; }
    public Transform TargetTransform { get; private set; }
    public CardState State = CardState.InHand;

    public int Value { get; private set; } = 1;


    [SerializeField] float posSmoothTime = 0.1f;
    [SerializeField] float rotateSmoothing = 0.1f;
    [SerializeField] float scaleSmoothTime = 0.1f;
    Vector3 startScale;
    Vector3 targetScale;
    Vector3 scaleVel;
    Vector3 posVel;
    Vector3 posOffset;

    Material backMat;
    Material frontMat;


    void Awake()
    {
        startScale = transform.localScale;
        targetScale = startScale;

        backMat = GetComponent<MeshRenderer>().materials[0];
        frontMat = GetComponent<MeshRenderer>().materials[1];
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
        posOffset = transform.up * 0.04f;
    }

    public void EndHover()
    {
        targetScale = startScale;
        posOffset = Vector3.zero;
    }


    public void SetValue(int value)
    {
        Value = value;

        frontMat.SetFloat("_CardIndex", Value);
        backMat.SetFloat("_BloodIndex", Random.Range(0, GameManager.MAX_BLOOD_INDEX));
        Debug.Log($"set card value to: {value}");
    }
}

public enum CardState
{
    InHand,
    OnTable,
    Drag
}
