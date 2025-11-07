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
    private float acceleration = 30.0f;

    [SerializeField]
    private GameInput gameInput;

    private Vector3 smooth_movement;

    private void Update()
    {
        Vector2 inputVector = gameInput.GetInputVector();
        float speed_move = gameInput.IsRunning() ? speedRunMove : speedMove;

        Vector3 move_dir = new(inputVector.x, 0, inputVector.y);

        smooth_movement = Vector3.MoveTowards(smooth_movement, move_dir * speed_move, acceleration * Time.deltaTime);
      
        transform.position += smooth_movement * Time.deltaTime;
        transform.forward = Vector3.Slerp(transform.forward, move_dir, speedRotate * Time.deltaTime);
    }
}
