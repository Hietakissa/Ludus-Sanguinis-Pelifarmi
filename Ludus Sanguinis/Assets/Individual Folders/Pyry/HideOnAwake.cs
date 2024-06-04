using UnityEngine;

public class HideOnAwake : MonoBehaviour
{
    void Awake()
    {
        gameObject.SetActive(false);
    }
}
