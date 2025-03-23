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
    public List<IStateBehavior> behaviors;
    public List<IStateTransition> transitions;

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
        foreach (State state in states)
        {
            state.gameObject.SetActive(state.Equals(currentState));
        }
        behaviors = GetComponentsInChildren<IStateBehavior>().ToList();
        transitions = GetComponentsInChildren<IStateTransition>().ToList();
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
