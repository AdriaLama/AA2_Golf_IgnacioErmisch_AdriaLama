using UnityEngine;

public class PhysicsManager : MonoBehaviour
{
    public static PhysicsManager Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public enum TerrainType { Cesped, Hielo, Arena }
    public TerrainType currentTerrain = TerrainType.Cesped;

    float GetFrictionCoeff()
    {
        switch (currentTerrain)
        {
            case TerrainType.Cesped: return 0.4f;
            case TerrainType.Hielo: return 0.1f;
            case TerrainType.Arena: return 0.6f;
            default: return 0.4f;
        }
    }

    [Header("Bola")]
    public float mass = 1f;
    public Vector3 velocity = Vector3.zero;
    public float angularVelocity = 0f;

    private Transform ball;
    private Collider ballCollider;
    private float radius;

    private const float GRAVITY = 9.81f;

    [Header("Colisiones con paredes")]
    public float wallCastSkin = 0.02f;
    public int maxBorderContacts = 2;

    [HideInInspector] public int borderContactCount = 0;

    void Start()
    {
        GameObject ballGO = GameObject.FindGameObjectWithTag("Ball");
        ball = ballGO.transform;
        ballCollider = ballGO.GetComponent<SphereCollider>();
        radius = ((SphereCollider)ballCollider).radius
                       * Mathf.Max(ball.lossyScale.x, ball.lossyScale.y, ball.lossyScale.z);
        ballGO.layer = LayerMask.NameToLayer("Ignore Raycast");
    }

    void FixedUpdate()
    {
        if (ball == null) return;

        float dt = Time.fixedDeltaTime;

        velocity.y -= GRAVITY * dt;

        bool grounded = Physics.Raycast(
            ball.position + Vector3.up * radius,
            Vector3.down,
            out RaycastHit groundHit,
            radius * 2f);

        if (grounded)
        {
            if (groundHit.collider.CompareTag("Cesped")) currentTerrain = TerrainType.Cesped;
            else if (groundHit.collider.CompareTag("Hielo")) currentTerrain = TerrainType.Hielo;
            else if (groundHit.collider.CompareTag("Arena")) currentTerrain = TerrainType.Arena;
        }

        if (grounded && velocity.magnitude > 0.05f)
        {
            float mu = GetFrictionCoeff();
            float fNormal = mass * GRAVITY;
            float torque = -mu * fNormal * radius;
            float inertia = (2f / 5f) * mass * radius * radius;
            float alpha = torque / inertia;
            float frictionAcc = alpha * radius;
            Vector3 flatVel = new Vector3(velocity.x, 0f, velocity.z);
            angularVelocity = flatVel.magnitude / radius;

            if (flatVel.magnitude > 0.001f)
                velocity += flatVel.normalized * frictionAcc * dt;
        }

        if (velocity.magnitude < 0.05f)
        {
            velocity = Vector3.zero;
            angularVelocity = 0f;
        }

        ResolveWallCollisions(dt);

        ball.position += velocity * dt;

        grounded = Physics.Raycast(
            ball.position + Vector3.up * radius,
            Vector3.down,
            out groundHit,
            radius * 2f);

        if (grounded && velocity.y < 0f)
        {
            ball.position = groundHit.point + Vector3.up * radius;
            velocity.y = 0f;
        }
    }

    void ResolveWallCollisions(float dt)
    {
        if (velocity.magnitude < 0.001f) return;

        Vector3 moveDir = velocity.normalized;
        float moveDist = velocity.magnitude * dt;
        int layerMask = ~(1 << LayerMask.NameToLayer("Ignore Raycast"));

        if (Physics.SphereCast(
                ball.position,
                radius + wallCastSkin,
                moveDir,
                out RaycastHit wallHit,
                moveDist + wallCastSkin,
                layerMask))
        {
            Vector3 n = wallHit.normal;
            if (Mathf.Abs(n.y) > 0.85f) return;

            float e = GetRestitution(wallHit.collider);
            float vDotN = Vector3.Dot(velocity, n);
            velocity = velocity - (1f + e) * vDotN * n;

            float penetration = (radius + wallCastSkin) - wallHit.distance;
            if (penetration > 0f)
                ball.position += n * penetration;

            if (wallHit.collider.CompareTag("Border"))
            {
                borderContactCount++;
                if (borderContactCount > maxBorderContacts)
                    Debug.Log("[PhysicsManager] Límite de contactos con el borde superado.");
            }
        }
    }

    float GetRestitution(Collider col)
    {
        if (col.CompareTag("Goma")) return 0.8f;
        if (col.CompareTag("Arena")) return 0.2f;
        return 0.6f;
    }

    public void ApplyImpulse(Vector3 impulse)
    {
        velocity += impulse / mass;
    }
}