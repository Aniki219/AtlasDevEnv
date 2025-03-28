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
    }

    public void UpdateState()
    {
        float t = state.GetNomalizedTime(state.stateAnimation.length);
        body.SetForwardVelocity(velocityProfile.Evaluate(t) * speed);
    }

    public void FixedUpdateState() { }

    public void ExitState()
    {
        sprite.transform.eulerAngles = Vector3.zero;
        body.canGravity = true;
    }
}