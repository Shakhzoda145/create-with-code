using UnityEngine;

// Unit 4 + slash-tillägg: en jämn ström av fiender framifrån, max 6 samtidigt.
// Varje runda kommer de tätare, rullar fortare och fler får vara ute samtidigt.
public class SpawnManager : MonoBehaviour
{
    public GameObject enemyPrefab;
    public GameObject powerupPrefab;     // används inte längre (powerups är borttagna)
    public int enemyCount;
    public int absoluteMaxEnemies = 6;   // aldrig fler än så här samtidigt
    public float startInterval = 2.5f;   // sekunder mellan bollar i runda 1
    public float minInterval = 0.7f;     // snabbaste takten
    private float spawnTimer = 1.0f;
    private Transform focalPoint;
    private SlashGameManager game;

    void Start()
    {
        focalPoint = GameObject.Find("Focal Point").transform;
        game = SlashGameManager.instance;
    }

    void Update()
    {
        if (game != null && !game.isGameActive) return;

        int round = game != null ? game.round : 1;
        enemyCount = FindObjectsByType<Enemy>(FindObjectsSortMode.None).Length;
        int maxEnemies = Mathf.Min(2 + round, absoluteMaxEnemies); // runda 1: 3, runda 4+: 6

        spawnTimer -= Time.deltaTime;
        if (spawnTimer <= 0)
        {
            if (enemyCount < maxEnemies)
            {
                SpawnEnemy(round);
            }
            spawnTimer = Mathf.Max(minInterval, startInterval - 0.3f * (round - 1));
        }
    }

    void SpawnEnemy(int round)
    {
        GameObject enemy = Instantiate(enemyPrefab, GenerateSpawnPosition(), enemyPrefab.transform.rotation);
        Enemy e = enemy.GetComponent<Enemy>();
        e.speed = 3.0f + 0.75f * (round - 1);
        e.maxSpeed = 4.0f + 1.0f * (round - 1);
    }

    // Bollarna kommer från bortre kanten, framför kameran
    private Vector3 GenerateSpawnPosition()
    {
        Vector3 forward = focalPoint.forward;
        forward.y = 0;
        forward.Normalize();
        Vector3 right = new Vector3(forward.z, 0, -forward.x);

        Vector3 pos = forward * 9f + right * Random.Range(-6f, 6f);
        pos = Vector3.ClampMagnitude(pos, 10f); // håll positionen innanför väggarna
        return new Vector3(pos.x, 0, pos.z);
    }
}
