using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    [SerializeField]
    private PlayerController player;

    private Animator animator;

    private string currentAnimation = "idle";

    public void setAnimation(string tag)
    {
        // Debug.Log("Current animation: " + tag);
        animator.SetTrigger(tag);
        currentAnimation = tag;
    }

    void Start()
    {
        animator = player.GetComponent<Animator>();
        setAnimation("idle");
    }

    void Update()
    {
        if (player.IsRunning() && currentAnimation != "run")
            setAnimation("run");

        else if (player.IsWalking() && !player.IsRunning() && currentAnimation != "walk")
            setAnimation("walk");

        else if (!player.IsWalking() && !player.IsRunning() && currentAnimation != "idle")
            setAnimation("idle");

    }
}
