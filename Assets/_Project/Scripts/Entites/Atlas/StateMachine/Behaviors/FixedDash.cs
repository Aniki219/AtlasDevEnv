using System.Diagnostics.CodeAnalysis;
using UnityEngine;

public class FixedDash : StateBehavior, IStateBehavior
{
    [SerializeField] AnimationCurve velocityProfile;
    private float duration;

    public void StartState()
    {
        duration = velocityProfile[velocityProfile.length - 1].time;
        // sprite.CreateParticle(ParticleSystem.Dust);
    }

    public void UpdateState()
    {
        float t = state.GetNomalizedTime(duration);
        body.SetForwardVelocity(velocityProfile.Evaluate(t * duration));
        if (t >= 1)
        {
            state.MarkComplete();
        }
    }

    public void FixedUpdateState() { }

    public void ExitState() { }
}