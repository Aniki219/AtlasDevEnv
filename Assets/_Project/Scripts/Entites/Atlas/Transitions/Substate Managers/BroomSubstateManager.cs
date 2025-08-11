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
    [SerializeField] State UpsideDownRising;
    [SerializeField] State UpsideDownPitchUpFalling;
    [SerializeField] State UpsideDownPitchUpRising;
    [SerializeField] State CompleteLoop;
    [SerializeField] State RollOver;
    [SerializeField] State StraightHoldBack;
    [SerializeField] State PitchDownHoldBack;
    [SerializeField] State TurnAround;

    [SerializeField] float angleWrapCutoff;
    [SerializeField] float breakpointThreshold;

    private Dictionary<State, Can> Transitions = new Dictionary<State, Can>();

    private float SinPitch => Mathf.Round(Mathf.Sin(beh.pitchLerper.Value() * Mathf.Deg2Rad));
    private bool Up => input.Up(ButtonState.DOWN);
    private bool Down => input.Down(ButtonState.DOWN);
    private bool Back => input.Back();
    private bool Forward => input.Forward();

    Func<State> ToStraight => () => Mathf.Approximately(SinPitch, 0f) ? Straight : null;

    Func<State> ToPitchUpRaising => () => SinPitch >= 0 && Up ? PitchUpRaising : null;
    Func<State> ToPitchUpFalling => () => SinPitch > 0 && !Up ? PitchUpFalling : null;
    Func<State> ToPitchDownFalling => () => SinPitch <= 0 && Down && !Back ? PitchDownFalling : null;
    Func<State> ToPitchDownRaising => () => SinPitch < 0 && !Down ? PitchDownRaising : null;
    Func<State> ToPitchUpFull => () => SinPitch >= PitchBreakpoint(PitchUpRaising) && Back && Up ? PitchUpFull : null;
    Func<State> ToPitchUpFullUpsideDown => () => SinPitch >= PitchBreakpoint(PitchUpRaising) && Forward && Up ? PitchUpFull : null;

    Func<State> ToUpsideDown => () => SinPitch >= PitchBreakpoint(PitchUpFull) && Back && !Up ? UpsideDown : null;
    Func<State> ToUpsideDownNoBack => () => SinPitch >= PitchBreakpoint(PitchUpFull) && !Up ? UpsideDown : null;
    Func<State> ToUpsideDownFalling => () => SinPitch <= PitchBreakpoint(UpsideDown) && Down ? UpsideDownFalling : null;
    Func<State> ToUpsideDownRising => () => SinPitch < PitchBreakpoint(UpsideDown) && !Down ? UpsideDownRising : null;
    Func<State> ToUpsideDownPitchUpRising => () => SinPitch >= PitchBreakpoint(UpsideDown) && Up ? UpsideDownPitchUpRising : null;
    Func<State> ToUpsideDownPitchUpFalling => () => SinPitch > PitchBreakpoint(UpsideDown) && !Up ? UpsideDownPitchUpFalling : null;
    Func<State> ToRollOver => () => InStateForSeconds(2.0f) ? RollOver : null;

    Func<State> ToCompleteLoop => () => SinPitch <= PitchBreakpoint(UpsideDownFalling) && Down && Forward ? CompleteLoop : null;
    Func<State> ToAngleWrap => () => isPassedWrapAngle() ? PitchDownRaising : null;

    Func<State> ToStraightHoldBack => () => Mathf.Approximately(SinPitch, 0f) && Back ? StraightHoldBack : null;
    Func<State> ToPitchDownHoldBack => () => SinPitch < 0 && Back ? PitchDownHoldBack : null;
    Func<State> ToTurnAround => () => InStateForSeconds(0.5f) && Back ? TurnAround : null;
    Func<State> ToDiveTurnAround => () => InStateForSeconds(0.1f) && Back ? TurnAround : null;

    public void Start()
    {
        Transitions = new Dictionary<State, Can>()
        {
            [Straight] = new Can { ToStraightHoldBack, ToPitchUpRaising, ToPitchDownFalling },
            [PitchUpRaising] = new Can { ToPitchUpFull, ToPitchUpFalling, ToPitchDownFalling },
            [PitchUpFalling] = new Can { ToPitchUpRaising, ToStraight, ToPitchDownFalling },
            [PitchDownFalling] = new Can { ToPitchDownHoldBack, ToPitchDownRaising, ToPitchUpRaising, },
            [PitchDownRaising] = new Can { ToPitchDownFalling, ToStraight, ToPitchUpRaising },
            [PitchUpFull] = new Can { ToUpsideDown, ToPitchUpFalling },
            [UpsideDown] = new Can { ToRollOver, ToUpsideDownPitchUpRising, ToUpsideDownFalling },
            [UpsideDownPitchUpRising] = new Can { ToUpsideDownFalling, ToUpsideDownNoBack, ToUpsideDownPitchUpFalling, ToPitchUpFullUpsideDown },
            [UpsideDownPitchUpFalling] = new Can { ToUpsideDownFalling, ToUpsideDownNoBack, ToUpsideDownPitchUpRising },
            [UpsideDownFalling] = new Can { ToUpsideDownRising, ToUpsideDown, ToCompleteLoop },
            [UpsideDownRising] = new Can { ToUpsideDown, ToUpsideDownPitchUpRising, ToUpsideDownFalling },
            [CompleteLoop] = new Can { ToAngleWrap },
            [RollOver] = new Can { /* Transitions to Straight on AnimEnd */ },
            [StraightHoldBack] = new Can { ToTurnAround, ToStraightHoldBack, ToStraight },
            [PitchDownHoldBack] = new Can { ToStraightHoldBack, ToDiveTurnAround, ToPitchDownFalling, ToPitchDownRaising, },
            [TurnAround] = new Can { },
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
        return Mathf.Round(Mathf.Sin(comp.targetPitch * Mathf.Deg2Rad) * breakpointThreshold);
    }

    private bool InStateForSeconds(float seconds)
    {
        return stateMachine.currentState.GetElapsedTime() > seconds;
    }
}
