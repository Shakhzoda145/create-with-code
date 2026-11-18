using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Unit 5 (lektion 5.1–5.4): styr spawn, poäng, game over, restart och svårighetsgrad
public class GameManager : MonoBehaviour
{
    public List<GameObject> targets;
    public TextMeshProUGUI scoreText;        // 5.2
    public TextMeshProUGUI gameOverText;     // 5.3
    public Button restartButton;             // 5.3
    public GameObject titleScreen;           // 5.4
    public bool isGameActive;                // 5.3

    private int score;
    private float spawnRate = 1.0f;

    void Awake()
    {
        // Unit 3 ändrar gravitationen, så sätt tillbaka standardvärdet här
        Physics.gravity = new Vector3(0, -9.81f, 0);
    }

    // 5.4: anropas av svårighetsknapparna
    public void StartGame(int difficulty)
    {
        isGameActive = true;
        score = 0;
        spawnRate /= difficulty;
        StartCoroutine(SpawnTarget());
        UpdateScore(0);
        titleScreen.SetActive(false);
    }

    // 5.1: skapa ett slumpat mål med jämna mellanrum så länge spelet pågår
    IEnumerator SpawnTarget()
    {
        while (isGameActive)
        {
            yield return new WaitForSeconds(spawnRate);
            int index = Random.Range(0, targets.Count);
            Instantiate(targets[index]);
        }
    }

    // 5.2
    public void UpdateScore(int scoreToAdd)
    {
        score += scoreToAdd;
        scoreText.text = "Score: " + score;
    }

    // 5.3
    public void GameOver()
    {
        restartButton.gameObject.SetActive(true);
        gameOverText.gameObject.SetActive(true);
        isGameActive = false;
    }

    // 5.3
    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
