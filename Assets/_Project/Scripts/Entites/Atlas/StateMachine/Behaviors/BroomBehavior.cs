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
    public GameObject broomTrail;

    [SerializeField] float minThrust;

    public UnityEvent OnCancelBroom;

    public float thrust;
    public float lift;
    public float thrustRate;
    public float initialThrust;
    public float maxThrust;
    public RateLerper pitchLerper;
    public float pitchRate;
    public bool pitchSpriteAngle = true;

    public float yAngle = 0;
    [field: SerializeField] public float pitch { get; private set; }

    private void OnEnable()
    {
        broomTrail.SetActive(true);
    }

    private void OnDisable()
    {
        broomTrail.SetActive(false);
    }

    public void StartState()
    {
        body.canGravity = false;
        body.isFlying = true;
    }

    /*
        This Update behavior is intended to run for all substates.
        This function uses:
        
        trajectory: A direction vector based on pitch angle and facing
        
        dThrust: The amount to change thrust based on the Dot of *trajectory*.
                scaled by thrustRate.

        thrust: Non-negative magnitude of Broom velocity
        
        yAngle: Used for turning around. We simulate rotating *velocity* around 
                the y-axis then project this vector back onto the 2D plane.
                This calculation boils down to multiplying vel.x by the cos(yAngle) 

        velocity: The actual velocity is set to (Trajectory * Thrust rotated by yAngle)

        pitch: Normalized around 0 degrees.
               Determines trajectory, dThrust, and sprite angle
    */

    public void UpdateState()
    {
        pitch = (pitchLerper != null) ? pitchLerper.Value() : 0;

        // X is multiplied by facing because pitch is right-hemisphere normalized
        Vector2 trajectory = new Vector2(
            Cos(pitch * Deg2Rad) * entity.facing,
            Sin(pitch * Deg2Rad)
        );

        // Thrust is modified based on pitch angle.
        // A pitch above 0 degrees will decrease thrust, below will increase
        float dThrust = Vector2.Dot(trajectory, Vector2.down) * thrustRate * Time.deltaTime;

        thrust += dThrust;
        lift += dThrust;

        thrust = Clamp(thrust, 0, maxThrust);
        lift = thrust;
        //lift = Clamp(lift, 0, thrust - 2f);

        // Thrust * Velocity. Lift may one day be different than thrust
        Vector2 thrustVector = new Vector2(
            thrust * trajectory.x,
            lift * trajectory.y
        );

        // Rotate the thrust vector around the yAxis to simulate turning around
        Vector2 yRotationMatrix = new Vector2(Cos(yAngle * Deg2Rad), 1);
        Vector2 rotatedThrustVector = Vector2.Scale(thrustVector, yRotationMatrix);

        body.SetTargetVelocity(rotatedThrustVector);

        SetSprite();
        SetSliders();
    }

    public void FixedUpdateState()
    {

    }

    public void ExitState(StateType toState)
    {
        if (!toState.isA(stateMachine.stateRegistry, StateType.su_Broom))
        {
            pitchLerper = new RateLerper();
            spriteTransform.eulerAngles = Vector3.zero;
            body.canGravity = true;
            body.isFlying = false;
        }
    }

    /*
        Sets the sprite rotation based on the pitch. This effect can be
        turned on or off using the pitchSpriteAngle bool
    */
    private void SetSprite()
    {
        if (!pitchSpriteAngle) return;
        float sprAngle = (entity.facing > 0 ? 0 : 180) - Vector2.SignedAngle(body.velocity.normalized, Vector2.right);

        spriteTransform.eulerAngles = new Vector3(0, 0, sprAngle);
    }

    private void SetSliders()
    {
        thrustSlider.value = thrust / maxThrust;
    }
}
