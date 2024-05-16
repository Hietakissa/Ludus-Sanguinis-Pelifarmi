using UnityEngine;

public abstract class PlayableItem : MonoBehaviour
{
    public Transform TargetTransform { get; protected set; }

    [SerializeField] protected float posSmoothTime = 0.1f;
    [SerializeField] protected float rotateSmoothing = 12f;
    protected Vector3 posVel;
    protected Vector3 posOffset;


    public void SetTargetTransform(Transform target) => TargetTransform = target;
    public void InstaMoveToTarget()
    {
        transform.position = TargetTransform.position;
        transform.rotation = TargetTransform.rotation;
    }
}
