using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using Can = System.Collections.Generic.List<StateType>;

using static StateType;

public class AtlasStateTransitions : StateTransition
{
    [SerializeField] float angleWrapCutoff;
    [SerializeField] float breakpointThreshold;

    private Dictionary<StateType, Can> Transitions;
    private Dictionary<(StateType to, StateType? from), Func<bool>> ToConditions;

    private State st_Broom => stateRegistry.GetState(Broom);
    private State st_PU_Rising => stateRegistry.GetState(PU_Rising);
    private State st_PU_Full => stateRegistry.GetState(PU_Full);
    private State st_UD_ => stateRegistry.GetState(UpsideDown);
    private State st_UD_Falling => stateRegistry.GetState(UD_Falling);
    
    private BroomBehavior _bh_Broom;
    private BroomBehavior bh_Broom => _bh_Broom ??= st_Broom.GetComponent<BroomBehavior>();

    private float SinPitch => Mathf.Round(Mathf.Sin(bh_Broom.pitchLerper.Value() * Mathf.Deg2Rad));
    private bool Up => input.Up(ButtonState.DOWN);
    private bool Down => input.Down(ButtonState.DOWN);
    private bool Back => input.Back();
    private bool Forward => input.Forward();

    public readonly struct StateTransitionBuilder
    {
        private readonly StateType _to;
        
        public StateTransitionBuilder(StateType to)
        {
            _to = to;
        }
        
        public (StateType, StateType?) From(StateType from)
        {
            return (_to, from);
        }
        
        // Implicit conversion to tuple when no From() is called
        public static implicit operator (StateType, StateType?)(StateTransitionBuilder builder)
        {
            return (builder._to, null);
        }
    }

    // Helper method to start the builder
    private StateTransitionBuilder To(StateType to)
    {
        return new StateTransitionBuilder(to);
    }

