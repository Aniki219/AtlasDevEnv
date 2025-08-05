using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using Try = System.Func<State>;
using Can = System.Collections.Generic.List<System.Func<State>>;
using UnityEngine.Experimental.GlobalIllumination;

public class LedgeTransitions : StateTransition
{
    [SerializeField] State LedgeGrab;
    [SerializeField] State LedgeGrabLookBack;
    [SerializeField] State LedgeHop;
    [SerializeField] State LedgeGetUpStand;
    [SerializeField] State LedgeGetUpCrouch;
    [SerializeField] State Fall;
    [SerializeField] State FallingNair;
    [SerializeField] State RisingNair;

    //[SerializeField] StateTransition ToLedgeGrab;

    private Dictionary<State, Can> Transitions = new Dictionary<State, Can>();

    private bool Up => input.Up();
    private bool Down => input.Down();
    private bool Back => input.Back();
    private bool Forward => input.Forward();
    private bool Jump => input.Jump();
    private bool Attack => input.Attack();

    Try ToLedgeGrab => () => !Back ? LedgeGrab : null;
    Try ToLedgeGrabLookBack => () => Back ? LedgeGrabLookBack : null;
    Try ToLedgeHop => () => Jump ? LedgeHop : null;
    Try ToLedgeGetUpStand => () => Forward || Up ? LedgeGetUpStand : null;
    Try ToLedgeGetUpCrouch => () => Forward || Up ? LedgeGetUpCrouch : null;
    Try ToRelease => () => Down || Jump ? Fall : null;
    Try ToReleaseBack => () => Down ? Fall : null;
    Try ToAttack => () => Attack ? FallingNair : null;

    public void Start()
    {
        Transitions = new Dictionary<State, Can>()
        {
            [LedgeGrab] = new Can { ToLedgeGrabLookBack, ToRelease, ToLedgeGetUpStand, ToLedgeGetUpCrouch, ToAttack },
            [LedgeGrabLookBack] = new Can { ToLedgeGrab, ToReleaseBack, ToLedgeHop, ToAttack },
            [LedgeHop] = new Can { ToRelease },
            [LedgeGetUpStand] = new Can { ToRelease },
            [LedgeGetUpCrouch] = new Can { ToRelease },
        };
    }

    public override bool CheckCondition()
    {
        to = Transitions.GetValueOrDefault(stateMachine.currentState)
        ?.Select(transition => transition())
        .FirstOrDefault(result => result != null);

        return to != null && !Equals(to, state);
    }
}

