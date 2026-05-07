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

    [Header("Bola")]
    public float mass = 1f;
    public Vector3 velocity = Vector3.zero;
    public float angularVelocity = 0f;

    private Transform ball;
    private Collider ballCollider;
    private float radius;

    private const float GRAVITY = 9.81f;

    public int maxBorderContacts = 2;
    public int borderContactCount = 0;

    public float airDensity = 1.225f;
    public float dragCoefficient = 0.47f;
    private float crossSection;
    private float heightAboveGround;

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

    void Start()
    {
        GameObject ballGO = GameObject.FindGameObjectWithTag("Ball");
        ball = ballGO.transform;
        ballCollider = ballGO.GetComponent<SphereCollider>();
        radius = ((SphereCollider)ballCollider).radius * Mathf.Max(ball.lossyScale.x, ball.lossyScale.y, ball.lossyScale.z);
        ballGO.layer = LayerMask.NameToLayer("Ignore Raycast");
        crossSection = Mathf.PI * radius * radius;
    }

    void FixedUpdate()
    {
        if (ball == null) return;

        float dt = Time.fixedDeltaTime;

        bool grounded = Physics.Raycast(ball.position + Vector3.up * radius, Vector3.down, out RaycastHit groundHit, radius * 4f);

        if (grounded)
            heightAboveGround = ball.position.y - groundHit.point.y;
        else
            heightAboveGround = float.MaxValue;

        bool inAir = !grounded || heightAboveGround > 1f;

        // Gravedad solo en el aire
        if (!grounded)
        {
            velocity.y -= GRAVITY * dt;
        }

        // Resistencia del aire
        if (inAir && velocity.magnitude > 0.001f)
        {
            float speed = velocity.magnitude;
            float fDrag = 0.5f * airDensity * speed * speed * dragCoefficient * crossSection;
            Vector3 dragAcc = -velocity.normalized * (fDrag / mass);
            velocity += dragAcc * dt;
        }

        float slopeAngle = 0f;

        if (grounded)
        {
            if (groundHit.collider.CompareTag("Cesped")) currentTerrain = TerrainType.Cesped;
            else if (groundHit.collider.CompareTag("Hielo")) currentTerrain = TerrainType.Hielo;
            else if (groundHit.collider.CompareTag("Arena")) currentTerrain = TerrainType.Arena;

            slopeAngle = Vector3.Angle(groundHit.normal, Vector3.up);
            float theta = slopeAngle * Mathf.Deg2Rad;
            float mu = GetFrictionCoeff();

            if (slopeAngle > 1f)
            {
                // --- Plano inclinado ---
                float fNormal = mass * GRAVITY * Mathf.Cos(theta);
                float fParallel = mass * GRAVITY * Mathf.Sin(theta);
                float fFriction = mu * fNormal;
                Vector3 slopeDir = Vector3.ProjectOnPlane(Vector3.down, groundHit.normal).normalized;

                if (velocity.magnitude > 0.05f)
                {
                    // Bola en movimiento: Fparallel + friccion opuesta al movimiento
                    velocity += slopeDir * (fParallel / mass) * dt;

                    Vector3 velOnPlane = Vector3.ProjectOnPlane(velocity, groundHit.normal);
                    if (velOnPlane.magnitude > 0.001f)
                    {
                        Vector3 frictionAcc = -velOnPlane.normalized * (fFriction / mass) * dt;
                        if (frictionAcc.magnitude >= velOnPlane.magnitude)
                            velocity = Vector3.zero;
                        else
                            velocity += frictionAcc;
                    }
                }
                else
                {
                    // Bola casi parada: solo se mueve si Fparallel supera friccion estatica
                    if (fParallel > fFriction)
                    {
                        velocity += slopeDir * ((fParallel - fFriction) / mass) * dt;
                    }
                }
            }
            else
            {
                // --- Terreno plano: friccion de rodadura igual que el original ---
                if (velocity.magnitude > 0.05f)
                {
                    float fNormal = mass * GRAVITY;
                    float torque = -mu * fNormal * radius;           // τ = -µ·Fnormal·r
                    float inertia = (2f / 5f) * mass * radius * radius; // I = 2/5·mr²
                    float alpha = torque / inertia;
                    float frictionAcc = alpha * radius;              // a = α·r

                    Vector3 flatVel = new Vector3(velocity.x, 0f, velocity.z);
                    angularVelocity = flatVel.magnitude / radius;    // ω = v/r

                    if (flatVel.magnitude > 0.001f)
                    {
                        Vector3 frictionForce = flatVel.normalized * frictionAcc * dt;
                        if (frictionForce.magnitude >= flatVel.magnitude)
                            velocity = new Vector3(0f, velocity.y, 0f);
                        else
                            velocity += frictionForce;
                    }
                }
            }
        }

        float stopThreshold = slopeAngle > 1f ? 0.01f : 0.15f;
        if (velocity.magnitude < stopThreshold && slopeAngle < 2f)
        {
            velocity = Vector3.zero;
            angularVelocity = 0f;
        }

        ResolveWallCollisions(dt);

        ball.position += velocity * dt;

        // Snap al suelo
        grounded = Physics.Raycast(ball.position + Vector3.up * radius, Vector3.down, out groundHit, radius * 4f);

        if (grounded && velocity.y < 0.1f)
        {
            ball.position = groundHit.point + Vector3.up * radius;
            if (slopeAngle < 2f)
                velocity.y = 0f;
            else
                velocity = Vector3.ProjectOnPlane(velocity, groundHit.normal);
        }
    }

    void ResolveWallCollisions(float dt)
    {
        if (velocity.magnitude < 0.001f) return;

        Vector3 moveDir = velocity.normalized;
        float moveDist = velocity.magnitude * dt;
        int layerMask = ~(1 << LayerMask.NameToLayer("Ignore Raycast"));

        RaycastHit[] hits = Physics.SphereCastAll(ball.position, radius, moveDir, moveDist, layerMask);

        foreach (RaycastHit wallHit in hits)
        {
            if (!wallHit.collider.CompareTag("Border") &&
                !wallHit.collider.CompareTag("Goma") &&
                !wallHit.collider.CompareTag("SacoArena"))
                continue;

            Vector3 n = wallHit.normal;
            float vDotN = Vector3.Dot(velocity, n);
            if (vDotN >= 0f) continue;

            float e = GetRestitution(wallHit.collider);
            velocity = velocity - (1f + e) * vDotN * n;

            Vector3 vParallel = velocity - Vector3.Dot(velocity, n) * n;
            velocity -= vParallel * 0.1f;

            if (wallHit.collider.CompareTag("Border"))
                borderContactCount++;

            if (velocity.magnitude < 0.15f)
            {
                velocity = Vector3.zero;
                angularVelocity = 0f;
            }
        }
    }

    float GetRestitution(Collider col)
    {
        if (col.CompareTag("Goma")) return 0.8f;
        if (col.CompareTag("SacoArena")) return 0.2f;
        return 0.4f;
    }

    public void ApplyImpulse(Vector3 impulse)
    {
        velocity += impulse / mass;
    }
}