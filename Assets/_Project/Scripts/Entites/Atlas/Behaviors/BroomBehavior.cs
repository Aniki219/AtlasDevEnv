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
    public float lift;
    public float thrustRate;
    public float initialThrust;
    public float maxThrust;
    public RateLerper pitchLerper;
    public float pitchRate;
    //dPitch = +/-k1
    //dThrust = k2 * pitch . right

    public float yAngle = 0;
    [field: SerializeField] public float pitch { get; private set; }

    public void StartState()
    {
        body.canGravity = false;
    }

    public void UpdateState()
    {
        pitch = (pitchLerper != null) ? pitchLerper.Value() : 0;

        Vector2 trajectory = new Vector2(
            Cos(pitch * Deg2Rad) * entity.facing,
            Sin(pitch * Deg2Rad)
        );

        float dThrust = Vector2.Dot(
                trajectory,
                Vector2.down
            ) * thrustRate * Time.deltaTime;

        thrust += dThrust;
        lift += dThrust;

        thrust = Clamp(thrust, 0, maxThrust);
        lift = thrust;
        //lift = Clamp(lift, 0, thrust - 2f);

        body.velocity = new Vector2(
            thrust * trajectory.x  * Cos(yAngle * Deg2Rad),
            lift * trajectory.y
        );

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
