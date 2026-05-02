using UnityEngine;

/// <summary>
/// Управление игроком
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private float speedWalkMove = 2.0f;
    [SerializeField] private float speedRunMove = 5.0f;
    [SerializeField] private float speedRotate = 15.0f;
    [SerializeField] private float acceleration = 25.0f;
    [SerializeField] private float gravity = -9.81f;

    [SerializeField] private GameInput gameInput;

    private CharacterController characterController;
    private Vector3 smooth_movement;
    private float verticalVelocity;
    
    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
    }

    private void Update()
    {
        Vector2 inputVector = gameInput.GetInputVectorNormalized();
        Vector3 move_dir = new(inputVector.x, 0, inputVector.y);

        float speed_move = gameInput.IsRunning() ? speedRunMove : speedWalkMove;
        smooth_movement = Vector3.MoveTowards(smooth_movement, move_dir * speed_move, acceleration * Time.deltaTime);

        // Гравитация
        if (characterController.isGrounded)
            verticalVelocity = -5f;         // прижимаем к земле        
        else
            verticalVelocity += gravity * Time.deltaTime;
 

        Vector3 desired_move = smooth_movement + Vector3.up * verticalVelocity;     
        characterController.Move(desired_move * Time.deltaTime);


        if (move_dir != Vector3.zero)
        {
            transform.forward = Vector3.Slerp(transform.forward, move_dir, speedRotate * Time.deltaTime);
            if (gameInput.IsRunning())
                SoundManager.PlayRun();
            else
                SoundManager.PlayWalk();
        }
    }
}
