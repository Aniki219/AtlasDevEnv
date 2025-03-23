using UnityEngine;
using static AtlasHelpers;

public class MoveBehavior : StateBehavior, IStateBehavior
{
    [SerializeField] bool canTurnAround = true;
    [SerializeField] float moveSpeed = 4.0f;
    [SerializeField] float msMod = 1.0f;
    [SerializeField] float AccelerationTime = 0.1f;
    [SerializeField] float DeccelerationTime = 0.04f;

    float xVelocitySmoothing;

    public void StartState()
    {
        body.OnBonkCeiling.AddListener(bonkCeilingListener);
    }

    public void UpdateState()
    {
        float targetVelocityX = input.X * msMod * moveSpeed;
        changeFacingDirection();

        bool isAccelerating = SameSign(body.velocity.x, targetVelocityX) &&
                              Mathf.Abs(targetVelocityX) >= Mathf.Abs(body.velocity.x);

        float smoothTime = isAccelerating ? AccelerationTime : DeccelerationTime;
        body.velocity.x = Mathf.SmoothDamp(body.velocity.x, targetVelocityX, ref xVelocitySmoothing, smoothTime);
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
            pc.facing = newFacing;
            sprite.SetFacing(pc.facing);
        }
    }

    public void ExitState()
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