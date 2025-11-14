using System.Collections;
using UnityEngine;

public class GuardPatrol : MonoBehaviour
{
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private float speedMove = 2f;
    [SerializeField] private float speedRotate = 15f;

    private int currentPointIndex = 0;
    


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Patrol();
    }

    private void Patrol()
    {
        if (patrolPoints == null || patrolPoints.Length == 0) return;

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
            currentPointIndex = (currentPointIndex + 1) % patrolPoints.Length;
        }
    }    
    //связка с анимацией
    public bool IsMoving()
    {
        if (patrolPoints == null || patrolPoints.Length == 0) return false;
        Transform target = patrolPoints[currentPointIndex];
        return Vector3.Distance(transform.position, target.position) > 0.1f;
    }
}
