using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class StateMachine : MonoBehaviour
{
    [NotNull] public SpriteController sprite;
    [NotNull] public StateRegistry stateRegistry;
    [NotNull] public StateTransitionManager stateTransitions;

    [NotNull]
    public State currentState;
    public bool initialized { get; private set; } = false;

    public async Task Init()
    {
        setActiveStateObjects();
        await Task.Delay(10);
        StartState();
        initialized = true;
    }

    public void ChangeState(State newState)
    {
        //Logging

        if (!newState) return;
        if (Equals(newState.stateType, StateType.Unset)) return;
        if (currentState.Equals(newState)) return;

        ExitState(newState.stateType);

        currentState = newState;
        setActiveStateObjects();

        StartState();
    }

    private void setActiveStateObjects()
    {
        List<Transform> parents = GetStateParents(currentState.transform.parent);

        foreach (State state in stateRegistry.states)
        {
            bool isCurrentState = state.Equals(currentState);
            bool isParent = parents.Contains(state.transform);

            state.gameObject.SetActive(isCurrentState || isParent);
        }
    }

    public static List<Transform> GetStateParents(Transform go)
    {
        if (go == null)
        {
            throw new Exception(
                $"State {go.name} is not a child of a StateMachine"
            );
        }
        if (go.GetComponent<StateMachine>())
        {
            return new List<Transform>();
        }
        
        var states = new List<Transform> { go };
        states.AddRange(GetStateParents(go.parent));
        return states;
    }

    private void checkTransitions()
    {
        // TODO: We probably need some unavoidable transitions such as Hurt and Bonk
        // Though there are still times those should not be allowed either, perhaps
        // some kind of tiered system such as Pausible States and Unpausible States
        // then the ability to prevfent transitions to even unpausable states during
        // cutscenes etc
        if (currentState.isTransitionPaused()) return;
        if (stateTransitions.TryGetFirstActiveTransition(out var toStateType))
        {
            State toState = stateRegistry.GetState(toStateType);
            ChangeState(toState);
        }
    }

    #region Call Behaviors
    void StartState()
    {
        sprite.SetAnimationClip(currentState.stateAnimation);
        foreach (IStateBehavior beh in GetComponentsInChildren<IStateBehavior>())
        {
            beh.StartState();
        }
    }

    void Update()
    {
        if (!initialized) return;

        foreach (IStateBehavior beh in GetComponentsInChildren<IStateBehavior>())
        {
            beh.UpdateState();
        }
        checkTransitions();
    }

    void FixedUpdate()
    {
        if (!initialized) return;

        foreach (IStateBehavior beh in GetComponentsInChildren<IStateBehavior>())
        {
            beh.FixedUpdateState();
        }
    }

    void ExitState(StateType toState)
    {
        //Could be cool to log the toState here

        foreach (IStateBehavior beh in GetComponentsInChildren<IStateBehavior>())
        {
            beh.ExitState(toState);
        }
        currentState.OnExit();
        sprite.ClearOverrideClip();
    }
    #endregion
}
