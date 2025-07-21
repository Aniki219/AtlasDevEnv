using System;
using System.Collections.Generic;
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
    [SerializeField] State UpsideDownFalling;
    [SerializeField] State PitchToStraight;
    [SerializeField] State RollOver;
    [SerializeField] State StraightHoldBack;
    [SerializeField] State PitchDownHoldBack;
    [SerializeField] State TurnAround;

    [SerializeField] float angleWrapCutoff;
    [SerializeField] float breakpointThreshold;
    [SerializeField] float sinPitch;
    [SerializeField] float targetPitch;
    [SerializeField] bool UP;
    [SerializeField] bool DOWN;
    [SerializeField] bool BACK;
    [SerializeField] bool FORWARD;

    State TryStraight => sinPitch == 0 ? Straight : null;
    State TryPitchUpRaising => sinPitch >= 0 && UP ? PitchUpRaising : null;
    State TryPitchUpFalling => sinPitch > 0 && !UP ? PitchUpFalling : null;
    State TryPitchDownFalling => sinPitch <= 0 && DOWN ? PitchDownFalling : null;
    State TryPitchDownRaising => sinPitch < 0 && !DOWN ? PitchDownRaising : null;
    State TryPitchUpFull => sinPitch >= PitchBreakpoint(PitchUpRaising) && BACK && UP ? PitchUpFull : null;
    State TryUpsideDown => sinPitch >= PitchBreakpoint(PitchUpFull) && BACK && !UP ? UpsideDown : null;
    State TryUpsideDownFalling => sinPitch <= PitchBreakpoint(UpsideDown) && DOWN ? UpsideDownFalling : null;
    State TryAngleWrap => isPassedWrapAngle() ? PitchDownRaising : null;
    //State TryRollOver => ? RollOver : null;
    State TryStraightHoldBack => sinPitch == 0 && BACK ? StraightHoldBack : null;
    State TryPitchDownHoldBack => sinPitch < 0 && BACK ? PitchDownHoldBack : null;
    //State TryTurnAround => ? TurnAround : null;


    float PitchBreakpoint(State state)
    {
        BroomTargetPitchBehavior comp = state.GetComponentInChildren<BroomTargetPitchBehavior>();
        if (!comp) return Mathf.Infinity;
        return Mathf.Sin(comp.targetPitch * Mathf.Deg2Rad) * breakpointThreshold;
    }

    Dictionary<State, List<State>> Transitions = new Dictionary<State, List<State>>();

    public void Update()
    {
        updateStateVariables();
        Transitions = new Dictionary<State, List<State>>()
        {
            [Straight]          = new List<State> { TryPitchUpRaising,   TryPitchDownFalling                     },
            [PitchUpRaising]    = new List<State> { TryPitchUpFull,      TryPitchUpFalling,  TryPitchDownFalling },
            [PitchUpFalling]    = new List<State> { TryPitchUpRaising,   TryStraight,        TryPitchDownFalling },
            [PitchDownFalling]  = new List<State> { TryPitchDownRaising, TryPitchUpRaising                       },
            [PitchDownRaising]  = new List<State> { TryPitchDownFalling, TryStraight,        TryPitchUpRaising   },
            [PitchUpFull]       = new List<State> { TryPitchUpFalling,   TryUpsideDown                           },
            [UpsideDown]        = new List<State> { TryUpsideDownFalling                                         },
            [UpsideDownFalling] = new List<State> { TryAngleWrap                                                 },
            //[RollOver]        = new List<State> { },
            [StraightHoldBack]  = new List<State> { },
            [PitchDownHoldBack] = new List<State> { },
            //[TurnAround]      = new List<State> { },
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
        to = null;

        List<State> transitions = new List<State>();
        if (Transitions.TryGetValue(stateMachine.currentState, out transitions))
        {
            transitions.ForEach(transition => { if (transition) to = transition; });
        }

        return to != null && !Equals(to, state);
    }

    private void updateStateVariables()
    {
        sinPitch = Mathf.Sin(beh.pitchLerper.Value() * Mathf.Deg2Rad);
        UP = input.Up(ButtonState.DOWN);
        DOWN = input.Down(ButtonState.DOWN);
        BACK = input.Back();
        FORWARD = input.Forward();
    }
}
