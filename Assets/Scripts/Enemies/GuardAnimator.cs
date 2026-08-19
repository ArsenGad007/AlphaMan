using UnityEngine;

public class GuardAnimator : MonoBehaviour
{    
    [SerializeField] private float minAnimationInterval = 0.1f;

    private GuardController guardController;
    private Animator animator;

    private string currentAnimation = "idle";
    private float lastChangeTime;

    private void Awake()
    {
        guardController = GetComponent<GuardController>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        string next_animation = "idle";

        if (guardController?.currentState == GuardController.State.Chase)
            next_animation = "run";
        else if (guardController?.currentState == GuardController.State.Walking)
            next_animation = "walk";

        if (currentAnimation != next_animation && Time.time - lastChangeTime > minAnimationInterval)
        {
            SetAnimation(next_animation);
            lastChangeTime = Time.time;
        }
    }

    private void SetAnimation(string tag)
    {
        // —брасываем все булевы параметры
        animator.SetBool("isIdle", false);
        animator.SetBool("isWalking", false);
        animator.SetBool("isRunning", false);

        switch (tag)
        {
            case "idle": animator.SetBool("isIdle", true); break;
            case "walk": animator.SetBool("isWalking", true); break;
            case "run": animator.SetBool("isRunning", true); break;
        }

        currentAnimation = tag;
    }
}
