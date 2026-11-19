using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

// Slash-tillägg: håller koll på poäng, liv och runda, och visar game over
public class SlashGameManager : MonoBehaviour
{
    public static SlashGameManager instance;   // så att andra skript lätt hittar den

    public int lives = 3;
    public int score = 0;
    public int round = 1;
    public int killsPerRound = 4;              // så många bollar för att nå nästa runda
    public bool isGameActive = true;

    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI livesText;
    public TextMeshProUGUI roundText;
    public TextMeshProUGUI gameOverText;

    private float invulnerableUntil = 0;      // kort skydd efter en träff

    void Awake()
    {
        instance = this;
        Time.timeScale = 1;
    }

    void Start()
    {
        gameOverText.gameObject.SetActive(false);
        UpdateUI();
    }

    void Update()
    {
        // R startar om när spelet är slut
        if (!isGameActive && Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    public void AddKill()
    {
        if (!isGameActive) return;
        score++;
        if (score % killsPerRound == 0)
        {
            round++;
        }
        UpdateUI();
    }

    public void PlayerHit()
    {
        if (!isGameActive || Time.time < invulnerableUntil) return;
        lives--;
        invulnerableUntil = Time.time + 1.0f;
        if (lives <= 0)
        {
            GameOver();
        }
        UpdateUI();
    }

    void GameOver()
    {
        isGameActive = false;
        gameOverText.text = "GAME OVER\nPoints: " + score + "\nPress R to play again";
        gameOverText.gameObject.SetActive(true);
        Time.timeScale = 0;   // frys spelet
    }

    void UpdateUI()
    {
        scoreText.text = "Points: " + score;
        livesText.text = "Lives: " + Mathf.Max(lives, 0);
        roundText.text = "Round " + round;
    }
}
