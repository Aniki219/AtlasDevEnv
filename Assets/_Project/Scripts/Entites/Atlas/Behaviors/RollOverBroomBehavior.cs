using UnityEngine;

public class RollOverBroomBehavior : StateBehavior, IStateBehavior
{
    public BroomBehavior broomBehavior;

    public void ExitState() {}

    public void FixedUpdateState() {}

    public void StartState()
    {
        entity.TurnAround();
        broomBehavior.pitchLerper = new RateLerper();
    }

    public void UpdateState() {}
}
