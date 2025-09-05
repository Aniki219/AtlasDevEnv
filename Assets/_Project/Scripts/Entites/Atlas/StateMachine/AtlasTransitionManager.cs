using UnityEngine;

using static StateType;
using bt = ButtonType;
using System.Threading.Tasks;

/*
    This class is a Component on Atlas' StateMachine GameObject and is Initialized by
    the BootSequencer.
    This is a partial class which holds the Dictionaries defining the available Transitions
    and Transition Conditions for each of Atlas' StateTypes. The two other partial classes
    are:
        - AtlasTransitionManager.CanTransitions
        - AtlasTransitionManager.ToConditions
    This partial class defines useful helper methods and state info.

    This class inherits a few useful helper methods from the StateTransition class which is
    the base version of this class
*/
public partial class AtlasTransitionManager : StateTransitionManager
{
    [SerializeField] float angleWrapCutoff;
    [SerializeField] float breakpointThreshold;

    private State st_Broom => stateRegistry.GetState(su_Broom);
    private State st_PU_Rising => stateRegistry.GetState(PU_Rising);
    private State st_PU_Full => stateRegistry.GetState(PU_Full);
    private State st_UpsideDown => stateRegistry.GetState(UpsideDown);
    private State st_UD_Falling => stateRegistry.GetState(UD_Falling);

    public BroomBehavior _bh_Broom;
    private BroomBehavior bh_Broom => _bh_Broom ??= st_Broom.GetComponentInChildren<BroomBehavior>();

    private float SinPitch => Mathf.Sin(bh_Broom.pitchLerper.Value() * Mathf.Deg2Rad);
    private bool Back => InputManager.Instance.Back();
    private bool Forward => InputManager.Instance.Forward();
    private bool Up => InputManager.Instance.Y > 0;
    private bool Down => InputManager.Instance.Y < 0;

    public override async Task Init()
    {
        await base.Init();
        InitializeToConditions();
        InitializeCanTransitions();
    }

    private void Update() {
        Debug.Log($"sp: {Mathf.Sin(bh_Broom.pitchLerper.Value() * Mathf.Deg2Rad)} down: {Down} not back {!Back}");
    }

    // Helpers for use with ToConditions
    private bool PushAgainstWall()
    {
        return ColRight() && input.X > 0 || ColLeft() && input.X < 0;
    }

    private bool ColRight()
    {
        return body.collisions.getRight();
    }

    private bool ColLeft()
    {
        return body.collisions.getLeft();
    }

    // This looks like it should be in BroomBehavior somewhere
    private bool isPassedWrapAngle()
    {
        float pitch = bh_Broom.pitchLerper.Value();
        if (pitch >= angleWrapCutoff)
        {
            bh_Broom.pitchLerper = new RateLerper(angleWrapCutoff - 360, 0, bh_Broom.pitchRate, Time.time);
            return true;
        }
        return false;
    }

    private float PitchBreakpoint(State state)
    {
        // Throw if not broom super state
        BroomTargetPitchBehavior comp = state.GetComponentInChildren<BroomTargetPitchBehavior>();
        if (!comp) return Mathf.Infinity;
        return Mathf.Sin(comp.targetPitch * Mathf.Deg2Rad) * breakpointThreshold;
    }
}

