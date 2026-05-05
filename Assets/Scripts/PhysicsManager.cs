using UnityEngine;

public class PhysicsManager : MonoBehaviour
{
    public static PhysicsManager Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public float mass = 0.05f;
    public Vector3 velocity = Vector3.zero;

    private Transform ball;
    private Collider ballCollider;
    private float radius;

    private float gravity = 9.81f;

    void Start()
    {
        GameObject ballGO = GameObject.FindGameObjectWithTag("Ball");
        ball = ballGO.transform;
        ballCollider = ballGO.GetComponent<SphereCollider>();
        radius = ((SphereCollider)ballCollider).radius * Mathf.Max(ball.lossyScale.x, ball.lossyScale.y, ball.lossyScale.z);
    }

    void FixedUpdate()
    {
        if (ball == null) return;

        velocity.y -= gravity * Time.fixedDeltaTime;
        ball.position += velocity * Time.fixedDeltaTime;

        ballCollider.enabled = false;
        bool grounded = Physics.Raycast(ball.position + Vector3.up * radius, Vector3.down, out RaycastHit hit, radius * 2f);
        ballCollider.enabled = true;

        if (grounded && velocity.y < 0f)
        {
            ball.position = hit.point + Vector3.up * radius;
            velocity.y = 0f;
        }
    }

    public void ApplyImpulse(Vector3 impulse)
    {
        velocity += impulse / mass;
    }
}