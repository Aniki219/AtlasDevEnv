using System.Runtime.CompilerServices;
using UnityEngine;

public class JumpBehavior : StateBehavior, IStateBehavior
{
    [SerializeField] float apex = 5f;
    [SerializeField] float jumpDistance = 0;

    public void StartState()
    {
        float g = body.GetGravity();
        float viy = Mathf.Sqrt(-2 * apex * g);
        float apexTime = viy / -g;
        float vix = jumpDistance / (2 * apexTime);

        body.velocity.y = viy;
        if (vix > 0) body.SetForwardVelocity(vix);
    }

    public void UpdateState() { }

    public void FixedUpdateState() { }

    public void ExitState() { }
}