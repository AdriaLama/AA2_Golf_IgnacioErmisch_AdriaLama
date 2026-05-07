using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class LevelLoader : MonoBehaviour
{
    [Header("Meta")]
    public Transform hole;
    public float holeRadius = 0.8f;

    [Header("Condiciones de victoria")]
    public float maxSpeedAtHole = 0.5f;

    [Header("UI")]
    public GameObject winPanel;
    public TextMeshProUGUI botesParedText; // arrastra aquí el BotesPared

    private bool levelCompleted = false;
    private bool ballInHole = false;
    private Transform ball;

    void Start()
    {
        ball = GameObject.FindGameObjectWithTag("Ball")?.transform;
        PhysicsManager.Instance.borderContactCount = 0;
        if (winPanel != null) winPanel.SetActive(false);
        if (botesParedText != null) botesParedText.text = "Botes: 0 / 2";
    }

    void Update()
    {
        if (ball == null || hole == null) return;

        PhysicsManager pm = PhysicsManager.Instance;

        // --- Actualizar UI ---
        if (botesParedText != null)
            botesParedText.text = $"Botes: {pm.borderContactCount} / {pm.maxBorderContacts}";

        if (levelCompleted) return;

        // --- Fallo por exceso de botes ---
        if (pm.borderContactCount > pm.maxBorderContacts)
        {
            Debug.Log($"FALLO: demasiados botes ({pm.borderContactCount}/{pm.maxBorderContacts})");
            Invoke(nameof(RestartLevel), 1f);
            return;
        }

        // --- Condición de victoria ---
        Vector3 ballFlat = new Vector3(ball.position.x, 0f, ball.position.z);
        Vector3 holeFlat = new Vector3(hole.position.x, 0f, hole.position.z);
        bool inHole = Vector3.Distance(ballFlat, holeFlat) < holeRadius;

        if (inHole && !ballInHole) ballInHole = true;
        if (!inHole) ballInHole = false;

        bool ballStopped = pm.velocity == Vector3.zero;

        if (ballInHole && ballStopped)
        {
            if (pm.velocity.magnitude < maxSpeedAtHole)
            {
                levelCompleted = true;
                Debug.Log($"VICTORIA | botes={pm.borderContactCount}");
                if (winPanel != null) winPanel.SetActive(true);
                Invoke(nameof(LoadNextLevel), 1.5f);
            }
        }
    }

    void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void LoadNextLevel()
    {
        string current = SceneManager.GetActiveScene().name;
        if (current == "Level_1") SceneManager.LoadScene("Level_2");
        else if (current == "Level_2") SceneManager.LoadScene("Level_3");
        else SceneManager.LoadScene("MainMenu");
    }
}