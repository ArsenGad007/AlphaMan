using System.Collections;
using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    [SerializeField]
    private PlayerController player;

    [SerializeField]
    private GameInput gameInput;

    [SerializeField]
    private float animationChangeDelay = 0.1f;

    private Animator animator;

    private string currentAnimation = "idle";

    private bool isChangingAnimation = false;

    public IEnumerator SetAnimationWithDelay(string tag)
    {
        if (isChangingAnimation && tag != "idle") 
            yield break;

        isChangingAnimation = true;

        animator.ResetTrigger(currentAnimation);
        animator.SetTrigger(tag);
        currentAnimation = tag;
        Debug.Log("Current animation: " + tag);

        // Немедленно переходим в idle, остальное — с небольшой задержкой
        yield return new WaitForSeconds(tag == "idle" ? 0f : animationChangeDelay);

        isChangingAnimation = false;
    }

    void Start()
    {
        animator = player.GetComponent<Animator>();
        StartCoroutine(SetAnimationWithDelay(currentAnimation));
    }

    void Update()
    {
        string next_animation = "idle";

        if (gameInput.IsRunning())
            next_animation = "run";
        else if (gameInput.IsWalking())
            next_animation = "walk";

        if (currentAnimation != next_animation)
            StartCoroutine(SetAnimationWithDelay(next_animation));
    }
}
