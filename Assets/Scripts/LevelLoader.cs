using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour
{
    [Header("Meta")]
    public Transform hole;
    public float holeRadius = 0.8f;

    [Header("Condiciones de victoria")]
    public int minBorderContacts = 2;
    public float maxSpeedAtHole = 0.5f;

    [Header("UI Feedback (opcional)")]
    public GameObject winPanel;

    private bool levelCompleted = false;
    private bool ballInHole = false;
    private Transform ball;

    void Start()
    {
        ball = GameObject.FindGameObjectWithTag("Ball")?.transform;
        PhysicsManager.Instance.borderContactCount = 0;
        if (winPanel != null) winPanel.SetActive(false);
    }

    void Update()
    {
        if (levelCompleted || ball == null || hole == null) return;

        PhysicsManager pm = PhysicsManager.Instance;

        Vector3 ballFlat = new Vector3(ball.position.x, 0f, ball.position.z);
        Vector3 holeFlat = new Vector3(hole.position.x, 0f, hole.position.z);
        bool inHole = Vector3.Distance(ballFlat, holeFlat) < holeRadius;

        if (inHole && !ballInHole) ballInHole = true;
        if (!inHole) ballInHole = false;

        bool ballStopped = pm.velocity == Vector3.zero;

        if (ballInHole && ballStopped)
        {
            bool slowEnough = pm.velocity.magnitude < maxSpeedAtHole;
            bool enoughBounces = pm.borderContactCount >= minBorderContacts;

            if (slowEnough && enoughBounces)
            {
                levelCompleted = true;             
                if (winPanel != null) winPanel.SetActive(true);
                Invoke(nameof(LoadNextLevel), 1.5f);
            }
            
        }
    }

    void LoadNextLevel()
    {
        string current = SceneManager.GetActiveScene().name;
        if (current == "Level_1") SceneManager.LoadScene("Level_2");
        else if (current == "Level_2") SceneManager.LoadScene("Level_3");
        else SceneManager.LoadScene("MainMenu");
    }
}