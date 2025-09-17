using System.Runtime.CompilerServices;
using UnityEngine;
using static ButtonType;

public class VariableJumpBehavior : StateBehavior, IStateBehavior
{
    private bool jumpReleased;

    public void StartState()
    {
        jumpReleased = false;
    }

    public void UpdateState()
    {
        if (!jumpReleased && Jump.Released() && body.velocity.y > 1)
        {
            body.velocity.y /= 4;
            jumpReleased = true;
        }
    }

    public void FixedUpdateState() { }

    public void ExitState(StateType toState) { }
}