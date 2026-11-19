using UnityEngine;

// Unit 4 + slash-tillägg: fienden rullar åt slumpmässiga håll och studsar mot väggarna
public class Enemy : MonoBehaviour
{
    public float speed = 3.0f;              // hur hårt bollen knuffas
    public float maxSpeed = 5.0f;           // högsta fart (ökar varje runda)
    public float changeDirectionTime = 2.0f; // ungefär hur ofta bollen byter håll
    private Rigidbody enemyRb;
    private Vector3 rollDirection;
    private float timer;

    void Start()
    {
        enemyRb = GetComponent<Rigidbody>();

        // Första riktningen: ungefär in mot mitten av ön, med slumpmässig spridning
        Vector3 toCenter = new Vector3(-transform.position.x, 0, -transform.position.z).normalized;
        rollDirection = Quaternion.Euler(0, Random.Range(-50f, 50f), 0) * toCenter;
        timer = changeDirectionTime;
    }

    void FixedUpdate()
    {
        enemyRb.AddForce(rollDirection * speed);

        // Begränsa farten så att bollarna inte blir hur snabba som helst
        Vector3 v = enemyRb.linearVelocity;
        Vector3 flat = new Vector3(v.x, 0, v.z);
        if (flat.magnitude > maxSpeed)
        {
            flat = flat.normalized * maxSpeed;
            enemyRb.linearVelocity = new Vector3(flat.x, v.y, flat.z);
        }
    }

    void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            PickRandomDirection();
        }

        // 4.4: ta bort fiender som ramlat av ön
        if (transform.position.y < -10)
        {
            Destroy(gameObject);
        }
    }

    void PickRandomDirection()
    {
        float angle = Random.Range(0f, 360f);
        rollDirection = Quaternion.Euler(0, angle, 0) * Vector3.forward;
        timer = changeDirectionTime * Random.Range(0.6f, 1.4f);
    }

    void OnCollisionEnter(Collision collision)
    {
        // Träffar bollen spelaren tar spelaren skada och bollen försvinner
        if (collision.gameObject.GetComponent<SumoPlayerController>() != null)
        {
            if (SlashGameManager.instance != null) SlashGameManager.instance.PlayerHit();
            Destroy(gameObject);
            return;
        }

        // Studsa mot väggar: spegla riktningen (golvet har en normal som pekar uppåt och ignoreras)
        Vector3 normal = collision.GetContact(0).normal;
        if (Mathf.Abs(normal.y) < 0.5f)
        {
            rollDirection = Vector3.Reflect(rollDirection, normal);
            rollDirection.y = 0;
            rollDirection.Normalize();
        }
    }
}
