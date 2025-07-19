using UnityEngine;

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

    public override bool CheckCondition()
    {
        to = null;

        float pitch = beh.pitchLerper.Value();
        bool UP = input.Up(ButtonState.DOWN);
        bool DOWN = input.Down(ButtonState.DOWN);
        bool BACK = Equals(input.GetTiltDirection(), Tilt.Backward);
        
        // Straight
        //     Pitch Angle 0
        //     No Vertical Input
        if (pitch == 0 && input.Y == 0)
        {
            to = Straight;
        }

        // PitchUpRaising
        //     Pitch Angle > 0
        //     Up Input
        if (pitch >= 0 && UP)
        {
            to = PitchUpRaising;
        }
        // PitchUpFalling
        //     Pitch Angle > 0
        //     No Up Input
        if (pitch > 0 && !UP)
        {
            to = PitchUpFalling;
        }

        // PitchDownFalling
        //     Pitch Angle < 0
        //     Down Input
        if (pitch <= 0 && DOWN)
        {
            to = PitchDownFalling;
        }

        // PitchDownRaising
        //     Pitch Angle < 0
        //     No Down Input
        if (pitch < 0 && !DOWN)
        {
            to = PitchDownRaising;
        }

        // PitchUpFull
        //     Pitch Angle >= PitchUpRaising.MaxPitch
        //     Back Input + Up Input

        // UpsideDown
        //     Pitch Angle >= PitchUpFill.MaxPitch
        //     Back Input + No Up Input

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

        return to != null;
    }
}
