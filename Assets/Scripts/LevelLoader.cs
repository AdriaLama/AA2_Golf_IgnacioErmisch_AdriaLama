using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour
{
    [Header("Meta")]
    public Transform hole;
    public float holeRadius = 0.8f;

    private bool levelCompleted = false;
    private Transform ball;

    void Start()
    {
        ball = GameObject.FindGameObjectWithTag("Ball")?.transform;
        PhysicsManager.Instance.borderContactCount = 0;
    }

    void Update()
    {
        if (levelCompleted || ball == null || hole == null) return;

        PhysicsManager pm = PhysicsManager.Instance;

        Vector3 ballFlat = new Vector3(ball.position.x, 0f, ball.position.z);
        Vector3 holeFlat = new Vector3(hole.position.x, 0f, hole.position.z);
        float dist = Vector3.Distance(ballFlat, holeFlat);

        bool inHole = dist < holeRadius;
        bool slowEnough = pm.velocity.magnitude < 0.5f;
        bool borderOk = pm.borderContactCount <= pm.maxBorderContacts;
        bool ballStopped = pm.velocity.magnitude < 0.05f;

        if (inHole && ballStopped && slowEnough && borderOk)
        {
            levelCompleted = true;
            Invoke(nameof(LoadNextLevel), 1.5f);
        }
    }

    void LoadNextLevel()
    {
        string current = SceneManager.GetActiveScene().name;

        if (current == "Level_1") SceneManager.LoadScene("Level_2");
        else if (current == "Level_2") SceneManager.LoadScene("Level_3");
    }
}