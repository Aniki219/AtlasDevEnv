using UnityEngine;

public class Resetters : StateBehavior, IStateBehavior
{

    [SerializeField] bool resetGravity;
    [SerializeField] bool resetVelocity;

    public void StartState()
    {
        if (resetGravity) body.ResetGravity();
        if (resetVelocity) body.SetTargetVelocity(Vector2.zero);
    }

    public void UpdateState() { }

    public void FixedUpdateState() { }

    public void ExitState(StateType toState) { }
}