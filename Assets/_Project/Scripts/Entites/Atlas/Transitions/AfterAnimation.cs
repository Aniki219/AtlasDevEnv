using UnityEngine;

//TODO Make EventTransition
public class AfterAnimation : StateTransition
{
    public override bool CheckCondition()
    {
        return sprite.GetNormalizedTime() >= 1;
    }
}