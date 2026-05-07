using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Objetivo")]
    public Transform target;

    public float sensitivity = 3f;
    public float distance = 2.5f;
    public float minY = -10f; 
    public float maxY = 60f;

    public float zoomSpeed = 3f;
    public float minDistance = 1f;
    public float maxDistance = 5f;

    private float currentX = 0f;
    private float currentY = 20f;

    void Update()
    {
        if (Input.GetMouseButton(1))
        {
            currentX += Input.GetAxis("Mouse X") * sensitivity;
            currentY -= Input.GetAxis("Mouse Y") * sensitivity;
            currentY = Mathf.Clamp(currentY, minY, maxY);
        }

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0f)
        {
            distance -= scroll * zoomSpeed;
            distance = Mathf.Clamp(distance, minDistance, maxDistance);
        }
    }

    void LateUpdate()
    {
        if (target == null) return;

        Quaternion rotation = Quaternion.Euler(currentY, currentX, 0f);
        transform.position = target.position + rotation * new Vector3(0f, 0f, -distance);
        transform.LookAt(target.position);
    }
}