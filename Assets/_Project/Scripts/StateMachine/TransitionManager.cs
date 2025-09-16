using System.Threading.Tasks;
using UnityEngine;
using System.Linq;

using static StateType;
using static StateTransitionBuilder;
using System.Collections.Generic;
using System;
using System.Reflection;

public abstract class StateTransitionManager : MonoBehaviour
{
    [SerializeField] protected State to;

    protected EntityContext ctx;
    protected EntityController entity;
    protected PlayerController pc;
    protected EntityBody body;
    protected SpriteController sprite;
    protected InputManager input;
    protected StateMachine stateMachine;
    protected State state => stateMachine?.currentState;
    protected StateRegistry stateRegistry => stateMachine?.stateRegistry;

    public Dictionary<StateType, List<StateTypeWrapper>> CanTransitions { get; protected set; }
    public Dictionary<(StateType to, StateType? from), Func<bool>> ToConditions { get; protected set; }

    public virtual Task Init()
    {
        ctx = GetComponentInParent<EntityContext>();
        entity = ctx.controller;
        pc = entity as PlayerController;
        body = ctx.body;
        sprite = ctx.sprite;
        input = ctx.input;
        stateMachine = ctx.stateMachine;

        return Task.CompletedTask;
    }

    protected StateTypeWrapper OnAnimationEnd(StateType stateType)
    {
        return new StateTypeWrapper(stateType, () => sprite.GetNormalizedTime() >= 1f);
    }

    /*
        isComplete is a State property which must be set manually by a behavior.
        Exiting the State will reset isComplete to false.
    */
    protected StateTypeWrapper OnComplete(StateType stateType)
    {
        return new StateTypeWrapper(stateType, () => PlayerController.Instance.GetComponentInChildren<StateMachine>().currentState.isComplete);
    }

    protected bool InStateForSeconds(float seconds)
    {
        return stateMachine.currentState.GetElapsedTime() > seconds;
    }

    /*
        Our current state is the fromState. 
        We get the Cans from the fromState and its superstates. 
        These are used as our toState values for trying the get a ToCondition
        For each toState map this entry to
            1. to(toState).from(fromState)
            2. to(toState).from(fromSuper)
            3. to(toState)
            4. Unset
        return true or false to indicate success
        output the StateType or Unset if none found
    */
    public bool TryGetFirstActiveTransition(out StateType outStateType)
    {
        outStateType = Unset;
        StateType baseState = stateMachine.currentState.stateType;
        List<StateType> superStates = stateRegistry.GetSuperStates(baseState);

        // All the Cans of the base state
        List<StateTypeWrapper> baseTransitions = CanTransitions
            .GetValueOrDefault(baseState) ?? new List<StateTypeWrapper>();

        // All the Cans of the superstate
        List<StateTypeWrapper> superTransitions = superStates
            .SelectMany(
                superState =>
                {
                    var output = CanTransitions
                        .GetValueOrDefault(superState) ?? new List<StateTypeWrapper>();
                    return output;
                }
            )
            .ToList();

        // All Cans of base and super states
        var availableTransitions = baseTransitions.Concat(superTransitions);

        StateType firstActiveTransition = availableTransitions
            .Select<StateTypeWrapper, (StateType stateType, Func<bool> cond)>(baseStateTypeWrapper =>
            {
                var can = baseStateTypeWrapper.Value();

                // 1. Look first for a To().From(baseState) transition key
                if (ToConditions.TryGetValue(To(can)
                                                .From(baseState), out var toFrom))
                {
                    Debug.Log("1");
                    return (can, toFrom);
                }

                // 2. Then look for a To().From(superState) transition key
                foreach (StateType superState in superStates)
                {
                    if (ToConditions.TryGetValue(To(can)
                                                    .From(superState), out var toSuperFrom))
                    {
                        Debug.Log($"2 - {superState}");
                        return (can, toSuperFrom);
                    }
                }

                // 3. If no special-case transition key just look for a To()
                if (ToConditions.TryGetValue(To(can), out var to))
                {
                    Debug.Log($"3 to: {can} - cond: {to()} state: {stateMachine.stateTransitions.state.stateType} sm: {stateMachine.stateTransitions.state.isComplete}");
                    return (can, to);
                }
                Debug.Log("4");
                // 4. Return Unset if no Transitions
                return (Unset, () => false);
            })
            .Where(e => e.cond()) // Only return currently active transitions
            .Select(e => e.stateType) // Select the StateType from the tuple
            .FirstOrDefault() // Grab the first active Transition
        ;

        if (firstActiveTransition != Unset)
        {
            outStateType = firstActiveTransition;
            Debug.Log("5");
            return !Equals(firstActiveTransition, state?.stateType);
        }
Debug.Log("6");
        return false;
    }
}

