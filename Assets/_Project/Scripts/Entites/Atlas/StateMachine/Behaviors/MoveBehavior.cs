using UnityEngine;
using static AtlasHelpers;

public class MoveBehavior : StateBehavior, IStateBehavior
{
    [SerializeField] bool canTurnAround = true;
    [SerializeField] float moveSpeed = 4.0f;
    [SerializeField] float msMod = 1.0f;

    public void StartState()
    {
        body.OnBonkCeiling.AddListener(bonkCeilingListener);
    }

    public void UpdateState()
    {
        float targetVelocityX = input.X * msMod * moveSpeed;
        changeFacingDirection();
        body.targetVelocity = targetVelocityX * Vector2.right;
    }

    private void changeFacingDirection()
    {
        if (!canTurnAround)
        {
            return;
        }

        int newFacing = Sign(input.X);
        if (newFacing != 0)
        {
            pc.SetFacing(newFacing);
            sprite.SetFacing(pc.facing);
        }
    }

    public void ExitState(StateType toState)
    {
        body.OnBonkCeiling.RemoveListener(bonkCeilingListener);
    }

    public void bonkCeilingListener()
    {
        if (body.velocity.y <= 0) return;

        sprite.StartDeform(new Vector3(1.0f, 0.75f, 1.0f), 0.05f, 0.05f, Vector2.up);
        sprite.CreateStars(transform.position + 0.2f * Vector3.up);
        body.velocity.y = Mathf.Min(0, body.velocity.y / 2);
        body.ResetGravity();
    }

    public void FixedUpdateState() { }
}