using UnityEngine;

public class BroomTargetPitchBehavior : StateBehavior, IStateBehavior
{
    public float targetPitch;

    [SerializeField] BroomBehavior broomBehaviorRef;

    public void ExitState()
    {
    }

    public void FixedUpdateState()
    {
    }

    public void StartState()
    {
        broomBehaviorRef.pitchLerper = new RateLerper(
            broomBehaviorRef.pitchLerper.Value(),
            targetPitch,
            broomBehaviorRef.pitchRate,
            Time.time
        );
    }

    public void UpdateState()
    {
    }
}