using UnityEngine;

public class BroomTargetPitchBehavior : StateBehavior, IStateBehavior
{
    public float targetPitch;
    public float thrustDrag;

    [SerializeField] BroomBehavior broomBehaviorRef;

    public void ExitState()
    {
    }

    public void FixedUpdateState()
    {
    }

    public void StartState()
    {
        float pitch = broomBehaviorRef.pitchLerper.Value();

        broomBehaviorRef.pitchLerper = new RateLerper(
            pitch,
            targetPitch,
            broomBehaviorRef.pitchRate,
            Time.time
        );
    }

    public void UpdateState()
    {
        broomBehaviorRef.thrust -= thrustDrag * Time.deltaTime;
    }
}