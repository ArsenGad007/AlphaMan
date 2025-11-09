using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    [SerializeField] private PlayerController player;
    [SerializeField] private GameInput gameInput;
    [SerializeField] private float minAnimationInterval = 0.25f; 

    private Animator animator;

    private string currentAnimation = "idle";
    private float lastChangeTime;

    public void SetAnimation(string tag)
    {
        animator.ResetTrigger(currentAnimation);
        animator.SetTrigger(tag);
        currentAnimation = tag;
        Debug.Log("Current animation: " + tag);
    }

    void Start()
    {
        animator = player.GetComponent<Animator>();
    }

    void Update()
    {
        string next_animation = "idle";

        if (gameInput.IsRunning())
            next_animation = "run";
        else if (gameInput.IsWalking())
            next_animation = "walk";

        if (currentAnimation != next_animation && Time.time - lastChangeTime > minAnimationInterval)
        {
            SetAnimation(next_animation);
            lastChangeTime = Time.time;
        }
    }
}
