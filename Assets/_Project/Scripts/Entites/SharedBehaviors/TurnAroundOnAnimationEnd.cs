using UnityEngine;

/*
    Turns the Entity around when the AnimationEnd event of the
    EntityController triggers.
    
    This could be extended to:
        - Check if the animation was an override animation
        - Get the animation clip name
        - Choose whether to flip the sprite or just the facing
*/
public class TurnAroundOnAnimEnd : StateBehavior
{
    public void OnEnable()
    {
        sprite.OnAnimationEnd.AddListener(entity.TurnAround);
    }

    public void OnDisable()
    {
        sprite.OnAnimationEnd.RemoveListener(entity.TurnAround);
    }
}
