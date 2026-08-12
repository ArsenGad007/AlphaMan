using UnityEngine;

public class PlayerAnimator : Singleton<PlayerAnimator>, ISpeedUpgradable
{
    [SerializeField] private PlayerController player;
    [SerializeField] private GameInput gameInput;
    [SerializeField] private float minAnimationInterval = 0.25f; 
    [SerializeField] [Range(1f, 2f)] private float minSpeedRunMultiplier = 1f; 
    [SerializeField] [Range(1f, 2f)] private float maxSpeedRunMultiplier = 1.4f; 

    private Animator animator;

    private string currentAnimation = "idle";
    private float lastChangeTime;

    protected override void Awake()
    {
        base.Awake();
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
    private void SetAnimation(string tag)
    {
        if (tag == "run")
            animator.SetFloat("runSpeedMultiplier", SavesLogic.Get("speed_run_anim", minSpeedRunMultiplier));

        animator.ResetTrigger(currentAnimation);
        animator.SetTrigger(tag);
        currentAnimation = tag;
        Debug.Log("Current animation: " + tag);
    }

    public void SpeedProgressUpdate() 
    {
        float step = (maxSpeedRunMultiplier - minSpeedRunMultiplier) / SavesLogic.Get("progress_bar_size", 4);
        SavesLogic.Set("speed_run_anim", SavesLogic.Get("speed_run_anim", minSpeedRunMultiplier) + step);
        Debug.Log($"speed_run_anim: {SavesLogic.Get("speed_run_anim", minSpeedRunMultiplier)}");
    }
}
