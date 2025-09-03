using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// using Can = System.Collections.Generic.List<StateType?>;

using static StateType;
using bt = ButtonType;
using System.Threading.Tasks;

public class AtlasStateTransitions : StateTransition, IStateTransition
{
    [SerializeField] float angleWrapCutoff;
    [SerializeField] float breakpointThreshold;

    public Dictionary<StateType, List<StateType>> CanTransitions;
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

    public override async Task Init()
    {
        await base.Init();

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

            [To(Jump)]               = () => bt.Jump.Pressed(),

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

        CanTransitions = new Dictionary<StateType, List<StateType>>() {
            [Straight] = Can(
                HoldBack,
                PU_Rising,
                PD_Falling
            ),
            [PU_Rising] = Can(
                PU_Full,
                PU_Falling,
                PD_Falling
            ),
            [PU_Falling] = Can(
                PU_Rising,
                Straight,
                PD_Falling
            ),
            [PD_Falling] = Can(
                PD_HoldBack,
                PD_Rising,
                PU_Rising
            ),
            [PD_Rising] = Can(
                PD_Falling,
                Straight,
                PU_Rising
            ),
            [PU_Full] = Can(
                UpsideDown,
                PU_Falling
            ),
            [UpsideDown] = Can(
                RollOver,
                UD_PURising,
                UD_Falling
            ),
            [UD_PURising] = Can(
                UD_Falling,
                UD_PUFalling,
                PU_Full
            ),
            [UD_PUFalling] = Can(
                UD_Falling,
                UpsideDown,
                UD_PURising
            ),
            [UD_Falling] = Can(
                UD_Rising,
                UpsideDown,
                CompleteLoop
            ),
            [UD_Rising] = Can(
                UpsideDown,
                UD_PURising,
                UD_Falling
            ),
            [CompleteLoop] = Can(
                PD_Rising
            ),
            [RollOver] = Can(
                /* Transitions to Straight on AnimEnd */
            ),
            [HoldBack] = Can(
                TurnAround,
                PD_HoldBack,
                Straight
            ),
            [PD_HoldBack] = Can(
                HoldBack,
                TurnAround,
                PD_Falling,
                PD_Rising
            ),
            [TurnAround] = Can(
                OnAnimationEnd(Straight)
            ),
            [Bonk] = Can( 
                OnAnimationEnd(Fall)
            ),
            [su_Broom] = Can(
                /* Superstate Transitions */
                Bonk,
                Fall,
                SuperStateTypes.ArialAttacks
            ),
            [BroomStart] = Can( 
                OnAnimationEnd(Straight)
            ),
            [Crouch] = Can( ),
            [Dash] = Can( ),
            [DownAir] = Can( ),
            [DownTilt] = Can( 
                OnAnimationEnd(Crouch)
            ),
            [Fall] = Can(
                Walk
            ),
            [FallingNair] = Can( ),
            [Hurt] = Can( ),
            [Jab1] = Can( ),
            [Jab2] = Can( ),
            [Jab3] = Can( ),
            [Jump] = Can( ),
            [RisingNair] = Can( ),
            [Slide] = Can( ),
            [Slip] = Can( ),
            [SpinJump] = Can( ),
            [UpAir] = Can( ),
            [UpTilt] = Can( ),
            [Wait] = Can( ),
            [Walk] = Can( 
                Jump,
                Fall,
                Crouch,
                Jab1,
                UpTilt
            ),
            [WallJump] = Can( ),
            [WallSlide] = Can( ),
        };
    }

    private List<StateType> Can(params object[] items) {
        var result = new List<StateType>();
        
        foreach (var item in items) {
            // If it's a single StateType, add it directly
            if (item is StateType single) {
                result.Add(single);
            }
            // If it's a collection of StateTypes, add all of them
            else if (item is IEnumerable<StateType> collection) {
                result.AddRange(collection);
            }
        }

        return result;
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
            ?.Select<StateType, (StateType stateType, Func<bool> cond)>(toStateType => {
                // Look first for a To().From() transition key
                if (ToConditions.TryGetValue(To(toStateType)
                                                .From(fromStateType), out var toFrom)) {
                    return (toStateType, toFrom);
                }
                // If no special transition key just look for a To()
                if (ToConditions.TryGetValue(To(toStateType), out var to)) {
                    return (toStateType, to);
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
