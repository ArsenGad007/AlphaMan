using System.Collections;
using UnityEngine;

public class GuardScript : MonoBehaviour
{
    public Transform[] patrolPoints;
    public float speed = 5.0f;
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
        if (patrolPoints == null || patrolPoints.Length == 0)
        {
            //Debug.Log("нет точек патрулирования");
            return;
        }
        
        Transform target = patrolPoints[currentPointIndex];

        Vector3 dir = target.position - transform.position;
        dir.y = 0;
        if (dir != Vector3.zero)
        {     
            transform.rotation = Quaternion.LookRotation(dir);
        }

        transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);
        if (Vector3.Distance(transform.position, target.position) < 0.1f)
        {
            //Debug.Log("дошел до точки" + currentPointIndex);
            currentPointIndex++;
            if (currentPointIndex >= patrolPoints.Length)
            {
                currentPointIndex = 0;
                // Debug.Log("возвращается");
            }
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
