using System.Collections;
using UnityEngine;

public class GuardScript : MonoBehaviour
{
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private float speedMove = 2f;
    [SerializeField] private float speedRotate = 15f;

    private int currentPointIndex = 0;

    [SerializeField] private Animator animator;
    private string currentAnimation = "idle";
    private bool isChangingAnimation = false;
    [SerializeField] private float animationChangeDelay = 0.1f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (animator != null)
        {
            StartCoroutine(SetAnimationWithDelay(currentAnimation));
        }
    }

    // Update is called once per frame
    void Update()
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

        string nextAnimation = "idle";
        if (Vector3.Distance(transform.position, target.position) > 0.1f)
        {
            nextAnimation = "walk";
        }

        if (animator != null && currentAnimation != nextAnimation)
        {
            StartCoroutine(SetAnimationWithDelay(nextAnimation));
        }
    }

    public IEnumerator SetAnimationWithDelay(string tag)
    {
        if (isChangingAnimation && tag != "idle")
            yield break;

        isChangingAnimation = true;

        animator.ResetTrigger(currentAnimation);
        animator.SetTrigger(tag);
        currentAnimation = tag;
        Debug.Log("Guard animation: " + tag);

        yield return new WaitForSeconds(tag == "idle" ? 0f : animationChangeDelay);

        isChangingAnimation = false;
    }
}
