using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

// Unit 4 (lektion 4.1 + 4.3): kursens PlayerController.
// Heter SumoPlayerController eftersom PlayerController redan används av Unit 1.
public class SumoPlayerController : MonoBehaviour
{
    public float speed = 5.0f;               // (kursens gamla kraft, används inte längre)
    public float moveSpeed = 8.0f;           // Slash-tillägg: hur snabbt karaktären springer
    public float acceleration = 40.0f;       // hur snabbt den når full fart / stannar
    public float turnSpeed = 720.0f;         // grader per sekund när den vänder sig
    public bool hasPowerup = false;
    public GameObject powerupIndicator;
    public InputAction slashAction;          // Slash-tillägg: Space
    public float slashRange = 3.0f;          // hur långt slashen når
    public SwordSwing sword;                 // svärdet som sveper
    public AudioClip slashSound;             // ljudet för varje slash
    private AudioSource playerAudio;
    public Animator characterAnim;           // karaktärens animationer (springa/stå)

    private Rigidbody playerRb;
    private GameObject focalPoint;
    private InputAction moveAction;
    private float powerupStrength = 15.0f;

    void Start()
    {
        // Unit 3 ändrar gravitationen, så sätt tillbaka standardvärdet här
        Physics.gravity = new Vector3(0, -9.81f, 0);

        playerRb = GetComponent<Rigidbody>();
        focalPoint = GameObject.Find("Focal Point");
        moveAction = CreateMoveAction();

        // Slash-tillägg: spelaren ska inte rulla, bara springa och vända sig
        playerRb.constraints = RigidbodyConstraints.FreezeRotation;

        if (slashAction.bindings.Count == 0) slashAction.AddBinding("<Keyboard>/space");
        slashAction.Enable();
        playerAudio = GetComponent<AudioSource>();
        if (characterAnim != null) characterAnim.SetBool("Static_b", true);
    }

    // Krafter läggs i FixedUpdate, som körs i takt med fysikmotorn.
    // I Update blir kraften starkare ju högre bildfrekvens datorn har.
    void FixedUpdate()
    {
        // Slash-tillägg: WASD styr karaktären direkt, relativt kameran.
        // I stället för en kraft styr vi farten mot ett mål, så att den
        // svarar snabbt och stannar när man släpper tangenten.
        Vector3 targetVelocity = GetMoveDirection() * moveSpeed;
        Vector3 v = playerRb.linearVelocity;
        Vector3 flat = new Vector3(v.x, 0, v.z);
        flat = Vector3.MoveTowards(flat, targetVelocity, acceleration * Time.fixedDeltaTime);
        playerRb.linearVelocity = new Vector3(flat.x, v.y, flat.z);
    }

    // W/S = fram/bak och A/D = vänster/höger, sett från kameran
    Vector3 GetMoveDirection()
    {
        Vector2 input = moveAction.ReadValue<Vector2>();
        Vector3 forward = focalPoint.transform.forward;
        forward.y = 0;
        forward.Normalize();
        Vector3 right = new Vector3(forward.z, 0, -forward.x);
        return Vector3.ClampMagnitude(forward * input.y + right * input.x, 1f);
    }

    void Update()
    {
        // Slash-tillägg: karaktären vänder sig åt det håll den springer
        Vector3 moveDirection = GetMoveDirection();
        if (moveDirection.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
        }

        // Karaktären springer snabbare ju fortare spelaren rör sig
        if (characterAnim != null)
        {
            Vector3 v = playerRb.linearVelocity;
            float groundSpeed = new Vector3(v.x, 0, v.z).magnitude;
            characterAnim.SetFloat("Speed_f", Mathf.Clamp01(groundSpeed / moveSpeed));
        }

        if (slashAction.triggered)
        {
            Slash();
        }

        // 4.3: ringen följer spelaren
        powerupIndicator.transform.position = transform.position + new Vector3(0, -0.5f, 0);
    }

    // 4.3: plocka upp powerup
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Powerup"))
        {
            hasPowerup = true;
            Destroy(other.gameObject);
            StartCoroutine(PowerupCountdownRoutine());
            powerupIndicator.gameObject.SetActive(true);
        }
    }

    // 4.3: powerup varar i 7 sekunder
    IEnumerator PowerupCountdownRoutine()
    {
        yield return new WaitForSeconds(7);
        hasPowerup = false;
        powerupIndicator.gameObject.SetActive(false);
    }

    // 4.3: knuffa bort fiender hårt när du har powerup
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy") && hasPowerup)
        {
            Rigidbody enemyRigidbody = collision.gameObject.GetComponent<Rigidbody>();
            Vector3 awayFromPlayer = collision.gameObject.transform.position - transform.position;
            enemyRigidbody.AddForce(awayFromPlayer * powerupStrength, ForceMode.Impulse);
            Debug.Log("Collided with " + collision.gameObject.name + " with powerup set to " + hasPowerup);
        }
    }
    // Tangentbordsstyrning: WASD och piltangenter.
    // (Projektets "Player/Move" läser också gamepad/joystick, och en ansluten
    // joystick kan ge spelaren en konstant knuff.)
    static InputAction CreateMoveAction()
    {
        var action = new InputAction("Move", InputActionType.Value);
        action.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/w").With("Down", "<Keyboard>/s")
            .With("Left", "<Keyboard>/a").With("Right", "<Keyboard>/d");
        action.Enable();   // Slash-tillägg: bara WASD, piltangenterna vrider kameran
        return action;
    }

    void OnDisable()
    {
        moveAction?.Disable();
        slashAction.Disable();
    }

    // Slash-tillägg: förstör alla fiender i ett klot runt spelaren (lite framåt)
    void Slash()
    {
        if (sword != null) sword.Swing();
        if (playerAudio != null && slashSound != null) playerAudio.PlayOneShot(slashSound);

        Vector3 center = transform.position + transform.forward * 1.0f;
        Collider[] hits = Physics.OverlapSphere(center, slashRange);

        foreach (Collider hit in hits)
        {
            if (hit.CompareTag("Enemy"))
            {
                Destroy(hit.gameObject);
                if (SlashGameManager.instance != null) SlashGameManager.instance.AddKill();
            }
        }
    }
}
