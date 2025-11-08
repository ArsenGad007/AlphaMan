using UnityEngine;
using UnityEngine.InputSystem;

public class GameInput : MonoBehaviour
{
    [SerializeField] private float runDelay = 0.1f; 
    [SerializeField] private float walkDelay = 0.05f;

    private bool isWalkingKeyPressed = false;
    private bool isRunKeyPressed = false; 

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

    private void Update()
    {
        UpdateWalking();
        UpdateRunning();
    }

    private void UpdateWalking()
    {
        if (isWalkingKeyPressed)
        {
            if (!isActuallyWalking && Time.time - walkChangeTime >= walkDelay)
                isActuallyWalking = true;
        }
        else if (isActuallyWalking)
            isActuallyWalking = false;
    }
    private void UpdateRunning()
    {
        if (isRunKeyPressed && isActuallyWalking)
        {
            if (!isActuallyRunning && Time.time - runChangeTime >= runDelay)
                isActuallyRunning = true;
        }
        else if (isActuallyRunning)
            isActuallyRunning = false;
    }


    private void OnMovePerformed(InputAction.CallbackContext context)
    {
        inputVector = context.ReadValue<Vector2>();
        isWalkingKeyPressed = inputVector != Vector2.zero;
        walkChangeTime = Time.time;

        // если уже зажат Shift Ч сбрасываем таймер бега, чтобы сработала задержка
        if (isRunKeyPressed)
            runChangeTime = Time.time;
    }
    private void OnMoveCanceled(InputAction.CallbackContext context)
    {
        inputVector = Vector2.zero;
        isWalkingKeyPressed = false;
    }
    private void OnRunPerformed(InputAction.CallbackContext context) => (isRunKeyPressed, runChangeTime) = (true, Time.time);
    private void OnRunCanceled(InputAction.CallbackContext context) => isRunKeyPressed = false;


    public bool IsWalking() => isActuallyWalking;
    public bool IsRunning() => isActuallyRunning;
    public Vector2 GetInputVector() => isActuallyWalking ? inputVector : Vector2.zero;
}
