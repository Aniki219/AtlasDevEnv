using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using Can = System.Collections.Generic.List<Try>;

public class AtlasStateTransitions : StateTransition
{
    [SerializeField] float angleWrapCutoff;
    [SerializeField] float breakpointThreshold;

    private Dictionary<StateType, Can> Transitions = new Dictionary<StateType, Can>();

    private State st_Broom => stateRegistry.GetState(StateType.Broom);
    private State st_PitchUpRaising => stateRegistry.GetState(StateType.Broom_PitchUpRaising);
    private State st_PitchUpFull => stateRegistry.GetState(StateType.Broom_PitchUpFull);
    private State st_UpsideDown => stateRegistry.GetState(StateType.Broom_UpsideDown);
    private State st_UpsideDownFalling => stateRegistry.GetState(StateType.Broom_UpsideDownFalling);
    
    private BroomBehavior _bh_Broom;
    private BroomBehavior bh_Broom => _bh_Broom ??= st_Broom.GetComponent<BroomBehavior>();

    private float SinPitch => Mathf.Round(Mathf.Sin(bh_Broom.pitchLerper.Value() * Mathf.Deg2Rad));
    private bool Up => input.Up(ButtonState.DOWN);
    private bool Down => input.Down(ButtonState.DOWN);
    private bool Back => input.Back();
    private bool Forward => input.Forward();

    Try ToStraight          => StateType.Broom_Straight.When(() => Mathf.Approximately(SinPitch, 0f));

    Try ToPitchUpRaising    => StateType.Broom_PitchUpRaising.When(() => SinPitch >= 0 && Up);
    Try ToPitchUpFalling    => StateType.Broom_PitchUpFalling.When(() => SinPitch > 0 && !Up);
    Try ToPitchDownFalling  => StateType.Broom_PitchDownFalling.When(() => SinPitch <= 0 && Down && !Back);
    Try ToPitchDownRaising  => StateType.Broom_PitchDownRaising.When(() => SinPitch < 0 && !Down);
    Try ToPitchUpFull       => StateType.Broom_PitchUpFull.When(() => SinPitch >= PitchBreakpoint(st_PitchUpRaising) && Back && Up);
    Try ToPitchUpFullUpsideDown => StateType.Broom_PitchUpFull.When(() => SinPitch >= PitchBreakpoint(st_PitchUpRaising) && Forward && Up);

    Try ToUpsideDown        => StateType.Broom_UpsideDown.When(() => SinPitch >= PitchBreakpoint(st_PitchUpFull) && Back && !Up);
    Try ToUpsideDownNoBack  => StateType.Broom_UpsideDown.When(() => SinPitch >= PitchBreakpoint(st_PitchUpFull) && !Up);
    Try ToUpsideDownFalling => StateType.Broom_UpsideDownFalling.When(() => SinPitch <= PitchBreakpoint(st_UpsideDown) && Down);
    Try ToUpsideDownRising  => StateType.Broom_UpsideDownRising.When(() => SinPitch < PitchBreakpoint(st_UpsideDown) && !Down);
    Try ToUpsideDownPitchUpRising  => StateType.Broom_UpsideDownPitchUpRising.When(() => SinPitch >= PitchBreakpoint(st_UpsideDown) && Up);
    Try ToUpsideDownPitchUpFalling => StateType.Broom_UpsideDownPitchUpFalling.When(() => SinPitch > PitchBreakpoint(st_UpsideDown) && !Up);
    Try ToRollOver          => StateType.Broom_RollOver.When(() => InStateForSeconds(2.0f));

    Try ToCompleteLoop      => StateType.Broom_CompleteLoop.When(() => SinPitch <= PitchBreakpoint(st_UpsideDownFalling) && Down && Forward);
    Try ToAngleWrap         => StateType.Broom_PitchDownRaising.When(() => isPassedWrapAngle());

    Try ToStraightHoldBack  => StateType.Broom_StraightHoldBack.When(() => Mathf.Approximately(SinPitch, 0f) && Back);
    Try ToPitchDownHoldBack => StateType.Broom_PitchDownHoldBack.When(() => SinPitch < 0 && Back);
    Try ToTurnAround        => StateType.Broom_TurnAround.When(() => InStateForSeconds(0.5f) && Back);
    Try ToDiveTurnAround    => StateType.Broom_TurnAround.When(() => InStateForSeconds(0.1f) && Back);

