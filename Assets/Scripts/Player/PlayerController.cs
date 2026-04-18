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
        {
            transform.forward = Vector3.Slerp(transform.forward, move_dir, speedRotate * Time.deltaTime);
            if (gameInput.IsRunning())
                SoundManager.PlayRun();
            else
                SoundManager.PlayWalk();
        }
    }

    private Vector3 MovementForCollisions(Vector3 delta)
    {
        if (delta == Vector3.zero) 
            return Vector3.zero;

        // Попытка движения целиком
        if (CanMove(delta)) 
            return delta;   

        // Если столкнулись, то пробуем скользить вдоль препятствия
        Vector3 slide = GetSlideDirection(delta);  
        if (slide != Vector3.zero && CanMove(slide))
            return slide;

        // Попробуем движение только по X
        Vector3 moveX = new Vector3(delta.x, 0, 0);     
        if (CanMove(moveX)) 
            return moveX;

        // Попробуем движение только по Z
        Vector3 moveZ = new Vector3(0, 0, delta.z);     
        if (CanMove(moveZ)) 
            return moveZ;

        // Если всё заблокировано
        return Vector3.zero;



        // ======== Локальные функции ========

        bool CanMove(Vector3 moveDir)
        {
            if (moveDir == Vector3.zero)
                return true;

            return !Physics.CapsuleCast(
                GetBottomDot(),
                GetTopDot(),
                playerCollisionRadius,
                moveDir,
                speedRunMove * Time.deltaTime,
                ~0,                                // все слои         
                QueryTriggerInteraction.Ignore);   // игнорируем триггеры
        }

        Vector3 GetSlideDirection(Vector3 delta)
        {
            // Получаем нормаль столкновения
            RaycastHit hit;
            if (Physics.CapsuleCast(
                GetBottomDot(),
                GetTopDot(),
                playerCollisionRadius,
                delta,
                out hit,
                speedRunMove * Time.deltaTime,
                ~0,                                       
                QueryTriggerInteraction.Ignore))
            {
                // Возвращаем направление скольжения вдоль поверхности
                return Vector3.ProjectOnPlane(delta, hit.normal);
            }
            return Vector3.zero;
        }

        Vector3 GetBottomDot() => transform.position;
        Vector3 GetTopDot() => transform.position + Vector3.up * playerCollisionHeight;
    }
}
