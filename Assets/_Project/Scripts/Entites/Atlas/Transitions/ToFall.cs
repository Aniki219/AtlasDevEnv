public class ToFall : StateTransition
{
    public override bool CheckCondition()
    {
        return !body.IsGrounded() && body.velocity.y <= 0;
    }
}