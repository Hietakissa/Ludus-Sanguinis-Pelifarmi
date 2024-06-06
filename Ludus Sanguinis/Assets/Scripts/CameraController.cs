using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] float rotationSpeed = 15f;
    [SerializeField] float rotationAmount = 5f;
    Quaternion startRot;

    Camera cam;
    [SerializeField] float baseFOV;
    [SerializeField] float zoomFOV;
    [SerializeField] float zoomSpeed;
    float targetFOV;


    void Awake()
    {
        startRot = transform.rotation;
        cam = GetComponent<Camera>();
    }

    void Update()
    {
        float xOffset = Mathf.Cos(Time.time * 0.25f) * 20f;
        float yOffset = Mathf.Sin(Time.time * 0.4f) * 65f;
        float vertical = (Mathf.Clamp(Input.mousePosition.y + yOffset, 0f, Screen.height) / Screen.height * 2) - 1f; 
        float horizontal = (Mathf.Clamp(Input.mousePosition.x + xOffset, 0f, Screen.width) / Screen.width * 2) - 1f;

        bool zooming = Input.GetKey(KeyCode.F) && !string.IsNullOrEmpty(GameManager.Instance.PlayerName);
        float zoomMultiplier = zooming ? 3f : 1f;

        Quaternion rot = startRot * Quaternion.Euler(Vector3.up * horizontal * rotationAmount * zoomMultiplier);
        rot *= Quaternion.Euler(Vector3.left * vertical * rotationAmount * zoomMultiplier);
        
        transform.localRotation = Quaternion.Slerp(transform.localRotation, rot, rotationSpeed * Time.deltaTime);


        targetFOV = zooming ? zoomFOV : baseFOV;
        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFOV, zoomSpeed * Time.deltaTime);
    }
}
