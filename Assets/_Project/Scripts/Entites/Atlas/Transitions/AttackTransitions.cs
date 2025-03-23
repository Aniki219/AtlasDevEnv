using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class AttackTransitions : StateTransition
{
    public State Attack;
    public State AttackRising;
    public State AttackFalling;
    public State AttackUp;
    public State AttackDown;

    public State Special;
    public State UpSpecial;
    public State DownSpecial;
    public State SideSpecial;

    public override bool CheckCondition()
    {
        to = null;

        if (AttackRising && AttackFalling)
        {
            Attack = body.velocity.y > 0 ? AttackRising : AttackFalling;
        }

        if (input.AttackUp()) { to = AttackUp; }
        if (input.AttackDown()) { to = AttackDown; }
        if (input.Attack()) { to = Attack; }
        if (input.AttackForward()) { to = Attack; }
        if (input.AttackBack()) { to = Attack; }
        if (input.UpSpecial()) { to = UpSpecial; }
        if (input.DownSpecial()) { to = DownSpecial; }
        if (input.SideSpecial()) { to = SideSpecial; }
        if (input.Special()) { to = Special; }

        return to != null;
    }
}