using NUnit.Framework;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    private float speedMove = 2.0f;

    [SerializeField]
    private float speedRunMove = 5.0f;

    [SerializeField]
    private float speedRotate = 15.0f;

    private bool isWalking = false;

    private bool isRun = false;

    /// <summary>
    /// Новая система управления
    /// </summary>
    private PlayerInputActions playerInputActions;

    private void Start()
    {
        playerInputActions = new();
        playerInputActions.Player.Enable();
    }

    private void Update()
    {
        Vector2 inputVector = playerInputActions.Player.Move.ReadValue<Vector2>();
        //Debug.Log(inputVector);

        isRun = Input.GetKey(KeyCode.LeftShift);
        float speed_move = isRun ? speedRunMove : speedMove;

        Vector3 move_dir = new(inputVector.y, 0, -inputVector.x);
        isWalking = move_dir != Vector3.zero;

        transform.position += move_dir * speed_move * Time.deltaTime;
        transform.forward = Vector3.Slerp(transform.forward, move_dir, speedRotate * Time.deltaTime);
    }

    public bool IsWalking() => isWalking;
    public bool IsRunning() => isRun && IsWalking();  
}
