using UnityEngine;
using UnityEngine.InputSystem;

public class GameInput : MonoBehaviour
{
    private bool isWalking = false;
    private bool isRunning = false;
    private bool isInteract = false;

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
        playerInputActions.Player.Interact.performed += OnInteractPerformed;
        playerInputActions.Player.Interact.canceled += OnInteractCanceled;
    }

    private void OnDestroy()
    {
        playerInputActions.Player.Move.performed -= OnMovePerformed;
        playerInputActions.Player.Move.canceled -= OnMoveCanceled;
        playerInputActions.Player.Run.performed -= OnRunPerformed;
        playerInputActions.Player.Run.canceled -= OnRunCanceled;
        playerInputActions.Player.Interact.performed -= OnInteractPerformed;
        playerInputActions.Player.Interact.canceled -= OnInteractCanceled;

        playerInputActions.Player.Disable();
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
    private void OnInteractPerformed(InputAction.CallbackContext obj) => isInteract = true;
    private void OnInteractCanceled(InputAction.CallbackContext obj) => isInteract = false;

    public bool IsWalking() => isWalking;
    public bool IsRunning() => isWalking && isRunning;
    public bool IsInteract() => isInteract;
    public void DisablePlayerInputActions() => playerInputActions.Disable();
    public Vector2 GetInputVectorNormalized() => inputVector;
}
