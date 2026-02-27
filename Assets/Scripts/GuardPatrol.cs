using UnityEngine;
using UnityEngine.Splines;

public class GuardPatrol : MonoBehaviour
{
    public enum State { Walking, LookingAround }
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private float speedMove = 2f;
    [SerializeField] private float speedRotate = 15f;
    [SerializeField] private FieldOfView fieldOfView;

    private int currentPointIndex = 0;
    private float lookAroundDuration = 2f; 
    private float lookAngle = 45f;
    private float stateTimer = 0f;
    private State currentState = State.Walking;
   
    void Update()
    {
        Patrol();

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

    //связка с анимацией
    public bool IsMoving()
    {
        if (patrolPoints == null || patrolPoints.Length == 0) 
            return false;

        Transform target = patrolPoints[currentPointIndex];
        return Vector3.Distance(transform.position, target.position) > 0.1f;
    }

}
