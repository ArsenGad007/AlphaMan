using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float speedMove = 2.0f;
    [SerializeField] private float speedRunMove = 5.0f;
    [SerializeField] private float speedRotate = 15.0f;

    [SerializeField] private float acceleration = 25.0f;

    [SerializeField] private float playerCollisionRadius = 0.6f;
    [SerializeField] private float playerCollisionHeight = 2f;

    [SerializeField] private GameInput gameInput;

    private Vector3 smooth_movement;

    private void Update()
    {
        Vector2 inputVector = gameInput.GetInputVectorNormalized();
        Vector3 move_dir = new(inputVector.x, 0, inputVector.y);

        float speed_move = gameInput.IsRunning() ? speedRunMove : speedMove;
        smooth_movement = Vector3.MoveTowards(smooth_movement, move_dir * speed_move, acceleration * Time.deltaTime);

        Vector3 desired_move = smooth_movement * Time.deltaTime;
        desired_move = MovementForCollisions(desired_move);
        transform.position += desired_move;

        if (move_dir != Vector3.zero)
            transform.forward = Vector3.Slerp(transform.forward, move_dir, speedRotate * Time.deltaTime);
    }

    private Vector3 MovementForCollisions(Vector3 delta)
    {
        bool CanMove(Vector3 moveDir)
        {
            if (moveDir == Vector3.zero)
                return true;

            return !Physics.CapsuleCast(
                transform.position,
                transform.position + Vector3.up * playerCollisionHeight,
                playerCollisionRadius,
                moveDir,
                speedRunMove * Time.deltaTime);
        }

        if (delta == Vector3.zero) return Vector3.zero;

        // Если можем двигаться целиком — двигаемся
        if (CanMove(delta)) return delta;

        // Попробуем движение только по X
        Vector3 moveX = new Vector3(delta.x, 0, 0);
        if (CanMove(moveX)) return moveX;

        // Попробуем движение только по Z
        Vector3 moveZ = new Vector3(0, 0, delta.z);
        if (CanMove(moveZ)) return moveZ;

        // Если всё заблокировано
        return Vector3.zero;
    }

}
