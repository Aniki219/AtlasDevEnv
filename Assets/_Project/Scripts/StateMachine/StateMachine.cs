using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using UnityEngine;

public class StateMachine : MonoBehaviour
{
    [NotNull] public SpriteController sprite;

    [NotNull]
    public State currentState;
    [NotNull]
    public Transform anyState;

    public List<State> states;
    public List<StateBehavior> behaviors;
    public List<StateTransition> transitions;

    void Start()
    {
        states = GetComponentsInChildren<State>().ToList();
        loadStateComponents();
        StartState();
    }

    public void ChangeState(State newState)
    {
        if (currentState.Equals(newState))
        {
            return;
        }

        ExitState();

        currentState = newState;
        loadStateComponents();

        StartState();
    }

    private void loadStateComponents()
    {
        List<State> parentStates = getParentStates(currentState.transform.parent);

        foreach (State state in states)
        {
            bool isCurrentState = state.Equals(currentState);
            bool isParentState = parentStates.Contains(state);

            state.gameObject.SetActive(isCurrentState || isParentState);
        }
        behaviors = GetComponentsInChildren<StateBehavior>().ToList();
        transitions = GetComponentsInChildren<StateTransition>().ToList();
    }

    private List<State> getParentStates(Transform go)
    {
        if (go == null)
        {
            throw new Exception(
                $"State {currentState.name} is not a child of a StateMachine"
            );
        }
        if (go.GetComponent<StateMachine>())
        {
            return new List<State>();
        }
        
        var states = new List<State>
        {
            go.GetComponent<State>()
        };
        states.AddRange(getParentStates(go.parent));
        return states;
    }

    private void checkTransitions()
    {
        foreach (StateTransition transition in transitions)
        {
            if (transition.CheckCondition())
            {
                ChangeState(transition.ToState());
                return;
            }
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
