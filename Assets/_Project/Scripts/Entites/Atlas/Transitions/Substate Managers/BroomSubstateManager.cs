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
    [SerializeField] State PitchToStraight;
    [SerializeField] State RollOver;
    [SerializeField] State StraightHoldBack;
    [SerializeField] State PitchDownHoldBack;
    [SerializeField] State TurnAround;

    [SerializeField] float angleWrapCutoff;
    [SerializeField] float breakpointThreshold;

    private float SinPitch => Mathf.Sin(beh.pitchLerper.Value() * Mathf.Deg2Rad);
    private bool Up => input.Up(ButtonState.DOWN);
    private bool Down => input.Down(ButtonState.DOWN);
    private bool Back => input.Back();

    Try ToStraight          => () => Mathf.Approximately(SinPitch, 0f) ? Straight : null;
    Try ToPitchUpRaising    => () => SinPitch >= 0 && Up ? PitchUpRaising : null;
    Try ToPitchUpFalling    => () => SinPitch > 0 && !Up ? PitchUpFalling : null;
    Try ToPitchDownFalling  => () => SinPitch <= 0 && Down ? PitchDownFalling : null;
    Try ToPitchDownRaising  => () => SinPitch < 0 && !Down ? PitchDownRaising : null;
    Try ToPitchUpFull       => () => SinPitch >= PitchBreakpoint(PitchUpRaising) && Back && Up ? PitchUpFull : null;
    Try ToUpsideDown        => () => SinPitch >= PitchBreakpoint(PitchUpFull) && Back && !Up ? UpsideDown : null;
    Try ToUpsideDownFalling => () => SinPitch <= PitchBreakpoint(UpsideDown) && Down ? UpsideDownFalling : null;
    Try ToAngleWrap         => () => isPassedWrapAngle() ? PitchDownRaising : null;
    // Try ToRollOver          => () => ? RollOver : null;
    Try ToStraightHoldBack  => () => SinPitch == 0 && Back ? StraightHoldBack : null;
    Try ToPitchDownHoldBack => () => SinPitch < 0 && Back ? PitchDownHoldBack : null;
    // Try ToTurnAround        => () => ? TurnAround : null;


    float PitchBreakpoint(State state)
    {
        BroomTargetPitchBehavior comp = state.GetComponentInChildren<BroomTargetPitchBehavior>();
        if (!comp) return Mathf.Infinity;
        return Mathf.Sin(comp.targetPitch * Mathf.Deg2Rad) * breakpointThreshold;
    }

    private Dictionary<State, Can> Transitions = new Dictionary<State, Can>();

    
    public void Start()
    {
        Transitions = new Dictionary<State, Can>()
        {
            [Straight] = new Can { ToPitchUpRaising, ToPitchDownFalling },
            [PitchUpRaising] = new Can { ToPitchUpFull, ToPitchUpFalling, ToPitchDownFalling },
            [PitchUpFalling] = new Can { ToPitchUpRaising, ToStraight, ToPitchDownFalling },
            [PitchDownFalling] = new Can { ToPitchDownRaising, ToPitchUpRaising },
            [PitchDownRaising] = new Can { ToPitchDownFalling, ToStraight, ToPitchUpRaising },
            [PitchUpFull] = new Can { ToUpsideDown, ToPitchUpFalling },
            [UpsideDown] = new Can { ToUpsideDownFalling },
            [UpsideDownFalling] = new Can { ToAngleWrap },
            //[RollOver]        = new List<Func<State>> { },
            [StraightHoldBack] = new Can { },
            [PitchDownHoldBack] = new Can { },
            //[TurnAround]      = new List<Func<State>> { },
        };
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

    public override bool CheckCondition()
    {
        to = Transitions.GetValueOrDefault(stateMachine.currentState)
        ?.Select(transition => transition())
        .FirstOrDefault(result => result != null);

        return to != null && !Equals(to, state);
    }
}
