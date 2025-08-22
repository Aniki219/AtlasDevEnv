using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using Can = System.Collections.Generic.List<StateType?>;

using static StateType;
using bt = ButtonType;

public class AtlasStateTransitions : StateTransition
{
    [SerializeField] float angleWrapCutoff;
    [SerializeField] float breakpointThreshold;

    public Dictionary<StateType, Can> CanTransitions { get; private set; }
    public Dictionary<(StateType to, StateType? from), Func<bool>> ToConditions { get; private set; }

    private State st_Broom => stateRegistry.GetState(Straight);
    private State st_PU_Rising => stateRegistry.GetState(PU_Rising);
    private State st_PU_Full => stateRegistry.GetState(PU_Full);
    private State st_UpsideDown => stateRegistry.GetState(UpsideDown);
    private State st_UD_Falling => stateRegistry.GetState(UD_Falling);
    
    private BroomBehavior _bh_Broom;
    private BroomBehavior bh_Broom => _bh_Broom ??= st_Broom.GetComponent<BroomBehavior>();

    private float SinPitch => Mathf.Round(Mathf.Sin(bh_Broom.pitchLerper.Value() * Mathf.Deg2Rad));
    private bool Back      => InputManager.Instance.Back();
    private bool Forward   => InputManager.Instance.Forward();
    private bool Up        => InputManager.Instance.Y > 0;
    private bool Down      => InputManager.Instance.X < 0;

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

    private InputQuery On(ButtonType buttonType) {
        return input.Query(buttonType);
    }

