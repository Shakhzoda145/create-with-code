using System.ComponentModel;
using UnityEngine;
using UnityEngine.InputSystem;


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
    }

    void OnDisable()
    {
        jumpAction.Disable();
        flipAction.Disable();
  
    


    }
    void Update()
    {
          
    if (flipAction.triggered && !isOnGround && !gameOver)
    Debug.Log("FLIP!");
    
        if (jumpAction.triggered && isOnGround && !gameOver)
        {
            playerRb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isOnGround = false;
            playerAnim.SetTrigger("Jump_trig");              
            dirtParticle.Stop();                             
            playerAudio.PlayOneShot(jumpSound, 1.0f);        
        }
    
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isOnGround = true;
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
