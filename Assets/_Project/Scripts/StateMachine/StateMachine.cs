using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using UnityEngine;

public class StateMachine : MonoBehaviour
{
    [NotNull] public readonly SpriteController sprite;
    [NotNull] public readonly StateRegistry stateRegistry;
    [NotNull] public readonly IStateTransition stateTransitions;

    [NotNull]
    public State currentState;

    void Start()
    {
        loadStateComponents();
        StartState();
    }

    public void ChangeState(State newState)
    {
        if (!newState) return;
        if (currentState.Equals(newState)) return;

        ExitState();

        currentState = newState;
        loadStateComponents();

        StartState();
    }

    private void loadStateComponents()
    {
        List<Transform> parents = getParents(currentState.transform.parent);

        foreach (State state in stateRegistry.states)
        {
            bool isCurrentState = state.Equals(currentState);
            bool isParent = parents.Contains(state.transform);

            state.gameObject.SetActive(isCurrentState || isParent);
        }
    }

    private List<Transform> getParents(Transform go)
    {
        if (go == null)
        {
            throw new Exception(
                $"State {currentState.name} is not a child of a StateMachine"
            );
        }
        if (go.GetComponent<StateMachine>())
        {
            return new List<Transform>();
        }
        
        var states = new List<Transform> { go };
        states.AddRange(getParents(go.parent));
        return states;
    }

    private void checkTransitions()
    {
        if (stateTransitions.TryGetFirstActiveTransition(out var toStateType)) {
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
        foreach (IStateBehavior beh in GetComponentsInChildren<IStateBehavior>())
        {
            beh.UpdateState();
        }
        checkTransitions();
    }

    void FixedUpdate()
    {
        foreach (IStateBehavior beh in GetComponentsInChildren<IStateBehavior>())
        {
            beh.FixedUpdateState();
        }
    }

    void ExitState()
    {
        foreach (IStateBehavior beh in GetComponentsInChildren<IStateBehavior>())
        {
            beh.ExitState();
        }
        sprite.ClearOverrideClip();
    }
    #endregion
}
