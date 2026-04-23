using UnityEngine;
using UnityEngine.Splines;
using System.Collections;

public class GuardPatrol : MonoBehaviour
{
    public enum State { Walking, LookingAround, Alerted, Searching }
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private float speedMove = 2f;
    [SerializeField] private float speedRotate = 10f;
    [SerializeField] private FieldOfView fieldOfView;

    private int currentPointIndex = 0;
    // private float lookAroundDuration = 2f; 
    private float lookAngle = 45f;
    private float stateTimer = 0f;
    private State currentState = State.Walking;
    [SerializeField] private GameOver gameOver;
    private Vector3 lastSeenPlayerPosition;
    [SerializeField] private float searchDuration = 3f;
    [SerializeField] private float alertTime = 0.5f;

    //для обнаружения
    private bool isHiding = false;
    private Quaternion targetRotationToPlayer;
    private float turnToPlayerTimer;
    private bool isTurningToPlayer;

    void Update()
    {
        PlayerCheck();
        if (currentState != State.Alerted && currentState != State.Searching)
        {
            Patrol();
        }
        else
        {
            HandleAlertedOrSearching();


        }

        if (fieldOfView != null)
        {
            fieldOfView.UpdateFOV(transform.position, transform.forward);
        }
        if (isTurningToPlayer)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotationToPlayer, speedRotate * Time.deltaTime);
            turnToPlayerTimer += Time.deltaTime;

            if (turnToPlayerTimer >= 0.3f)
            {
                isTurningToPlayer = false;
            }
        }
    }

    private void Patrol()
    {
        switch (currentState)
        {
            case State.Walking:
                HandleWalking();
                break;
                /*case State.LookingAround:
                    HandleLookingAround();
                    break;*/
        }
    }


    private void HandleWalking()
    {
        if (patrolPoints == null || patrolPoints.Length == 0)
            return;

        Transform target = patrolPoints[currentPointIndex];

        float distanceToTarget = Vector3.Distance(transform.position, target.position);
        transform.position = Vector3.MoveTowards(transform.position, target.position, speedMove * Time.deltaTime);

        Vector3 directionToTarget = target.position - transform.position;
        directionToTarget.y = 0f;

        if (directionToTarget != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(directionToTarget.normalized, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, speedRotate * Time.deltaTime);
        }
        if (distanceToTarget <= 0.1f)
        {
            currentPointIndex = (currentPointIndex + 1) % patrolPoints.Length;
            stateTimer = 0f;
        }
    }

    /*private void HandleLookingAround()
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
    }*/

    private void PlayerCheck()
    {
        FieldOfView.DetectionType detection = fieldOfView.CheckForDetection();
        if (detection == FieldOfView.DetectionType.InstantDeath)
        {
                lastSeenPlayerPosition = fieldOfView.PlayerTransform.position;
                Vector3 dirToPlayer = lastSeenPlayerPosition - transform.position;
                dirToPlayer.y = 0f;

                if (dirToPlayer.magnitude > 0)
                {
                    // Quaternion targetRot = Quaternion.LookRotation(dirToPlayer.normalized, Vector3.up);
                    // transform.rotation = targetRot; 
                    targetRotationToPlayer = Quaternion.LookRotation(dirToPlayer.normalized, Vector3.up);
                    turnToPlayerTimer = 0f;
                    isTurningToPlayer = true;
                    StartCoroutine(TriggerGameOverWithDelay());
                }
               // StartCoroutine(TriggerGameOverWithDelay());
               // return;
        }

        if (detection == FieldOfView.DetectionType.None)
        {
            isHiding = false;
            return;
        }

        if (currentState == State.Searching)
        {
            TriggerGameOver();
            return;
        }
        if (isHiding)
            return;

        OnPlayerDetected(fieldOfView.PlayerTransform.position);
    }

    private IEnumerator TriggerGameOverWithDelay()
    {
        yield return new WaitForSeconds(0.1f); 
        TriggerGameOver();
    }

    private void OnPlayerDetected(Vector3 playerPosition)
    {
        if (currentState == State.Walking)
        {
            EnterAlertedState(playerPosition);
        }
        else if (currentState == State.Alerted || currentState == State.Searching)
        {
            isTurningToPlayer = true;
            TriggerGameOver();
        }
    }

    private void EnterAlertedState(Vector3 playerPosition)
    {
        currentState = State.Alerted;
        lastSeenPlayerPosition = playerPosition;
        stateTimer = 0f;
        isHiding = true;

    }

    private void HandleAlertedOrSearching()
    {
        stateTimer += Time.deltaTime;

        if (currentState == State.Alerted)
        {
            Vector3 dirToPlayer = lastSeenPlayerPosition - transform.position;
            dirToPlayer.y = 0f;
            if (dirToPlayer != Vector3.zero)
            {
                Quaternion targetRot = Quaternion.LookRotation(dirToPlayer.normalized, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, speedRotate * Time.deltaTime);
            }
            if (stateTimer >= alertTime)
            {
                currentState = State.Searching;
                stateTimer = 0f;
            }
        }
        else if (currentState == State.Searching)
        {
            Vector3 baseDir = lastSeenPlayerPosition - transform.position;
            baseDir.y = 0f;
            if (baseDir == Vector3.zero) baseDir = transform.forward;

            float angleOffset = lookAngle * Mathf.Sin(Mathf.PI * 2 * stateTimer / searchDuration);
            Quaternion baseRot = Quaternion.LookRotation(baseDir.normalized, Vector3.up);
            Quaternion searchRot = baseRot * Quaternion.Euler(0, angleOffset, 0);
            transform.rotation = Quaternion.Slerp(transform.rotation, searchRot, speedRotate * Time.deltaTime);
            if (stateTimer >= searchDuration)
            {
                currentState = State.Walking;
                isHiding = false;
            }
        }
    }

    private void TriggerGameOver()
    {
        if (gameOver != null)
            gameOver.GameOverPanel();
    }
    public bool IsMoving()
    {

        return currentState == State.Walking;
    }
}