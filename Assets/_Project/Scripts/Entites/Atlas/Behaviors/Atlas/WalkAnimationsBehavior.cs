using UnityEngine;
using static AtlasHelpers;

public class WalkAnimationsBehavior : StateBehavior, IStateBehavior
{
    [SerializeField] AnimationClip turnAroundClip;
    [SerializeField] AnimationClip walkClip;

    public void StartState() { }

    public void UpdateState()
    {
        if (!sprite.IsOverrideClip(turnAroundClip))
        {
            if (input.X != 0)
            {
                if (!SameSign(input.X, body.velocity.x))
                {
                    sprite.SetOverrideClip(turnAroundClip);
                }
                else
                {
                    sprite.SetOverrideClip(walkClip);
                }
            }
            else
            {
                sprite.ClearOverrideClip();
            }
        }
    }

    public void FixedUpdateState() { }

    public void ExitState() { }
}