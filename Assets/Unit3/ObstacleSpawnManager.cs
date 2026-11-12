using UnityEngine;

// Unit 3 (lektion 3.1–3.2): kursens SpawnManager.
// Heter ObstacleSpawnManager eftersom SpawnManager redan används av Unit 2.
public class ObstacleSpawnManager : MonoBehaviour
{
    public GameObject obstaclePrefab;
    private Vector3 spawnPos = new Vector3(25, 0, 0);
    private float startDelay = 2;
    private float repeatRate = 2;
    private RunnerPlayerController playerControllerScript;

    void Start()
    {
        InvokeRepeating("SpawnObstacle", startDelay, repeatRate);
        playerControllerScript = GameObject.Find("Player").GetComponent<RunnerPlayerController>();
    }

    void SpawnObstacle()
    {
        // Skapa bara hinder så länge spelet pågår
        if (playerControllerScript.gameOver == false)
        {
            Instantiate(obstaclePrefab, spawnPos, obstaclePrefab.transform.rotation);
        }
    }
}
