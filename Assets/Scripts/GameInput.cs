using UnityEngine;
using UnityEngine.InputSystem;

public class GameInput : MonoBehaviour
{
    private bool isWalking = false;
    private bool isRunPressed = false;

    private Vector2 inputVector;

    private PlayerInputActions playerInputActions;

    private void Start()
    {
        playerInputActions = new();
        playerInputActions.Player.Enable();

        // Подписываемся на события
        playerInputActions.Player.Move.performed += OnMovePerformed;
        playerInputActions.Player.Move.canceled += OnMoveCanceled;
        playerInputActions.Player.Run.performed += OnRunPerformed;
        playerInputActions.Player.Run.canceled += OnRunCanceled;
    }

    private void OnDestroy()
    {
        // Отписываемся от событий при уничтожении объекта
        playerInputActions.Player.Move.performed -= OnMovePerformed;
        playerInputActions.Player.Move.canceled -= OnMoveCanceled;
        playerInputActions.Player.Run.performed -= OnRunPerformed;
        playerInputActions.Player.Run.canceled -= OnRunCanceled;
    }

    private void OnMovePerformed(InputAction.CallbackContext context)
    {
        inputVector = context.ReadValue<Vector2>();
        isWalking = inputVector != Vector2.zero;
    }

    private void OnMoveCanceled(InputAction.CallbackContext context)
    {
        inputVector = Vector2.zero;
        isWalking = false;
    }

    private void OnRunPerformed(InputAction.CallbackContext context) => isRunPressed = true;
    private void OnRunCanceled(InputAction.CallbackContext context) => isRunPressed = false;

    public bool IsWalking() => isWalking;
    public bool IsRunning() => isRunPressed && isWalking;
    public Vector2 GetInputVector() => inputVector;
}
