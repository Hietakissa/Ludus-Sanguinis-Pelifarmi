using UnityEngine;

public class Card : MonoBehaviour
{
    [field: SerializeField] public Vector3 StartPos { get; private set; }
    [field: SerializeField] public Vector3 StartForward { get; private set; }
    [field: SerializeField] public Vector3 StartUp { get; private set; }
    [SerializeField] public bool PlayableByPlayer = true;

    void Awake()
    {
        StartPos = transform.position;
        StartForward = transform.forward;
        StartUp = transform.up;
    }
}
