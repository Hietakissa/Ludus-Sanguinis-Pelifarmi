using UnityEngine;

public abstract class PlayableItem : MonoBehaviour
{
    [HideInInspector] public bool IsInteractable = true;
    [field: SerializeField] public PlayerType Owner { get; private set; }
    public Transform TargetTransform { get; protected set; }

    [SerializeField] protected float posSmoothTime = 0.1f;
    [SerializeField] protected float rotateSmoothing = 12f;
    [SerializeField] protected float scaleSmoothTime = 0.1f;
    protected Vector3 posVel;
    protected Vector3 posOffset;
    protected Vector3 startScale;
    protected Vector3 targetScale;
    protected Vector3 scaleVel;
    
    public virtual Transform VisualTransform => transform;

    public void SetTargetTransform(Transform target) => TargetTransform = target;
    public void InstaMoveToTarget()
    {
        transform.position = TargetTransform.position;
        transform.rotation = TargetTransform.rotation;
    }

    public virtual void StartHover()
    {
        targetScale = startScale * 1.1f;
        posOffset = transform.up * 0.04f;
    }
    public virtual void EndHover()
    {
        targetScale = startScale;
        posOffset = Vector3.zero;
    }
}
