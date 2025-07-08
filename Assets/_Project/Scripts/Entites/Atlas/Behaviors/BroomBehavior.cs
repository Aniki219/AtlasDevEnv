using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static UnityEngine.Mathf;

public class BroomBehavior : StateBehavior, IStateBehavior
{
    public new Transform transform;
    public Transform spriteTransform;

    public AnimationClip upPitchOverrideClip;
    public AnimationClip downPitchOverrideClip;
    public Slider thrustSlider;
    public Slider liftSlider;

    [SerializeField] float minThrust;
    public float initialThrust;
    [SerializeField] float thrust;
    [SerializeField] float minLift;
    [SerializeField] float lift;
    [SerializeField] float thrustRate;
    [SerializeField] float maxThrust;

    [SerializeField] float pitch = 0;
    [SerializeField] float minPitch = 1;
    [SerializeField] float pitchChangeSpeed = 100;
    [SerializeField] public float upTiltMax = 30;
    [SerializeField] public float downTiltMax = -30;

    [SerializeField] float pitchFriction = 10;

    public UnityEvent OnCancelBroom;

    float targetPitch = 0;
    float prevPitch;

    public void StartState()
    {
        targetPitch = 0;
        pitch = 0;
        prevPitch = 0;
        thrust = initialThrust;
        lift = minLift;
        body.canGravity = false;
    }

    public void UpdateState()
    {
        SetTargetPitch();
        SetSprite();
        SetSliders();
    }

    public void FixedUpdateState()
    {
        ChangePitchToTargetPitch();
        ChangeThrustAndLift();
    }

    public void ExitState()
    {
        spriteTransform.eulerAngles = Vector3.zero;
        body.canGravity = true;
    }

    private void ChangeThrustAndLift()
    {
        if (body.collisions.getAbove() && pitch > 0 || body.collisions.getBelow() && pitch < 0)
        {

        }
        else
        {
            float thrustDelta = thrustRate * Time.fixedDeltaTime * Sin(pitch * Deg2Rad);
            thrust -= thrustDelta;
            lift -= thrustDelta;
        }

        // Lose speed when changing pitch
        float deltaPitch = Abs(pitch - prevPitch) / pitchFriction * Time.fixedDeltaTime;
        thrust -= deltaPitch;
        lift -= deltaPitch;

        // Lose speed when speed is too low and not pitching down
        if (Abs(thrust) < minThrust && Sin(pitch * Deg2Rad) >= 0)
        {
            thrust -= 2f * Time.fixedDeltaTime;

            if (thrust < 0.25f)
            {
                OnCancelBroom.Invoke();
            }
        }

        thrust = Clamp(thrust, 0, maxThrust);
        lift = Clamp(lift, 0, maxThrust);

        body.velocity = new Vector2(
            Cos(pitch * Deg2Rad) * entity.facing * thrust,
            Sin(pitch * Deg2Rad) * lift
        );
    }

    private void SetTargetPitch()
    {
        if (input.Y != 0)
        {
            targetPitch = input.Y > 0 ? upTiltMax : downTiltMax;
        }
        else
        {
            targetPitch = 0;
        }
    }

    private void ChangePitchToTargetPitch()
    {
        prevPitch = pitch;

        if (pitch < targetPitch)
        {
            pitch += Max(pitchChangeSpeed, Abs(targetPitch - pitch)) * Time.fixedDeltaTime;
        }
        if (pitch > targetPitch)
        {
            pitch -= Max(pitchChangeSpeed, Abs(targetPitch - pitch)) * Time.fixedDeltaTime;
        }
        if (targetPitch == 0 && Abs(pitch) <= minPitch)
        {
            pitch = 0;
        }
    }

    private void SetSprite()
    {
        float sprAngle = (entity.facing > 0 ? 0 : 180) - Vector2.SignedAngle(body.velocity.normalized, Vector2.right);

        spriteTransform.eulerAngles = new Vector3(0, 0, sprAngle);

        if (input.Y == 0)
        {
            sprite.ClearOverrideClip();
        }
        else if (input.Y >= 0)
        {
            sprite.SetOverrideClip(upPitchOverrideClip, true);
        }
        else
        {
            sprite.SetOverrideClip(downPitchOverrideClip, true);
        }
    }

    private void SetSliders()
    {
        thrustSlider.value = thrust / maxThrust;
        liftSlider.value = lift / maxThrust;
    }
}
