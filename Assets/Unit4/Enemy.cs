using UnityEngine;

// Unit 4 (lektion 4.2 + 4.4): fienden rullar mot spelaren
public class Enemy : MonoBehaviour
{
    public float speed = 3.0f;
    private Rigidbody enemyRb;
    private GameObject player;

    void Start()
    {
        enemyRb = GetComponent<Rigidbody>();
        player = GameObject.Find("Player");
    }

    // Kraften läggs i FixedUpdate så att den inte beror på bildfrekvensen
    void FixedUpdate()
    {
        Vector3 lookDirection = (player.transform.position - transform.position).normalized;
        enemyRb.AddForce(lookDirection * speed);
    }

    void Update()
    {

        // 4.4: ta bort fiender som ramlat av ön
        if (transform.position.y < -10)
        {
            Destroy(gameObject);
        }
    }
}
