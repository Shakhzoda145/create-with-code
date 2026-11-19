using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

// Unit 4 (lektion 4.1 + 4.3): kursens PlayerController.
// Heter SumoPlayerController eftersom PlayerController redan används av Unit 1.
public class SumoPlayerController : MonoBehaviour
{
    public float speed = 5.0f;
    public bool hasPowerup = false;
    public GameObject powerupIndicator;
    public InputAction slashAction;          // Slash-tillägg: Space
    public float slashRange = 3.0f;          // hur långt slashen når

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

        // Slash-tillägg: spelaren ska inte rulla, bara glida och vridas mot kameran
        playerRb.constraints = RigidbodyConstraints.FreezeRotation;

        if (slashAction.bindings.Count == 0) slashAction.AddBinding("<Keyboard>/space");
        slashAction.Enable();
    }

    // Krafter läggs i FixedUpdate, som körs i takt med fysikmotorn.
    // I Update blir kraften starkare ju högre bildfrekvens datorn har.
    void FixedUpdate()
    {
        // 4.1: rulla framåt/bakåt i den riktning kameran tittar (W/S, upp/ner)
        float forwardInput = moveAction.ReadValue<Vector2>().y;
        playerRb.AddForce(focalPoint.transform.forward * speed * forwardInput);
    }

    void Update()
    {
        // Slash-tillägg: spelaren tittar alltid åt samma håll som kameran
        transform.rotation = Quaternion.Euler(0, focalPoint.transform.eulerAngles.y, 0);

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
        action.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/upArrow").With("Down", "<Keyboard>/downArrow")
            .With("Left", "<Keyboard>/leftArrow").With("Right", "<Keyboard>/rightArrow");
        action.Enable();
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
        Vector3 center = transform.position + transform.forward * 1.0f;
        Collider[] hits = Physics.OverlapSphere(center, slashRange);

        foreach (Collider hit in hits)
        {
            if (hit.CompareTag("Enemy"))
            {
                Destroy(hit.gameObject);
            }
        }
    }
}
