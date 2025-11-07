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

    [SerializeField]
    private GameInput gameInput;

    private void Update()
    {
        if (gameInput.IsWalking())
        {
            Vector2 inputVector = gameInput.GetInputVector();
            float speed_move = gameInput.IsRunning() ? speedRunMove : speedMove;

            Vector3 move_dir = new(inputVector.y, 0, -inputVector.x);

            transform.position += move_dir * speed_move * Time.deltaTime;
            transform.forward = Vector3.Slerp(transform.forward, move_dir, speedRotate * Time.deltaTime);
        }
    }
}