    public void Start()
    {
        Transitions = new Dictionary<StateType, Can>()
        {
            [StateType.Broom_Straight] = new Can {
                ToStraightHoldBack,
                ToPitchUpRaising,
                ToPitchDownFalling
            },
            [StateType.Broom_PitchUpRaising] = new Can {
                ToPitchUpFull,
                ToPitchUpFalling,
                ToPitchDownFalling
            },
            [StateType.Broom_PitchUpFalling] = new Can {
                ToPitchUpRaising,
                ToStraight,
                ToPitchDownFalling
            },
            [StateType.Broom_PitchDownFalling] = new Can {
                ToPitchDownHoldBack,
                ToPitchDownRaising,
                ToPitchUpRaising,
            },
            [StateType.Broom_PitchDownRaising] = new Can {
                ToPitchDownFalling,
                ToStraight,
                ToPitchUpRaising
            },
            [StateType.Broom_PitchUpFull] = new Can {
                ToUpsideDown,
                ToPitchUpFalling
            },
            [StateType.Broom_UpsideDown] = new Can {
                ToRollOver,
                ToUpsideDownPitchUpRising,
                ToUpsideDownFalling
            },
            [StateType.Broom_UpsideDownPitchUpRising] = new Can {
                ToUpsideDownFalling,
                ToUpsideDownNoBack,
                ToUpsideDownPitchUpFalling,
                ToPitchUpFullUpsideDown
            },
            [StateType.Broom_UpsideDownPitchUpFalling] = new Can {
                ToUpsideDownFalling,
                ToUpsideDownNoBack,
                ToUpsideDownPitchUpRising
            },
            [StateType.Broom_UpsideDownFalling] = new Can {
                ToUpsideDownRising,
                ToUpsideDown,
                ToCompleteLoop
            },
            [StateType.Broom_UpsideDownRising] = new Can {
                ToUpsideDown,
                ToUpsideDownPitchUpRising,
                ToUpsideDownFalling
            },
            [StateType.Broom_CompleteLoop] = new Can {
                ToAngleWrap
            },
            [StateType.Broom_RollOver] = new Can {
                /* Transitions to Straight on AnimEnd */
            },
            [StateType.Broom_StraightHoldBack] = new Can {
                ToTurnAround,
                ToStraightHoldBack,
                ToStraight
            },
            [StateType.Broom_PitchDownHoldBack] = new Can {
                ToStraightHoldBack,
                ToDiveTurnAround,
                ToPitchDownFalling,
                ToPitchDownRaising,
            },
            [StateType.Broom_TurnAround] = new Can
            {
                OnAnimationEnd(StateType.Broom_Straight),
                PauseTransition(StateType.Bonk, PauseType.StateEnd),
            },
            [StateType.Bonk] = new Can
            { 
                OnAnimationEnd(StateType.Fall),
            },
            [StateType.Broom] = new Can
            {
                /* Superstate Transitions */
                ToBonk,
                OnInput(Broom, StateType.Fall),
                OnInput(Attack, GetAttackStateType()),
                OnInput(Jump, GetAirJump()),
            },
            [StateType.BroomStart] = new Can
            { 
                OnAnimationEnd(StateType.Broom_Straight),
            },
            [StateType.Crouch] = new Can { },
            [StateType.Dash] = new Can { },
            [StateType.DownAir] = new Can { },
            [StateType.DownTilt] = new Can { },
            [StateType.Fall] = new Can { },
            [StateType.FallingNair] = new Can { },
            [StateType.Hurt] = new Can { },
            [StateType.Jab1] = new Can { },
            [StateType.Jab2] = new Can { },
            [StateType.Jab3] = new Can { },
            [StateType.Jump] = new Can { },
            [StateType.RisingNair] = new Can { },
            [StateType.Slide] = new Can { },
            [StateType.Slip] = new Can { },
            [StateType.SpinJump] = new Can { },
            [StateType.UpAir] = new Can { },
            [StateType.UpTilt] = new Can { },
            [StateType.Wait] = new Can { },
            [StateType.Walk] = new Can { },
            [StateType.WallJump] = new Can { },
            [StateType.WallSlide] = new Can { },
        };
    }

    public override bool CheckCondition()
    {
        Try toStateType =
            Transitions.GetValueOrDefault(stateMachine.currentState.stateType)
            ?.FirstOrDefault(transition => transition.Condition());

        if (toStateType != null) {
            to = stateRegistry.GetState(toStateType.TargetState);
            return !Equals(to, state);
        }

        return false;
    }

    private Try OnAnimationEnd(StateType stateType)
    {
        return stateType.When(() => sprite.GetNormalizedTime() >= 1);
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
