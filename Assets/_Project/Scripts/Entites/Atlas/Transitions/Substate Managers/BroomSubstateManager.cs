using System;
using UnityEngine;
using UnityEngine.Assertions.Must;

public class BroomSubstateManager : StateTransition
{
    [SerializeField] BroomBehavior beh;
    [SerializeField] State Straight;
    [SerializeField] State PitchUpRaising;
    [SerializeField] State PitchUpFalling;
    [SerializeField] State PitchDownFalling;
    [SerializeField] State PitchDownRaising;
    [SerializeField] State PitchUpFull;
    [SerializeField] State UpsideDown;
    [SerializeField] State RollOver;
    [SerializeField] State StraightHoldBack;
    [SerializeField] State PitchDownHoldBack;
    [SerializeField] State TurnAround;

    [SerializeField] float sinPitch;
    [SerializeField] bool UP;
    [SerializeField] bool DOWN;
    [SerializeField] bool BACK;
    [SerializeField] bool canFullPitch;
    [SerializeField] bool canUpsideDown;

    [SerializeField] float targetPitch;

    public override bool CheckCondition()
    {
        to = null;
        sinPitch = Mathf.Sin(beh.pitchLerper.Value() * Mathf.Deg2Rad);
        UP = input.Up(ButtonState.DOWN);
        DOWN = input.Down(ButtonState.DOWN);
        BACK = input.Back();
        canFullPitch = sinPitch >= Mathf.Sin(
            PitchUpRaising
            .GetComponentInChildren<BroomTargetPitchBehavior>()
            .targetPitch *
            0.9f *
            Mathf.Deg2Rad
        );
        canUpsideDown = sinPitch >= Mathf.Sin(
            PitchUpFull
            .GetComponentInChildren<BroomTargetPitchBehavior>()
            .targetPitch *
            0.9f *
            Mathf.Deg2Rad
        );

        // Straight
        //     Pitch Angle 0
        //     No Vertical Input
        if (sinPitch == 0 && input.Y == 0)
        {
            to = Straight;
        }

        // PitchUpRaising
        //     Pitch Angle > 0
        //     Up Input
        if (sinPitch >= 0 && UP)
        {
            to = PitchUpRaising;
        }
        // PitchUpFalling
        //     Pitch Angle > 0
        //     No Up Input
        if (sinPitch > 0 && !UP)
        {
            to = PitchUpFalling;
        }

        // PitchDownFalling
        //     Pitch Angle < 0
        //     Down Input
        if (sinPitch <= 0 && DOWN)
        {
            to = PitchDownFalling;
        }

        // PitchDownRaising
        //     Pitch Angle < 0
        //     No Down Input
        if (sinPitch < 0 && !DOWN)
        {
            to = PitchDownRaising;
        }

        // PitchUpFull
        //     Pitch Angle >= PitchUpRaising.MaxPitch
        //     Back Input + Up Input
        if (canFullPitch && UP && BACK)
        {
            to = PitchUpFull;
        }

        // UpsideDown
        //     Pitch Angle >= PitchUpFill.MaxPitch
        //     Back Input + No Up Input
        if (canUpsideDown && !UP && BACK)
        {
            to = UpsideDown;
        }
        // RollOver
        //     Upside Down for longer than UpsideDown.duration

        // StraightHoldBack
        //     Straight + Back Input
        //     PitchUpFalling + Back Input
        //     PitchDownFalling + Back Input

        // PitchDownHoldBack
        //     PitchDownFalling + Back Input

        // TurnAround
        //     StraightHoldBack + TurnAround.RateLerp.EndEvent == 1 ||
        //     PitchDownHoldBack + TurnAround.RateLerp.EndEvent == 1

        return to != null && !Equals(to, state);
    }
}