    public void Start()
    {
        ToConditions = new Dictionary<(StateType to, StateType? from), Func<bool>> {
            [To(Straight)]           = () => Mathf.Approximately(SinPitch, 0f),

            [To(PU_Rising)]          = () => SinPitch >= 0 &&  Up,
            [To(PU_Falling)]         = () => SinPitch >  0 && !Up,
            [To(PD_Falling)]         = () => SinPitch <= 0 &&  Down && !Back,
            [To(PD_Rising)]          = () => SinPitch <  0 && !Down,
            [To(PU_Full)]            = () => SinPitch >= PitchBreakpoint(st_PU_Rising)  &&  Back && Up,
            [To(PU_Full)
                .From(UD_PURising)]  = () => SinPitch >= PitchBreakpoint(st_PU_Rising)  &&  Forward && Up,

            [To(UpsideDown)]         = () => SinPitch >= PitchBreakpoint(st_PU_Full)    &&  Back && !Up,
            [To(UpsideDown)
                .From(UD_PUFalling)] = () => SinPitch >= PitchBreakpoint(st_PU_Full)    && !Up,
            [To(UpsideDown)
                .From(UD_Rising)]    = () => SinPitch >= PitchBreakpoint(st_PU_Full)    && !Up,
            [To(UD_Falling)]         = () => SinPitch <= PitchBreakpoint(st_UpsideDown) &&  Down,
            [To(UD_Rising)]          = () => SinPitch <  PitchBreakpoint(st_UpsideDown) && !Down,
            [To(UD_PURising)]        = () => SinPitch >= PitchBreakpoint(st_UpsideDown) &&  Up,
            [To(UD_PUFalling)]       = () => SinPitch >  PitchBreakpoint(st_UpsideDown) && !Up,
            [To(CompleteLoop)]       = () => SinPitch <= PitchBreakpoint(st_UD_Falling) &&  Down && Forward,
            [To(PD_HoldBack)]        = () => SinPitch < 0 && Back,

            [To(PD_Rising)
                .From(CompleteLoop)] = () => isPassedWrapAngle(),

            [To(HoldBack)]           = () => Mathf.Approximately(SinPitch, 0f) && Back,
            [To(HoldBack)]           = () => Mathf.Approximately(SinPitch, 0f) && Back,
            [To(RollOver)]           = () => InStateForSeconds(2.0f),
            [To(TurnAround)]         = () => InStateForSeconds(0.5f) && Back,
            [To(TurnAround)
                .From(PD_HoldBack)]  = () => InStateForSeconds(0.1f) && Back,
            [To(Walk)
                .From(Fall)]         = () => body.IsGrounded() && body.velocity.y <= 0,

            [To(Fall)]               = () => !body.IsGrounded(),
            [To(Fall)
                .From(su_Broom)]     = () => bt.Broom.Pressed(),

            [To(Jab1)]               = () => bt.Attack.Pressed(),
            [To(Jab2)]               = () => bt.Attack.Pressed(),
            [To(Jab3)]               = () => bt.Attack.Pressed(),
            [To(UpTilt)]             = () => bt.Attack.UpTilt()  .Pressed(),
            [To(DownTilt)]           = () => bt.Attack.DownTilt().Pressed(),
            [To(UpAir)]              = () => bt.Attack.UpTilt()  .Pressed() && !body.IsGrounded(),
            [To(DownAir)]            = () => bt.Attack.DownTilt().Pressed() && !body.IsGrounded(),
            [To(FallingNair)]        = () => bt.Attack.Pressed()            && !body.IsGrounded(),
            [To(RisingNair)]         = () => bt.Attack.Pressed()            && !body.IsGrounded(),
        };

        CanTransitions = new Dictionary<StateType, Can>() {
            [Straight] = new Can {
                HoldBack,
                PU_Rising,
                PD_Falling
            },
            [PU_Rising] = new Can {
                PU_Full,
                PU_Falling,
                PD_Falling,
            },
            [PU_Falling] = new Can {
                PU_Rising,
                Straight,
                PD_Falling,
            },
            [PD_Falling] = new Can {
                PD_HoldBack,
                PD_Rising,
                PU_Rising,
            },
            [PD_Rising] = new Can {
                PD_Falling,
                Straight,
                PU_Rising,
            },
            [PU_Full] = new Can {
                UpsideDown,
                PU_Falling,
            },
            [UpsideDown] = new Can {
                RollOver,
                UD_PURising,
                UD_Falling,
            },
            [UD_PURising] = new Can {
                UD_Falling,
                UD_PUFalling,
                PU_Full,
            },
            [UD_PUFalling] = new Can {
                UD_Falling,
                UpsideDown,
                UD_PURising,
            },
            [UD_Falling] = new Can {
                UD_Rising,
                UpsideDown,
                CompleteLoop,
            },
            [UD_Rising] = new Can {
                UpsideDown,
                UD_PURising,
                UD_Falling,
            },
            [CompleteLoop] = new Can {
                PD_Rising,
            },
            [RollOver] = new Can {
                /* Transitions to Straight on AnimEnd */
            },
            [HoldBack] = new Can {
                TurnAround,
                PD_HoldBack,
                Straight,
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
            [su_Broom] = new Can
            {
                /* Superstate Transitions */
                Bonk,
                OnInput(bt.Broom.Pressed(), Fall),
                // OnInput(bt.Attack.Pressed(), GetAttackStateType()),
                // OnInput(bt.Jump.Pressed(), GetAirJump()),
            },
            [BroomStart] = new Can
            { 
                OnAnimationEnd(Straight),
            },
            [Crouch] = new Can { },
            [Dash] = new Can { },
            [DownAir] = new Can { },
            [DownTilt] = new Can { 
                OnAnimationEnd(Crouch),
            },
            [Fall] = new Can {
                Walk,
            },
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

    public override bool TryGetFirstActiveTransition(out StateType outStateType)
    {
        outStateType = Unset;
        StateType fromStateType = stateMachine.currentState.stateType;
        
        /*
            Our current state is the fromState. Get the Can from the fromState to recieve a
            List of toStates.
            For each toState map this entry to
                1. to(toState).from(fromState)
                2. to(toState)
                3. null
            Then get the first non-null result as a StateType?
        */
        StateType? canStateType = CanTransitions
            .GetValueOrDefault(fromStateType) // Can: List<StateType?>
            ?.Select<StateType?, (StateType stateType, Func<bool> cond)>(toStateType => {
                if (toStateType.HasValue) {
                    // Look first for a To().From() transition key
                    if (ToConditions.TryGetValue(To(toStateType.Value)
                                                    .From(fromStateType), out var toFrom)) {
                        return (toStateType.Value, toFrom);
                    }
                    // If no special transition key just look for a To()
                    if (ToConditions.TryGetValue(To(toStateType.Value), out var to)) {
                        return (toStateType.Value, to);
                    }
                }
                // Return Unset if no Transitions
                return (Unset, () => false);
            })
            .Where(e => e.cond()) // Only return currently active transitions
            .Select(e => e.stateType) // Select the StateType from the tuple
            .FirstOrDefault() // Grab the first active Transition
        ;

        if (canStateType.HasValue) {
            outStateType = canStateType.Value;
            return !Equals(to, state);
        }

        return false;
    }

    private StateType? OnAnimationEnd(StateType stateType)
    {
        return sprite.GetNormalizedTime() >= 1 ?
            stateType :
            null;
    }

    private StateType? OnInput(InputQuery inputQuery, StateType stateType)
    {
        return inputQuery ?
            stateType :
            null;
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
