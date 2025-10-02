using System;
using System.Collections.Generic;
using static StateType;
using static StateTypeWrapper;

/*
    This is the partial AtlasTransitionManager class which holds the Dictionary
    for defining which StateTypes each State can Transition into

    The Keys in the Dictionary are simply the StateTypes of the State that is
    checking for an available condition each frame.

    The Values are each Lists of StateTypeWrappers. These need to have their Value
    method called on them in order to return the StateType they wrap.
    There are special StateTypeWrappers which will only return the StateType
    upon a conditional statement.

    We use the helper function Can which is defined as a static method of the
    StateTypeWrapper class. Can takes any number of StateTypes or StateTypeWrappers
    and creates a list of new StateTypeWrappers.
        - If Can receives a StateType it turns it into a StateTypeWrapper with no
        condition
        - If Can receives a StateTypeWrapper it will keep it as is

    Each member inside the Can will be checked in descending order by 
    StateTransition.TryGetFirstActiveTransition which will find the first transition
    condition that passes and return that StateType. So priority is ordered
    descendingly
*/
public partial class AtlasTransitionManager
{
    private void InitializeCanTransitions()
    {
        CanTransitions = new Dictionary<StateType, List<StateTypeWrapper>>()
        {
            [Straight] = Can(
                PU_Full,
                HoldBack,
                PU_Rising,
                PD_Falling
            ),
            [PU_Rising] = Can(
                PU_Full,
                PU_Falling,
                PD_Falling
            ),
            [PU_Falling] = Can(
                PU_Rising,
                Straight,
                PD_Falling
            ),
            [PD_Falling] = Can(
                PD_HoldBack,
                PD_Rising,
                PU_Rising
            ),
            [PD_Rising] = Can(
                PD_Falling,
                Straight,
                PU_Rising
            ),
            [PU_Full] = Can(
                UpsideDown,
                PU_Falling
            ),
            [UpsideDown] = Can(
                RollOver,
                UD_PURising,
                UD_PDFalling
            ),
            [UD_PURising] = Can(
                UD_PDFalling,
                UD_PUFalling,
                PU_Full
            ),
            [UD_PUFalling] = Can(
                UD_PDFalling,
                UpsideDown,
                UD_PURising
            ),
            [UD_PDFalling] = Can(
                UD_PDRising,
                CompleteLoop,
                UpsideDown
            ),
            [UD_PDRising] = Can(
                UpsideDown,
                UD_PURising,
                UD_PDFalling
            ),
            [CompleteLoop] = Can(
                PD_Rising
            ),
            [RollOver] = Can(
                OnAnimationEnd(Straight)
            ),
            [HoldBack] = Can(
                TurnAround,
                PD_HoldBack,
                Straight
            ),
            [PD_HoldBack] = Can(
                HoldBack,
                TurnAround,
                PD_Falling,
                PD_Rising
            ),
            [TurnAround] = Can(
                OnAnimationEnd(Straight)
            ),
            [Bonk] = Can(
                OnAnimationEnd(Fall)
            ),
            [su_Broom] = Can(
                SelectAttack(),
                Bonk,
                Fall
            ),
            [su_Attack] = Can(
                OnAnimationEnd(Walk),
                OnAnimationEnd(Fall)
            ),
            [su_Movement] = Can(
                SelectAttack(),
                BroomStart
            ),
            [su_Jump] = Can(
                SelectAttack(),
                BroomStart
            ),
            [BroomStart] = Can(
                OnComplete(Straight)
            ),
            [Crouch] = Can(
                Fall,
                Walk,
                Slide,
                DownTilt
            ),
            [DownAir] = Can(),
            [DownTilt] = Can(
                OnAnimationEnd(Crouch)
            ),
            [Fall] = Can(
                Walk
            ),
            [FallingNair] = Can(),
            [Hurt] = Can(
                OnAnimationEnd(Fall),
                OnAnimationEnd(Walk)
            ),
            [Jab1] = Can(),
            [Jab2] = Can(),
            [Jab3] = Can(),
            [Jump] = Can(
                Fall,
                OnComplete(Walk) // Once Atlas has left the ground
            ),
            [RisingNair] = Can(),
            [Slide] = Can(
                SpinJump,
                OnComplete(Crouch),
                OnComplete(Walk)
            ),
            [Slip] = Can(),
            [SpinJump] = Can(
                BroomStart,
                OnAnimationEnd(Fall),
                OnAnimationEnd(Walk)
            ),
            [UpAir] = Can(),
            [UpTilt] = Can(),
            [Wait] = Can(),
            [Walk] = Can(
                Jump,
                Fall,
                Crouch
            ),
            [WallJump] = Can(
                OnComplete(Jump),
                OnComplete(Fall)
            ),
            [WallSlide] = Can(
                Fall,
                WallJump
            )
        };
    }

    public StateTypeWrapper SelectAttack()
    {
        return new AttackStateTypeWrapper();
    }
}