using NUnit.Framework;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    private float speedMove = 2.0f;

    [SerializeField]
    private float speedShiftMove = 5.0f;

    [SerializeField]
    private float speedRotate = 15.0f;

    private bool isWalking = false;

    private bool isRun = false;

    private void Update()
    {
        Vector2 inputVector = new(0, 0);

        if (Input.GetKey(KeyCode.W))
            inputVector.x += 1;
        if (Input.GetKey(KeyCode.S))
            inputVector.x = -1;
        if (Input.GetKey(KeyCode.D))
            inputVector.y = 1;
        if (Input.GetKey(KeyCode.A))
            inputVector.y = -1;

        isRun = Input.GetKey(KeyCode.LeftShift);
        float speed_move = isRun ? speedShiftMove : speedMove;

        inputVector = inputVector.normalized;

        Vector3 move_dir = new(inputVector.x, 0, -inputVector.y);
        isWalking = move_dir != Vector3.zero;

        transform.position += move_dir * speed_move * Time.deltaTime;
        transform.forward = Vector3.Slerp(transform.forward, move_dir, speedRotate * Time.deltaTime);
    }

    public bool IsWalking()
    {
        return isWalking;
    }

    public bool IsRunning()
    {
        return isRun && IsWalking();
    }
}
