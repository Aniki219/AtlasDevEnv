using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static UnityEngine.Mathf;

public class BroomBehavior : StateBehavior, IStateBehavior
{
    public new Transform transform;
    public Transform spriteTransform;
    public Slider thrustSlider;

    [SerializeField] float minThrust;

    public UnityEvent OnCancelBroom;

    public float thrust;
    public float thrustRate;
    public float initialThrust;
    public float maxThrust;
    public RateLerper pitchLerper;
    public float pitchRate;
    //dPitch = +/-k1
    //dThrust = k2 * pitch . right

    public void StartState()
    {
        body.canGravity = false;
    }

    public void UpdateState()
    {
        float pitch = (pitchLerper != null) ? pitchLerper.Value() : 0;

        Vector2 trajectory = new Vector2(
            Cos(pitch * Deg2Rad),
            Sin(pitch * Deg2Rad)
        );

        thrust += Vector2.Dot(
                trajectory,
                Vector2.down
            ) * thrustRate * Time.deltaTime;
        thrust = Clamp(thrust, 0, maxThrust);

        body.velocity = thrust * trajectory;

        SetSprite();
        SetSliders();
    }

    public void FixedUpdateState()
    {

    }

    public void ExitState()
    {
        spriteTransform.eulerAngles = Vector3.zero;
        body.canGravity = true;
    }

    private void SetSprite()
    {
        float sprAngle = (entity.facing > 0 ? 0 : 180) - Vector2.SignedAngle(body.velocity.normalized, Vector2.right);

        spriteTransform.eulerAngles = new Vector3(0, 0, sprAngle);
    }

    private void SetSliders()
    {
        thrustSlider.value = thrust / maxThrust;
    }
}
