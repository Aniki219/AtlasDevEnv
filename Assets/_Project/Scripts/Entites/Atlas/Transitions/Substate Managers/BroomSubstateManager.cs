using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions.Must;

using Try = System.Func<State>;
using Can = System.Collections.Generic.List<System.Func<State>>;

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
    [SerializeField] State UpsideDownFalling;
    [SerializeField] State RollOver;
    [SerializeField] State StraightHoldBack;
    [SerializeField] State PitchDownHoldBack;
    [SerializeField] State TurnAround;

    [SerializeField] float angleWrapCutoff;
    [SerializeField] float breakpointThreshold;

    private Dictionary<State, Can> Transitions = new Dictionary<State, Can>();

    private float SinPitch  => Mathf.Sin(beh.pitchLerper.Value() * Mathf.Deg2Rad);
    private bool Up         => input.Up(ButtonState.DOWN);
    private bool Down       => input.Down(ButtonState.DOWN);
    private bool Back       => input.Back();

    Try ToStraight          => () => Mathf.Approximately(SinPitch, 0f) && !Back ? Straight : null;
    Try ToPitchUpRaising    => () => SinPitch >= 0 && Up ? PitchUpRaising : null;
    Try ToPitchUpFalling    => () => SinPitch > 0 && !Up ? PitchUpFalling : null;
    Try ToPitchDownFalling  => () => SinPitch <= 0 && Down && !Back ? PitchDownFalling : null;
    Try ToPitchDownRaising  => () => SinPitch < 0 && !Down ? PitchDownRaising : null;
    Try ToPitchUpFull       => () => SinPitch >= PitchBreakpoint(PitchUpRaising) && Back && Up ? PitchUpFull : null;
    Try ToUpsideDown        => () => SinPitch >= PitchBreakpoint(PitchUpFull) && Back && !Up ? UpsideDown : null;
    Try ToUpsideDownFalling => () => SinPitch <= PitchBreakpoint(UpsideDown) && Down ? UpsideDownFalling : null;
    Try ToAngleWrap         => () => isPassedWrapAngle() ? PitchDownRaising : null;
    Try ToRollOver          => () => InStateForSeconds(1.0f) ? RollOver : null;
    Try ToStraightHoldBack  => () => Mathf.Approximately(SinPitch, 0f) && Back ? StraightHoldBack : null;
    Try ToPitchDownHoldBack => () => SinPitch < 0 && Back ? PitchDownHoldBack : null;
    Try ToTurnAround        => () => InStateForSeconds(0.5f) && Back ? TurnAround : null;

    public void Start()
    {
        Transitions = new Dictionary<State, Can>()
        {
            [Straight]          = new Can { ToStraightHoldBack, ToPitchUpRaising, ToPitchDownFalling },
            [PitchUpRaising]    = new Can { ToPitchUpFull, ToPitchUpFalling, ToPitchDownFalling },
            [PitchUpFalling]    = new Can { ToPitchUpRaising, ToStraight, ToPitchDownFalling },
            [PitchDownFalling]  = new Can { ToPitchDownRaising, ToPitchUpRaising, ToPitchDownHoldBack },
            [PitchDownRaising]  = new Can { ToPitchDownFalling, ToStraight, ToPitchUpRaising },
            [PitchUpFull]       = new Can { ToUpsideDown, ToPitchUpFalling },
            [UpsideDown]        = new Can { ToUpsideDownFalling, ToRollOver },
            [UpsideDownFalling] = new Can { ToAngleWrap },
            [RollOver]          = new Can { ToStraight },
            [StraightHoldBack]  = new Can { ToStraight, ToTurnAround, ToPitchDownHoldBack },
            [PitchDownHoldBack] = new Can { ToStraightHoldBack, ToPitchDownFalling, ToPitchDownRaising, ToTurnAround },
            [TurnAround]        = new Can { ToStraight, ToPitchDownFalling, ToPitchDownRaising },
        };
    }

    public override bool CheckCondition()
    {
        to = Transitions.GetValueOrDefault(stateMachine.currentState)
        ?.Select(transition => transition())
        .FirstOrDefault(result => result != null);

        return to != null && !Equals(to, state);
    }

    public bool isPassedWrapAngle()
    {
        float pitch = beh.pitchLerper.Value();
        if (pitch >= angleWrapCutoff)
        {
            beh.pitchLerper = new RateLerper(angleWrapCutoff - 360, 0, beh.pitchRate, Time.time);
            return true;
        }
        return false;
    }

    float PitchBreakpoint(State state)
    {
        BroomTargetPitchBehavior comp = state.GetComponentInChildren<BroomTargetPitchBehavior>();
        if (!comp) return Mathf.Infinity;
        return Mathf.Sin(comp.targetPitch * Mathf.Deg2Rad) * breakpointThreshold;
    }

    private bool InStateForSeconds(float seconds)
    {
        return stateMachine.currentState.GetElapsedTime() > seconds;
    }
}
