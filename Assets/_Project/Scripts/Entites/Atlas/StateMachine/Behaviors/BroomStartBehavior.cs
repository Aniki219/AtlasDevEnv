using UnityEngine;

public class BroomStartBehavior : StateBehavior, IStateBehavior
{
    [SerializeField] AnimationCurve velocityProfile;
    [SerializeField] BroomBehavior broomBehavior;
    float speed;

    public void StartState()
    {
        speed = broomBehavior.initialThrust;
        body.canGravity = false;
        body.isFlying = true;
    }

    public void UpdateState()
    {
        float t = state.GetNomalizedTime(state.stateAnimation.length);
        body.SetTargetVelocity(velocityProfile.Evaluate(t) * speed * Vector2.right * entity.facing);
        body.velocity.y = 0;
        if (t >= 1)
        {
            state.MarkComplete();
        }
    }

    public void FixedUpdateState() { }

    public void ExitState(StateType toState)
    {
        sprite.transform.eulerAngles = Vector3.zero;
        broomBehavior.thrust = broomBehavior.initialThrust;
        broomBehavior.lift = broomBehavior.initialThrust;
        broomBehavior.pitchLerper = new RateLerper();
        if (!toState.isA(stateMachine.stateRegistry, StateType.su_Broom))
        {
            body.canGravity = true;
            body.isFlying = false;
        }
    }
}