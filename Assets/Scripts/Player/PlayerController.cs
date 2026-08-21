using UnityEngine;

/// <summary>
/// Управление главным героем
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class PlayerController : Singleton<PlayerController>, ISpeedUpgradable
{
    [SerializeField] [Min(0)] private float speedWalkMove = 2.0f;
    [SerializeField] [Min(0)] private float minSpeedRunMove = 5.0f;
    [SerializeField] [Min(0)] private float maxSpeedRunMove = 6.5f;
    [SerializeField] [Min(0)] private float speedRotate = 10.0f;
    [SerializeField] [Min(0)] private float minAcceleration = 20.0f;
    [SerializeField] [Min(0)] private float maxAcceleration = 30.0f;
    [SerializeField] private float gravity = -9.81f;

    private CharacterController characterController;
    private Vector3 smooth_movement;
    private float verticalVelocity;

    protected override void Awake()
    {
        base.Awake();
        characterController = GetComponent<CharacterController>();
    }

    private void Update()
    {
        Vector2 inputVector = GameInput.Instance.GetInputVectorNormalized();
        Vector3 move_dir = new(inputVector.x, 0, inputVector.y);

        float speed_move = GameInput.Instance.IsRunning() ? SavesLogic.Get("speed_run_move", minSpeedRunMove) : speedWalkMove;
        smooth_movement = Vector3.MoveTowards(smooth_movement, move_dir * speed_move, SavesLogic.Get("acceleration", minAcceleration) * Time.deltaTime);

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
            if (GameInput.Instance.IsRunning())
                SoundManager.PlayRun();
            else
                SoundManager.PlayWalk();
        }
    }

    public void SpeedProgressUpdate()
    {
        float step_move = (maxSpeedRunMove - minSpeedRunMove) / SavesLogic.Get("progress_bar_size", 4);
        SavesLogic.Set("speed_run_move", SavesLogic.Get("speed_run_move", minSpeedRunMove) + step_move);
        Debug.Log($"speed_run_move: {SavesLogic.Get("speed_run_move", minSpeedRunMove)}");

        float step_acceleration = (maxAcceleration - minAcceleration) / SavesLogic.Get("progress_bar_size", 4);
        SavesLogic.Set("acceleration", SavesLogic.Get("acceleration", minAcceleration) + step_acceleration);
        Debug.Log($"acceleration: {SavesLogic.Get("acceleration", minAcceleration)}");
    }
}