    public void Start()
    {
        ToConditions = new Dictionary<(StateType to, StateType? from), Func<bool>> {
            [To(Straight)]           = () => Mathf.Approximately(SinPitch, 0f),

            [To(PU_Rising)]          = () => SinPitch >= 0 && Up,
            [To(PU_Falling)]         = () => SinPitch > 0 && !Up,
            [To(PD_Falling)]         = () => SinPitch <= 0 && Down && !Back,
            [To(PD_Rising)]          = () => SinPitch < 0 && !Down,
            [To(PU_Full)]            = () => SinPitch >= PitchBreakpoint(st_PU_Rising) && Back && Up,
            [To(PU_Full)
                .From(UD_PURising)]  = () => SinPitch >= PitchBreakpoint(st_PU_Rising) && Forward && Up,

            [To(UpsideDown)]         = () => SinPitch >= PitchBreakpoint(st_PU_Full) && Back && !Up,
            [To(UpsideDown)
                .From(UD_PUFalling)] = () => SinPitch >= PitchBreakpoint(st_PU_Full) && !Up,
            [To(UpsideDown)
                .From(UD_Rising)]    = () => SinPitch >= PitchBreakpoint(st_PU_Full) && !Up,
            [To(UD_Falling)]         = () => SinPitch <= PitchBreakpoint(st_UD_) && Down,
            [To(UD_Rising)]          = () => SinPitch < PitchBreakpoint(st_UD_) && !Down,
            [To(UD_PURising)]        = () => SinPitch >= PitchBreakpoint(st_UD_) && Up,
            [To(UD_PUFalling)]       = () => SinPitch > PitchBreakpoint(st_UD_) && !Up,
            [To(CompleteLoop)]       = () => SinPitch <= PitchBreakpoint(st_UD_Falling) && Down && Forward,
            [To(PD_HoldBack)]        = () => SinPitch < 0 && Back,

            [To(PD_Rising)]          = () => isPassedWrapAngle(),

            [To(HoldBack)]           = () => Mathf.Approximately(SinPitch, 0f) && Back,
            [To(HoldBack)]           = () => Mathf.Approximately(SinPitch, 0f) && Back,
            [To(RollOver)]           = () => InStateForSeconds(2.0f),
            [To(TurnAround)]         = () => InStateForSeconds(0.5f) && Back,
            [To(TurnAround)
                .From(PD_HoldBack)]  = () => InStateForSeconds(0.1f) && Back,
        };

        Transitions = new Dictionary<StateType, Can>()
        {
            [Straight] = new Can {
                HoldBack,
                PU_Rising,
                PD_Falling
            },
            [PU_Rising] = new Can {
                PU_Full,
                PU_Falling,
                PD_Falling
            },
            [PU_Falling] = new Can {
                PU_Rising,
                Straight,
                PD_Falling
            },
            [PD_Falling] = new Can {
                PD_HoldBack,
                PD_Rising,
                PU_Rising,
            },
            [PD_Rising] = new Can {
                PD_Falling,
                Straight,
                PU_Rising
            },
            [PU_Full] = new Can {
                UpsideDown,
                PU_Falling
            },
            [UpsideDown] = new Can {
                RollOver,
                UD_PURising,
                UD_Falling
            },
            [UD_PURising] = new Can {
                UD_Falling,
                UD_NoBack,
                UD_PUFalling,
                PU_Full
            },
            [UD_PUFalling] = new Can {
                UD_Falling,
                UD_NoBack,
                UD_PURising
            },
            [UD_Falling] = new Can {
                UD_Rising,
                UpsideDown,
                CompleteLoop
            },
            [UD_Rising] = new Can {
                UpsideDown,
                UD_PURising,
                UD_Falling
            },
            [CompleteLoop] = new Can {
                ToAngleWrap
            },
            [RollOver] = new Can {
                /* Transitions to Straight on AnimEnd */
            },
            [HoldBack] = new Can {
                TurnAround,
                PD_HoldBack,
                Straight
            },
            [PD_HoldBack] = new Can {
                HoldBack,
                TurnAround,
                PD_Falling,
                PD_Rising,
            },
            [TurnAround] = new Can
            {
                OnAnimationEnd(Straight),
            },
            [Bonk] = new Can
            { 
                OnAnimationEnd(Fall),
            },
            [Broom] = new Can
            {
                /* Superstate Transitions */
                Bonk,
                OnInput(Broom, Fall),
                OnInput(Attack, GetAttackStateType()),
                OnInput(Jump, GetAirJump()),
            },
            [BroomStart] = new Can
            { 
                OnAnimationEnd(Straight),
            },
            [Crouch] = new Can { },
            [Dash] = new Can { },
            [DownAir] = new Can { },
            [DownTilt] = new Can { },
            [Fall] = new Can { },
            [FallingNair] = new Can { },
            [Hurt] = new Can { },
            [Jab1] = new Can { },
            [Jab2] = new Can { },
            [Jab3] = new Can { },
            [Jump] = new Can { },
            [RisingNair] = new Can { },
            [Slide] = new Can { },
            [Slip] = new Can { },
            [SpinJump] = new Can { },
            [UpAir] = new Can { },
            [UpTilt] = new Can { },
            [Wait] = new Can { },
            [Walk] = new Can { },
            [WallJump] = new Can { },
            [WallSlide] = new Can { },
        };
    }

    public override bool CheckCondition()
    {
        Try toStateType =
            Transitions.GetValueOrDefault(stateMachine.currentState.stateType)
            ?.FirstOrDefault(transition => transition.Condition());

        if (toStateType != null) {
            to = stateRegistry.GetState(toTargetState);
            return !Equals(to, state);
        }

        return false;
    }

    private Func<bool> OnAnimationEnd(StateType stateType)
    {
        return () => sprite.GetNormalizedTime() >= 1;
    }

    public bool isPassedWrapAngle()
    {
        float pitch = bh_Broom.pitchLerper.Value();
        if (pitch >= angleWrapCutoff)
        {
            bh_Broom.pitchLerper = new RateLerper(angleWrapCutoff - 360, 0, bh_Broom.pitchRate, Time.time);
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
