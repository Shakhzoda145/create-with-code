using UnityEngine;
using UnityEngine.InputSystem;

// Unit 3 (lektion 3.1–3.4): kursens PlayerController.
// Heter RunnerPlayerController eftersom PlayerController redan används av Unit 1.
public class RunnerPlayerController : MonoBehaviour
{
    private Rigidbody playerRb;
    private Animator playerAnim;
    private AudioSource playerAudio;

    public InputAction jumpAction;           // 3.1 – bindning: <Keyboard>/space
    public ParticleSystem explosionParticle; // 3.4
    public ParticleSystem dirtParticle;      // 3.4
    public AudioClip jumpSound;              // 3.4
    public AudioClip crashSound;             // 3.4

    public float jumpForce = 10;
    public float gravityModifier = 1.5f;
    public bool isOnGround = true;
    public bool gameOver = false;            // 3.2

    // Projektet har "Reload Domain" avstängt, så Physics.gravity nollställs inte mellan Play.
    // Kursens "Physics.gravity *= gravityModifier" skulle då göra gravitationen starkare
    // varje gång. Därför utgår vi från standardvärdet varje gång.
    private static readonly Vector3 defaultGravity = new Vector3(0, -9.81f, 0);

    void Start()
    {
        playerRb = GetComponent<Rigidbody>();
        playerAnim = GetComponent<Animator>();
        playerAudio = GetComponent<AudioSource>();

        Physics.gravity = defaultGravity * gravityModifier;

        // Om ingen tangent är kopplad i Inspector används Space
        if (jumpAction.bindings.Count == 0) jumpAction.AddBinding("<Keyboard>/space");
        jumpAction.Enable();
    }

    void OnDisable()
    {
        jumpAction.Disable();
    }

    void Update()
    {
        // 3.1 hopp, 3.3 inget hopp efter game over
        if (jumpAction.triggered && isOnGround && !gameOver)
        {
            playerRb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isOnGround = false;
            playerAnim.SetTrigger("Jump_trig");              // 3.3
            dirtParticle.Stop();                             // 3.4
            playerAudio.PlayOneShot(jumpSound, 1.0f);        // 3.4
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isOnGround = true;
            if (!gameOver) dirtParticle.Play();              // 3.4
        }
        else if (collision.gameObject.CompareTag("Obstacle"))
        {
            gameOver = true;                                 // 3.2
            Debug.Log("Game Over!");
            playerAnim.SetBool("Death_b", true);             // 3.3
            playerAnim.SetInteger("DeathType_int", 1);       // 3.3
            explosionParticle.Play();                        // 3.4
            dirtParticle.Stop();                             // 3.4
            playerAudio.PlayOneShot(crashSound, 1.0f);       // 3.4
        }
    }
}
