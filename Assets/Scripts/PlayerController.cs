using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float speedMove = 2.0f;
    [SerializeField] private float speedRunMove = 5.0f;
    [SerializeField] private float speedRotate = 15.0f;

    [SerializeField] private float acceleration = 25.0f;

    [SerializeField] private GameInput gameInput;

    private Vector3 smooth_movement;

    private void Update()
    {
        Vector2 inputVector = gameInput.GetInputVector();
        Vector3 move_dir = new(inputVector.x, 0, inputVector.y);

        float speed_move = gameInput.IsRunning() ? speedRunMove : speedMove;

        smooth_movement = Vector3.MoveTowards(smooth_movement, move_dir * speed_move, acceleration * Time.deltaTime);
      
        transform.position += smooth_movement * Time.deltaTime;
        if (move_dir != Vector3.zero)
            transform.forward = Vector3.Slerp(transform.forward, move_dir, speedRotate * Time.deltaTime);
    }
}
