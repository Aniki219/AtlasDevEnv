using System.Diagnostics.CodeAnalysis;
using UnityEngine;

public class FixedDash : StateBehavior, IStateBehavior
{
    [SerializeField] AnimationCurve velocityProfile;
    private float duration;
    public float pauseTransitionTime = 0.25f;

    public void StartState()
    {
        duration = velocityProfile[velocityProfile.length - 1].time;
        state.pauseTransitionsUntil = pauseTransitionTime;
        // sprite.CreateParticle(ParticleSystem.Dust);
    }

    public void UpdateState()
    {
        float t = state.GetNomalizedTime(duration);
        body.SetTargetForwardVelocity(velocityProfile.Evaluate(t * duration));
        if (t >= 1)
        {
            state.MarkComplete();
        }
    }

    public void FixedUpdateState() { }

    public void ExitState(StateType toState) { }
}