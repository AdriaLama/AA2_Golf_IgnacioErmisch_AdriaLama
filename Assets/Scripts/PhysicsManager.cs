using UnityEngine;

public class PhysicsManager : MonoBehaviour
{
    public static PhysicsManager Instance { get; private set; }
    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    //Tipos de terreno
    public enum TerrainType { 
        Cesped, 
        Hielo, 
        Arena 
    }

    public TerrainType currentTerrain = TerrainType.Cesped;

    float GetFrictionCoeff()
    {
        switch (currentTerrain)
        {
            case TerrainType.Cesped: 
                return 0.4f;
            case TerrainType.Hielo: 
                return 0.1f;
            case TerrainType.Arena: 
                return 0.6f;
            default: 
                return 0.4f;
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

    void Start()
    {
        GameObject ballGO = GameObject.FindGameObjectWithTag("Ball");
        ball = ballGO.transform;
        ballCollider = ballGO.GetComponent<SphereCollider>();
        radius = ((SphereCollider)ballCollider).radius * Mathf.Max(ball.lossyScale.x, ball.lossyScale.y, ball.lossyScale.z);

        ballGO.layer = LayerMask.NameToLayer("Ignore Raycast");

    }

    void FixedUpdate()
    {
        if (ball == null) return;

        float dt = Time.fixedDeltaTime;

        velocity.y -= GRAVITY * dt;

        bool grounded = Physics.Raycast(ball.position + Vector3.up * radius, Vector3.down, out RaycastHit hit, radius * 2f);

        if (grounded)
        {
            if (hit.collider.CompareTag("Cesped"))
                currentTerrain = TerrainType.Cesped;
            else if (hit.collider.CompareTag("Hielo"))
                currentTerrain = TerrainType.Hielo;
            else if (hit.collider.CompareTag("Arena"))
                currentTerrain = TerrainType.Arena;
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

        ball.position += velocity * dt;

        grounded = Physics.Raycast(ball.position + Vector3.up * radius,Vector3.down,out hit,radius * 2f);

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