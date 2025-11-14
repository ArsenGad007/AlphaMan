using System.Collections;
using UnityEngine;

public class GuardAnimator : MonoBehaviour
{
    [SerializeField] private GuardPatrol guardPatrol;
    [SerializeField] private float animationChangeDelay = 0.1f;

    [SerializeField] private Animator animator;
    private string currentAnimation = "idle";
    private bool isChangingAnimation = false;

    void Start()
    {

        if (animator != null)
        {
            StartCoroutine(SetAnimationWithDelay(currentAnimation));
        }
    }

    void Update()
    {
        if (guardPatrol == null) return;

        string nextAnimation = guardPatrol.IsMoving() ? "walk" : "idle";

        if (animator != null && currentAnimation != nextAnimation)
        {
            StartCoroutine(SetAnimationWithDelay(nextAnimation));
        }
    }

    public IEnumerator SetAnimationWithDelay(string tag)
    {
        if (isChangingAnimation && tag != "idle") yield break;
        isChangingAnimation = true;

        animator.ResetTrigger(currentAnimation);
        animator.SetTrigger(tag);
        currentAnimation = tag;
        Debug.Log("Guard animation: " + tag);

        yield return new WaitForSeconds(tag == "idle" ? 0f : animationChangeDelay);
        isChangingAnimation = false;
    }
}
