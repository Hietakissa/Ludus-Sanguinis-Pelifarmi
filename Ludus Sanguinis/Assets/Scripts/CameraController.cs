using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] float rotationSpeed = 15f;
    [SerializeField] float rotationAmount = 5f;
    Quaternion startRot;


    void Awake()
    {
        startRot = transform.rotation;
    }

    void Update()
    {
        float vertical = (Mathf.Clamp(Input.mousePosition.y, 0f, Screen.height) / Screen.height * 2) - 1f; 
        float horizontal = (Mathf.Clamp(Input.mousePosition.x, 0f, Screen.width) / Screen.width * 2) - 1f;

        Quaternion rot = startRot * Quaternion.Euler(Vector3.up * horizontal * rotationAmount);
        rot *= Quaternion.Euler(Vector3.left * vertical * rotationAmount);
        
        transform.rotation = Quaternion.Slerp(transform.rotation, rot, rotationSpeed * Time.deltaTime);
    }
}
