using UnityEngine;
using static AtlasHelpers;

public class CrouchAnimationsBehavior : StateBehavior, IStateBehavior
{
    [SerializeField] AnimationClip crouchClip;
    [SerializeField] AnimationClip crawlClip;

    public void StartState()
    {
        sprite.SetOverrideClip(crouchClip);
    }

    public void UpdateState()
    {
        if (!sprite.IsOverrideClip(crouchClip) && input.X != 0)
        {
            sprite.SetOverrideClip(crawlClip);
        }
        else
        {
            sprite.ClearOverrideClip();
        }
    }

    public void FixedUpdateState() { }

    public void ExitState() { }
}