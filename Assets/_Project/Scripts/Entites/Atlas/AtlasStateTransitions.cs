using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using Try = System.Func<StateType?>;
using Can = System.Collections.Generic.List<System.Func<StateType?>>;

public class AtlasStateTransitions : StateTransition
{
    [SerializeField] BroomBehavior beh;

    [SerializeField] float angleWrapCutoff;
    [SerializeField] float breakpointThreshold;

    private Dictionary<StateType, Can> Transitions = new Dictionary<StateType, Can>();

    private State PitchUpRaising    => stateRegistry.GetState(StateType.Broom_PitchUpRaising   );
    private State PitchUpFull       => stateRegistry.GetState(StateType.Broom_PitchUpFull      );
    private State UpsideDown        => stateRegistry.GetState(StateType.Broom_UpsideDown       );
    private State UpsideDownFalling => stateRegistry.GetState(StateType.Broom_UpsideDownFalling);

    private float SinPitch  => Mathf.Round(Mathf.Sin(beh.pitchLerper.Value() * Mathf.Deg2Rad));
    private bool Up         => input.Up(ButtonState.DOWN);
    private bool Down       => input.Down(ButtonState.DOWN);
    private bool Back       => input.Back();
    private bool Forward    => input.Forward();

    Try ToStraight                  => () => Mathf.Approximately(SinPitch, 0f) ? StateType.Broom_Straight : null;

    Try ToPitchUpRaising            => () => SinPitch >= 0 && Up    ? StateType.Broom_PitchUpRaising : null;
    Try ToPitchUpFalling            => () => SinPitch >  0 && !Up   ? StateType.Broom_PitchUpFalling : null;
    Try ToPitchDownFalling          => () => SinPitch <= 0 && Down && !Back ? StateType.Broom_PitchDownFalling : null;
    Try ToPitchDownRaising          => () => SinPitch <  0 && !Down ? StateType.Broom_PitchDownRaising : null;
    Try ToPitchUpFull               => () => SinPitch >= PitchBreakpoint(PitchUpRaising) && Back && Up ? StateType.Broom_PitchUpFull : null;
    Try ToPitchUpFullUpsideDown     => () => SinPitch >= PitchBreakpoint(PitchUpRaising) && Forward && Up ? StateType.Broom_PitchUpFull : null;

    Try ToUpsideDown                => () => SinPitch >= PitchBreakpoint(PitchUpFull) &&  Back && !Up ? StateType.Broom_UpsideDown : null;
    Try ToUpsideDownNoBack          => () => SinPitch >= PitchBreakpoint(PitchUpFull) && !Up   ? StateType.Broom_UpsideDown : null;
    Try ToUpsideDownFalling         => () => SinPitch <= PitchBreakpoint(UpsideDown)  &&  Down ? StateType.Broom_UpsideDownFalling : null;
    Try ToUpsideDownRising          => () => SinPitch <  PitchBreakpoint(UpsideDown)  && !Down ? StateType.Broom_UpsideDownRising : null;
    Try ToUpsideDownPitchUpRising   => () => SinPitch >= PitchBreakpoint(UpsideDown)  &&  Up   ? StateType.Broom_UpsideDownPitchUpRising : null;
    Try ToUpsideDownPitchUpFalling  => () => SinPitch >  PitchBreakpoint(UpsideDown)  && !Up   ? StateType.Broom_UpsideDownPitchUpFalling : null;
    Try ToRollOver                  => () => InStateForSeconds(2.0f) ? StateType.Broom_RollOver : null;

    Try ToCompleteLoop              => () => SinPitch <= PitchBreakpoint(UpsideDownFalling) && Down && Forward ? StateType.Broom_CompleteLoop : null;
    Try ToAngleWrap                 => () => isPassedWrapAngle() ? StateType.Broom_PitchDownRaising : null;

    Try ToStraightHoldBack          => () => Mathf.Approximately(SinPitch, 0f) && Back ? StateType.Broom_StraightHoldBack : null;
    Try ToPitchDownHoldBack         => () => SinPitch < 0 && Back ? StateType.Broom_PitchDownHoldBack : null;
    Try ToTurnAround                => () => InStateForSeconds(0.5f) && Back ? StateType.Broom_TurnAround : null;
    Try ToDiveTurnAround            => () => InStateForSeconds(0.1f) && Back ? StateType.Broom_TurnAround : null;

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
            [StateType.Broom_TurnAround] = new Can { },
            [StateType.Bonk] = new Can {},
            [StateType.Broom] = new Can {},
            [StateType.BroomStart] = new Can {},
            [StateType.Crouch] = new Can {},
            [StateType.Dash] = new Can {},
            [StateType.DownAir] = new Can {},
            [StateType.DownTilt] = new Can {},
            [StateType.Fall] = new Can {},
            [StateType.FallingNair] = new Can {},
            [StateType.Hurt] = new Can {},
            [StateType.Jab1] = new Can {},
            [StateType.Jab2] = new Can {},
            [StateType.Jab3] = new Can {},
            [StateType.Jump] = new Can {},
            [StateType.RisingNair] = new Can {},
            [StateType.Slide] = new Can {},
            [StateType.Slip] = new Can {},
            [StateType.SpinJump] = new Can {},
            [StateType.UpAir] = new Can {},
            [StateType.UpTilt] = new Can {},
            [StateType.Wait] = new Can {},
            [StateType.Walk] = new Can {},
            [StateType.WallJump] = new Can {},
            [StateType.WallSlide] = new Can {},
        };
    }

    public override bool CheckCondition()
    {
        StateType? toStateType = 
            Transitions.GetValueOrDefault(stateMachine.currentState.stateType)
            ?.Select(transition => transition())
            .FirstOrDefault(result => result != null);

        if (toStateType.HasValue) {
            to = stateRegistry.GetState(toStateType.Value);
            return !Equals(to, state);
        }

        return false;
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
