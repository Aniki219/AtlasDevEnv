using System.Threading.Tasks;
using UnityEngine;
using System.Linq;

using static StateType;
using static StateTransitionBuilder;
using System.Collections.Generic;
using System;

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
        return new StateTypeWrapper(stateType, () => state.isComplete);
    }

    protected bool InStateForSeconds(float seconds)
    {
        return stateMachine.currentState.GetElapsedTime() > seconds;
    }

    public bool TryGetFirstActiveTransition(out StateType outStateType)
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
            ?.Select<StateTypeWrapper, (StateType stateType, Func<bool> cond)>(toStateTypeWrapper =>
            {
                var toStateType = toStateTypeWrapper.Value();
                // Look first for a To().From() transition key
                if (ToConditions.TryGetValue(To(toStateType)
                                                .From(fromStateType), out var toFrom))
                {
                    return (toStateType, toFrom);
                }
                // If no special transition key just look for a To()
                if (ToConditions.TryGetValue(To(toStateType), out var to))
                {
                    return (toStateType, to);
                }
                // Return Unset if no Transitions
                return (Unset, () => false);
            })
            .Where(e => e.cond()) // Only return currently active transitions
            .Select(e => e.stateType) // Select the StateType from the tuple
            .FirstOrDefault() // Grab the first active Transition
        ;

        if (canStateType.HasValue && canStateType.Value != Unset)
        {
            outStateType = canStateType.Value;
            return !Equals(canStateType.Value, state);
        }

        return false;
    }
}

