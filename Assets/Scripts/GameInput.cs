using UnityEngine;
using UnityEngine.InputSystem;

public class GameInput : MonoBehaviour
{
    [SerializeField] private float runDelay = 0.1f; 
    [SerializeField] private float walkDelay = 0.05f;

    private bool isWalkingPressed = false;
    private bool isRunPressed = false;

    private bool isActuallyWalking = false;
    private bool isActuallyRunning = false;

    private float walkChangeTime;
    private float runChangeTime;

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

    private void Update()
    {
        // WALK задержка
        if (isWalkingPressed && !isActuallyWalking && Time.time - walkChangeTime >= walkDelay)
            isActuallyWalking = true;
        else if (!isWalkingPressed && isActuallyWalking)
            isActuallyWalking = false;

        // RUN задержка
        if (isRunPressed && isActuallyWalking && !isActuallyRunning && Time.time - runChangeTime >= runDelay)
            isActuallyRunning = true;
        else if ((!isRunPressed || !isActuallyWalking) && isActuallyRunning)
            isActuallyRunning = false;
    }

    private void OnMovePerformed(InputAction.CallbackContext context)
    {
        inputVector = context.ReadValue<Vector2>();
        isWalkingPressed = inputVector != Vector2.zero;
        walkChangeTime = Time.time;
    }
    private void OnMoveCanceled(InputAction.CallbackContext context)
    {
        inputVector = Vector2.zero;
        isWalkingPressed = false;
    }

    private void OnRunPerformed(InputAction.CallbackContext context) => (isRunPressed, runChangeTime) = (true, Time.time);
    private void OnRunCanceled(InputAction.CallbackContext context) => isRunPressed = false;

    public bool IsWalking() => isActuallyWalking;
    public bool IsRunning() => isActuallyRunning;
    public Vector2 GetInputVector() => isActuallyWalking ? inputVector : Vector2.zero;
}
