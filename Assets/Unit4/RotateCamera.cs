using UnityEngine;
using UnityEngine.InputSystem;

// Unit 4 (lektion 4.1): roterar kameran runt ön (slash-tillägg: piltangenter eller Q/E)
public class RotateCamera : MonoBehaviour
{
    public float rotationSpeed = 150f;
    private InputAction moveAction;

    void Start()
    {
        moveAction = CreateMoveAction();
    }

    void Update()
    {
        float horizontalInput = moveAction.ReadValue<Vector2>().x;
        transform.Rotate(Vector3.up, horizontalInput * rotationSpeed * Time.deltaTime);
    }
    // Tangentbordsstyrning: WASD och piltangenter.
    // (Projektets "Player/Move" läser också gamepad/joystick, och en ansluten
    // joystick kan ge spelaren en konstant knuff.)
    static InputAction CreateMoveAction()
    {
        var action = new InputAction("Move", InputActionType.Value);
        // Slash-tillägg: WASD styr nu karaktären, så kameran vrids med pilarna eller Q/E
        action.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/e").With("Down", "<Keyboard>/q")
            .With("Left", "<Keyboard>/q").With("Right", "<Keyboard>/e");
        action.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/upArrow").With("Down", "<Keyboard>/downArrow")
            .With("Left", "<Keyboard>/leftArrow").With("Right", "<Keyboard>/rightArrow");
        action.Enable();
        return action;
    }

    void OnDisable()
    {
        moveAction?.Disable();
    }
}
