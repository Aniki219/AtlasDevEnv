using UnityEngine;

public class LedgeGrabBehavior : StateBehavior, IStateBehavior
{

    public void StartState()
    {
        body.canGravity = false;
        body.ResetGravity();
        body.SetVelocity(Vector2.zero);
    }

    public void UpdateState() { }

    public void FixedUpdateState() { }

    public void ExitState()
    {
        body.canGravity = true;
    }
}