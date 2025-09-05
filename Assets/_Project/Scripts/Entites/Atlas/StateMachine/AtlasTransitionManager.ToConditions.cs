using UnityEngine;
using System;
using System.Collections.Generic;

using static StateType;
using static StateTransitionBuilder;

using bt = ButtonType;

/*
    This is a partial AtlasTransitionManager class which holds the Dictionary
    describing the transition conditions for each StateType.

    In general each StateType has a condition that must be met to transition
    to it. However for certain combinations of States, there may be a special
    condition describing that relationship.

    For example, it is possible to transition to the Fall State from any Broom
    State by pressing the Boom button. But not all states that can transition to
    the Fall state should be able to do so via pressing the Broom button.

    We created a To().From() syntax to create the Keys for this Dictionary.
    - To(StateType) specifies how to transition to that State. 
    - From(StateType) is entirely optional but when used will only allow the To
    to apply when the current state matches the From state.

    The StateTransition.TryGetFirstActiveTransition method will first attempt to
    lookup the To().From() key and only if that key does not exist will it then
    check for a To() key.
*/
public partial class AtlasTransitionManager
{
    private void InitializeToConditions()
    {
        ToConditions = new Dictionary<(StateType to, StateType? from), Func<bool>>
        {
            [To(BroomStart)] = () => bt.Broom.Pressed(),
            [To(Straight)] = () => Mathf.Approximately(SinPitch, 0f),
            [To(Straight)
                .From(HoldBack)] = () => Mathf.Approximately(SinPitch, 0f) && !Back,

            [To(PU_Rising)] = () => SinPitch >= 0 && Up,
            [To(PU_Falling)] = () => SinPitch > 0 && !Up,
            [To(PD_Falling)] = () => SinPitch <= 0 && Down && !Back,
            [To(PD_Rising)] = () => SinPitch < 0 && !Down,
            [To(PU_Full)] = () => SinPitch >= PitchBreakpoint(st_PU_Rising) && Back && Up,
            [To(PU_Full)
                .From(UD_PURising)] = () => SinPitch >= PitchBreakpoint(st_PU_Rising) && Forward && Up,

            [To(UpsideDown)] = () => SinPitch >= PitchBreakpoint(st_PU_Full) && Back && !Up,
            [To(UpsideDown)
                .From(UD_PUFalling)] = () => SinPitch >= PitchBreakpoint(st_PU_Full) && !Up,
            [To(UpsideDown)
                .From(UD_Rising)] = () => SinPitch >= PitchBreakpoint(st_PU_Full) && !Up,
            [To(UD_Falling)] = () => SinPitch <= PitchBreakpoint(st_UpsideDown) && Down,
            [To(UD_Rising)] = () => SinPitch < PitchBreakpoint(st_UpsideDown) && !Down,
            [To(UD_PURising)] = () => SinPitch >= PitchBreakpoint(st_UpsideDown) && Up,
            [To(UD_PUFalling)] = () => SinPitch > PitchBreakpoint(st_UpsideDown) && !Up,
            [To(CompleteLoop)] = () => SinPitch <= PitchBreakpoint(st_UD_Falling) && Down && Forward,
            [To(PD_HoldBack)] = () => SinPitch < 0 && Back,

            [To(PD_Rising)
                .From(CompleteLoop)] = () => isPassedWrapAngle(),

            [To(HoldBack)] = () => Mathf.Approximately(SinPitch, 0f) && Back,
            [To(HoldBack)] = () => Mathf.Approximately(SinPitch, 0f) && Back,
            [To(RollOver)] = () => InStateForSeconds(2.0f),
            [To(TurnAround)] = () => InStateForSeconds(0.5f) && Back,
            [To(TurnAround)
                .From(PD_HoldBack)] = () => InStateForSeconds(0.1f) && Back,
            [To(Walk)] = () => body.IsGrounded(),
            [To(Walk)
                .From(Crouch)] = () => !bt.Down.Held(),

            [To(Fall)] = () => !body.IsGrounded() && body.velocity.y <= 0,
            [To(Fall)
                .From(su_Broom)] = () => bt.Broom.Pressed(),
            [To(Fall)
                .From(WallSlide)] = () => !PushAgainstWall(),

            [To(Jump)] = () => bt.Jump.Pressed(),

            [To(Jab1)] = () => bt.Attack.Pressed(),
            [To(Jab2)] = () => bt.Attack.Pressed(),
            [To(Jab3)] = () => bt.Attack.Pressed(),
            [To(UpTilt)] = () => bt.Attack.UpTilt().Pressed(),
            [To(DownTilt)] = () => bt.Attack.DownTilt().Pressed(),
            [To(UpAir)] = () => bt.Attack.UpTilt().Pressed() && !body.IsGrounded(),
            [To(DownAir)] = () => bt.Attack.DownTilt().Pressed() && !body.IsGrounded(),
            [To(FallingNair)] = () => bt.Attack.Pressed() && !body.IsGrounded(),
            [To(RisingNair)] = () => bt.Attack.Pressed() && !body.IsGrounded(),

            [To(Hurt)] = () => true,
            [To(Bonk)] = () => false,
            [To(SpinJump)] = () => bt.Jump.Pressed(),
            [To(WallJump)] = () => bt.Jump.Pressed(),
            [To(WallSlide)] = () => PushAgainstWall(),
            [To(Crouch)] = () => bt.Down.Held(),

            [To(Slide)] = () => bt.Down.Held() && bt.Jump.Pressed(),
            [To(Slip)] = () => false,
            [To(Dash)] = () => false,
            [To(Wait)] = () => false,
        };
    }
}