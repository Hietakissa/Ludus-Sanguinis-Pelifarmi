using System.Collections;
using HietakissaUtils;
using UnityEngine;
using TMPro;

public class Card : PlayableItem
{
    //[HideInInspector] public bool IsInteractable = true;
    [HideInInspector] public bool CanStartHover = true;

    [SerializeField] TextMeshPro debugText;
    [SerializeField] Vector3 valueTextOffset;
    [SerializeField] TextMeshPro valueText;

    //[field: SerializeField] public PlayerType Owner { get; private set; }
    //public Transform TargetTransform { get; private set; }
    public CardState State = CardState.InHand;

    public int Value { get; private set; } = 1;
    bool flip;


    //[SerializeField] float posSmoothTime = 0.1f;
    //[SerializeField] float rotateSmoothing = 0.1f;
    //[SerializeField] float scaleSmoothTime = 0.1f;
    //Vector3 startScale;
    //Vector3 targetScale;
    //Vector3 scaleVel;
    //Vector3 posVel;
    //Vector3 posOffset;

    Material backMat;
    Material frontMat;


    void Awake()
    {
        startScale = transform.localScale;
        targetScale = startScale;

        backMat = GetComponent<MeshRenderer>().materials[0];
        frontMat = GetComponent<MeshRenderer>().materials[1];
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.H)) Flip();

        if (debugText) debugText.text = $"Interactable: {IsInteractable}\nHoverable: {CanStartHover}\nTarget: {TargetTransform.name}";


        Vector3 targetPos = TargetTransform.position + posOffset;
        transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref posVel, posSmoothTime);

        Quaternion target = Quaternion.LookRotation(flip ? -TargetTransform.forward : TargetTransform.forward, TargetTransform.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, target, rotateSmoothing * Time.deltaTime);
        
        transform.localScale = Vector3.SmoothDamp(transform.localScale, targetScale, ref scaleVel, scaleSmoothTime);



        if (valueText.gameObject.activeSelf)
        {
            valueText.transform.forward = -Maf.Direction(transform.position, Camera.main.transform.position);
            valueText.transform.position = transform.position + valueTextOffset;
        }


        Debug.DrawRay(transform.position, TargetTransform.forward * 0.1f, Color.blue);
        Debug.DrawRay(transform.position, TargetTransform.up * 0.1f, Color.green);
        Debug.DrawRay(transform.position, TargetTransform.right * 0.1f, Color.red);
    }


    public override void StartHover()
    {
        targetScale = startScale * 1.1f;
        posOffset = transform.up * 0.04f;

        EventManager.HoverCard();
    }
    //public void EndHover()
    //{
    //    targetScale = startScale;
    //    posOffset = Vector3.zero;
    //}


    public void SetValue(int value)
    {
        Value = value;

        frontMat.SetFloat("_CardIndex", Value);

        int bloodIndex = Random.Range(0, GameManager.MAX_BLOOD_INDEX);
        frontMat.SetFloat("_BloodIndex", bloodIndex);
        backMat.SetFloat("_BloodIndex", bloodIndex);
    }

    public void SetRevealState(bool state)
    {
        valueText.gameObject.SetActive(state);
        if (state) valueText.text = Value.ToString();
    }

    public void Flip() => flip = !flip;
    public bool IsFlipped => flip;
}

public enum CardState
{
    InHand,
    OnTable,
    Drag
}
