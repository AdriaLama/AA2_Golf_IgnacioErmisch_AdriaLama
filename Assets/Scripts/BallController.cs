using UnityEngine;

public class BallController : MonoBehaviour
{
    [Header("Fuerza")]
    public float maxForce;
    public float maxDragDistance;

    private float accumulatedDrag = 0f; 
    private bool isDragging = false;
    private LineRenderer line;

    void Start()
    {
        line = gameObject.AddComponent<LineRenderer>();
        line.startWidth = 0.05f;
        line.endWidth = 0.05f;
        line.positionCount = 2;
        line.enabled = false;
        line.material = new Material(Shader.Find("Sprites/Default"));
    }

    void Update()
    {
        if (PhysicsManager.Instance.velocity.magnitude > 0.1f) 
            return;

        if (Input.GetMouseButtonDown(0))
        {
            accumulatedDrag = 0f;
            isDragging = true;
        }

        if (isDragging && Input.GetMouseButton(0))
        {
            accumulatedDrag += -Input.GetAxis("Mouse Y") * 1.5f;
            accumulatedDrag = Mathf.Clamp(accumulatedDrag, 0f, maxDragDistance);

            float force = Mathf.Clamp01(accumulatedDrag / maxDragDistance) * maxForce;
            float t = force / maxForce;
            Vector3 direction = GetShootDirection();

            Color color = Color.Lerp(Color.green, Color.red, t);
            line.startColor = color;
            line.endColor = color;

            line.enabled = true;
            line.SetPosition(0, transform.position);
            line.SetPosition(1, transform.position + direction * (force / maxForce) * 2f);
        }

        if (Input.GetMouseButtonUp(0) && isDragging)
        {
            isDragging = false;
            line.enabled = false;

            float force = Mathf.Clamp01(accumulatedDrag / maxDragDistance) * maxForce;
            Vector3 direction = GetShootDirection();

            PhysicsManager.Instance.ApplyImpulse(direction * force);
        }
    }

    Vector3 GetShootDirection()
    {
        Vector3 camForward = Camera.main.transform.forward;
        camForward.y = 0f;
        return camForward.normalized;
    }
}