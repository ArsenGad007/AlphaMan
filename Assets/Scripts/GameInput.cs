using UnityEngine;
using UnityEngine.InputSystem;

public class GameInput : MonoBehaviour
{
    private bool isWalking = false;
    private bool isRunning = false;

    private Vector2 inputVector;
    private PlayerInputActions playerInputActions;

    private void Start()
    {
        playerInputActions = new();
        playerInputActions.Player.Enable();

        playerInputActions.Player.Move.performed += OnMovePerformed;
        playerInputActions.Player.Move.canceled += OnMoveCanceled;
        playerInputActions.Player.Run.performed += OnRunPerformed;
        playerInputActions.Player.Run.canceled += OnRunCanceled;
    }

    private void OnDestroy()
    {
        playerInputActions.Player.Move.performed -= OnMovePerformed;
        playerInputActions.Player.Move.canceled -= OnMoveCanceled;
        playerInputActions.Player.Run.performed -= OnRunPerformed;
        playerInputActions.Player.Run.canceled -= OnRunCanceled;
    }

    private void OnMovePerformed(InputAction.CallbackContext context)
    {
        inputVector = context.ReadValue<Vector2>();
        isWalking = true;
    }
    private void OnMoveCanceled(InputAction.CallbackContext context)
    {
        inputVector = Vector2.zero;
        isWalking = false;
    }
    private void OnRunPerformed(InputAction.CallbackContext context) => isRunning = true;
    private void OnRunCanceled(InputAction.CallbackContext context) => isRunning = false;


    public bool IsWalking() => isWalking;
    public bool IsRunning() => isWalking && isRunning;
    public Vector2 GetInputVectorNormalized() => inputVector;
}
