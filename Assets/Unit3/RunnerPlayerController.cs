using System.ComponentModel;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;


public class RunnerPlayerController : MonoBehaviour
{
    private Rigidbody playerRb;
    private Animator playerAnim;
    private AudioSource playerAudio;

    public InputAction jumpAction;   
    public InputAction flipAction;        
    public ParticleSystem explosionParticle; 
    public ParticleSystem dirtParticle;      
    public AudioClip jumpSound;              
    public AudioClip crashSound;   
    public TextMeshProUGUI trickText;     
    public float flipSpeed = 900f;     // grader per sekund (900 = ett varv på 0,4 s)
    private bool isFlipping = false;   // snurrar spelaren just nu?
    private float flipDegrees = 0;     // hur långt det pågående varvet har kommit
    private int flipCount = 0;         // antal hela varv i det här hoppet     

    public float jumpForce = 10;
    public float gravityModifier = 1.5f;
    public bool isOnGround = true;
    public bool gameOver = false;            

    private static readonly Vector3 defaultGravity = new Vector3(0, -9.81f, 0);

    void Start()
    {
        playerRb = GetComponent<Rigidbody>();
        playerAnim = GetComponent<Animator>();
        playerAudio = GetComponent<AudioSource>();

        Physics.gravity = defaultGravity * gravityModifier;

       
        if (jumpAction.bindings.Count == 0) jumpAction.AddBinding("<Keyboard>/space");
        jumpAction.Enable();
        if(flipAction.bindings.Count == 0) flipAction.AddBinding ("<Keyboard>/w");
        flipAction.Enable();
        trickText.gameObject.SetActive(false);
    }

    void OnDisable()
    {
        jumpAction.Disable();
        flipAction.Disable();
  
    


    }
   
    void Update()
    {
        // Hopp: bara på marken
        if (jumpAction.triggered && isOnGround && !gameOver)
        {
            playerRb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isOnGround = false;
            playerAnim.SetTrigger("Jump_trig");
            dirtParticle.Stop();
            playerAudio.PlayOneShot(jumpSound, 1.0f);
        }

        // Starta en kullerbytta: bara i luften och om vi inte redan snurrar
        if (flipAction.triggered && !isOnGround && !isFlipping && !gameOver)
        {
            isFlipping = true;
            flipDegrees = 0;
        }

        // Snurra en bit varje frame tills ett helt varv är klart
        if (isFlipping)
        {
            float step = flipSpeed * Time.deltaTime;
            transform.Rotate(Vector3.forward, -step, Space.World);
            flipDegrees += step;

            if (flipDegrees >= 360)
            {
                isFlipping = false;
                flipCount++;
                transform.rotation = Quaternion.Euler(0, 90, 0);
                Debug.Log("FLIPS!: " + flipCount);
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isOnGround = true;
            flipCount = 0; 
            if (!gameOver) dirtParticle.Play();              
        }
        else if (collision.gameObject.CompareTag("Obstacle"))
        {
            gameOver = true;                                 
            Debug.Log("Game Over!");
            playerAnim.SetBool("Death_b", true);             
            playerAnim.SetInteger("DeathType_int", 1);       
            explosionParticle.Play();                        
            dirtParticle.Stop();                             
            playerAudio.PlayOneShot(crashSound, 1.0f);       
        }
    }
}
