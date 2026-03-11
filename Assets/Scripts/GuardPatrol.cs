using UnityEngine;
using UnityEngine.Splines;

public class GuardPatrol : MonoBehaviour
{
    public enum State { Walking, LookingAround , Alerted, Searching}
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private float speedMove = 2f;
    [SerializeField] private float speedRotate = 15f;
    [SerializeField] private FieldOfView fieldOfView;

    private int currentPointIndex = 0;
    private float lookAroundDuration = 2f; 
    private float lookAngle = 45f;
    private float stateTimer = 0f;
    private State currentState = State.Walking;
    [SerializeField] private GameOver gameOver;
    private Vector3 lastSeenPlayerPosition;
    [SerializeField] private float searchDuration = 3f;

    void Update()
    {
        if (currentState != State.Alerted && currentState != State.Searching)
        {
            PlayerCheck();
            Patrol();
        }
        else
            HandleAlertedOrSearching();

        if (fieldOfView != null)
        {
            fieldOfView.UpdateFOV(transform.position, transform.forward);
        }
    }

    private void Patrol()
    {
        switch (currentState)
        {
            case State.Walking:
                HandleWalking();
                break;
            case State.LookingAround:
                HandleLookingAround();
                break;
        }
    }


    private void HandleWalking()
    {
        if (patrolPoints == null || patrolPoints.Length == 0)
            return;

        Transform target = patrolPoints[currentPointIndex];

        transform.position = Vector3.MoveTowards(transform.position, target.position, speedMove * Time.deltaTime);

        Vector3 directionToTarget = target.position - transform.position;
        directionToTarget.y = 0f;

        if (directionToTarget != Vector3.zero)
        {
            Vector3 targetForward = directionToTarget.normalized;
            Quaternion targetRotation = Quaternion.LookRotation(targetForward, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, speedRotate * Time.deltaTime);
        }

        if (Vector3.Distance(transform.position, target.position) < 0.1f)
        {
            currentState = State.LookingAround;
            stateTimer = 0f;
        }
    }

    private void HandleLookingAround()
    {
        stateTimer += Time.deltaTime;
        Transform target = patrolPoints[currentPointIndex];
        Vector3 baseForward = target.forward;
        baseForward.y = 0f;
        if (baseForward == Vector3.zero) baseForward = Vector3.forward;

        float angleOffset = lookAngle * Mathf.Sin(Mathf.PI * 2 * stateTimer / lookAroundDuration);
        Quaternion baseRotation = Quaternion.LookRotation(baseForward, Vector3.up);
        Quaternion lookRotation = baseRotation * Quaternion.Euler(0, angleOffset, 0);

        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, speedRotate * Time.deltaTime);

        if (stateTimer >= lookAroundDuration)
        {
            currentState = State.Walking; 
            currentPointIndex = (currentPointIndex + 1) % patrolPoints.Length;
        }
    }

    private void PlayerCheck()
    {
        if (!fieldOfView.IsPlayerVisible())
            return;
        OnPlayerDetected(fieldOfView.PlayerTransform.position);
    }
    private void OnPlayerDetected(Vector3 playerPosition)
    {
        switch (currentState)
        {
            case State.Walking:
            case State.LookingAround:
                EnterAlertedState(playerPosition);
                break;

            case State.Alerted:
            case State.Searching:
                TriggerGameOver();
                break;
        }
    }

    private void EnterAlertedState(Vector3 playerPosition)
    {
        currentState = State.Alerted;
        lastSeenPlayerPosition = playerPosition;
        stateTimer = 0f;
        Debug.Log("заметил");
    }

    private void HandleAlertedOrSearching()
    {
        stateTimer += Time.deltaTime;
        Vector3 dirToPlayer = lastSeenPlayerPosition - transform.position;
        dirToPlayer.y = 0f;
        if (dirToPlayer != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(dirToPlayer.normalized, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, speedRotate * Time.deltaTime);
        }
        if (currentState == State.Alerted && stateTimer >= 1f)
        {
            currentState = State.Searching;
            stateTimer = 0f;
            Debug.Log("ищет");
        }
        if ((currentState == State.Searching)&&(stateTimer >= searchDuration))
        {
            currentState = State.Walking;
            Debug.Log("успокоился");
        }
    }

    private void TriggerGameOver()
    {
       gameOver.GameOverPanel();
    }

    //связка с анимацией
    public bool IsMoving()
    {
        if (patrolPoints == null || patrolPoints.Length == 0) 
            return false;

        Transform target = patrolPoints[currentPointIndex];
        return Vector3.Distance(transform.position, target.position) > 0.1f;
    }

}
