using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    private float speedMove = 2.0f;

    [SerializeField]
    private float speedRunMove = 5.0f;

    [SerializeField]
    private float speedRotate = 15.0f;

    private bool isWalking = false;
    private bool isRunPressed = false; 
    private bool isRun => isRunPressed && isWalking; 

    private Vector2 inputVector;

    /// <summary>
    /// Новая система управления
    /// </summary>
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

    private void OnRunPerformed(InputAction.CallbackContext context)
    {
        isRunPressed = true;
    }

    private void OnRunCanceled(InputAction.CallbackContext context)
    {
        isRunPressed = false;
    }

    private void Update()
    {
        if (isWalking)
        {
            float speed_move = isRun ? speedRunMove : speedMove;

            Vector3 move_dir = new(inputVector.y, 0, -inputVector.x);

            transform.position += move_dir * speed_move * Time.deltaTime;
            transform.forward = Vector3.Slerp(transform.forward, move_dir, speedRotate * Time.deltaTime);
        }
    }

    public bool IsWalking() => isWalking;
    public bool IsRunning() => isRun;

    private void OnDestroy()
    {
        // Отписываемся от событий при уничтожении объекта
        playerInputActions.Player.Move.performed -= OnMovePerformed;
        playerInputActions.Player.Move.canceled -= OnMoveCanceled;
        playerInputActions.Player.Run.performed -= OnRunPerformed;
        playerInputActions.Player.Run.canceled -= OnRunCanceled;
    }
}
