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
        setAnimation(currentAnimation);
    }

    void Update()
    {
        if (player.IsRunning())
        {
            // ѕровер€ю потому что параметры состо€ний animator типа trigger (а не bool)
            if (currentAnimation != "run")  
                setAnimation("run");
        }
        else if (player.IsWalking())
        {
            if (currentAnimation != "walk")
                setAnimation("walk");
        }
        else if (currentAnimation != "idle")
            setAnimation("idle");
    }
}
