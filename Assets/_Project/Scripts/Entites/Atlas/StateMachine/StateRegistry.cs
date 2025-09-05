using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

/*

*/

[RequireComponent(typeof(StateMachine))]
public class StateRegistry : MonoBehaviour
{
    [SerializeField] private bool throwOnMissingState;

    private Dictionary<StateType, State> registry;
    private Dictionary<StateType, List<StateType>> superStatesByStateType = new Dictionary<StateType, List<StateType>>();

    [SerializeField] public List<State> states => registry.Values.ToList();
    
    public async Task Init()
    {
        registry = new Dictionary<StateType, State>();
        await DiscoverAndRegisterStates();
        await Task.Delay(10);
    }

    public State GetState(StateType stateType)
    {
        State state;
        if (Equals(stateType, StateType.Unset)) return null;

        registry.TryGetValue(stateType, out state);

        if (!state)
        {
            AtlasHelpers.WarnOrThrow(
                throwOnMissingState,
                $"Failed to get state {stateType}"
            );
        }

        return state;
    }

    public List<StateType> GetSuperStates(StateType baseState)
    {
        if (Equals(baseState, StateType.Unset)) return new List<StateType>();

        superStatesByStateType.TryGetValue(baseState, out var result);
        return result ?? new List<StateType>();
    }

    private Task DiscoverAndRegisterStates()
    {
        var stateRegistrations = GetComponentsInChildren<State>()
        .Select<State, (string stateName, bool success)>((state) =>
            (
                state.name,
                RegisterState(state)
            )
        )
        .ToList();

        /*
            For each of these States, we create a reverse lookup table to
            record any Superstates.

            During Transfer Condition checks, we can check for transitions
            made available to any Superstates found under the given StateType

            This is a registry of StateTypes to a List of its SuperstateTypes
        */
        foreach (var (baseStateType, state) in registry)
        {
            StateMachine.GetStateParents(state.transform)
                .Select(p => p.GetComponent<State>())
                .Where(s => s != null)
                .ToList() // List of all Superstates
                .ForEach(superState => {
                    // If the superStateRegistry already has an entry for the current
                    // base StateType, then add this superstate to the baseState row
                    if (superStatesByStateType.TryGetValue(baseStateType, out var superStateTypes))
                    {
                        superStateTypes.Add(superState.stateType);
                    }
                    else
                    // Otherwise create a new List of superStates for the baseState
                    {
                        superStatesByStateType.Add(
                            baseStateType,
                            new List<StateType>() { superState.stateType }
                        );
                    }
                });
        }

        int successful = stateRegistrations.Count(r => r.success);
        string failedStates = string.Join(", ",
            stateRegistrations
            .Where(r => !r.success)
            .Select(r => r.stateName));

        Debug.Log($"Registered {successful} states");
        if (failedStates.Length > 0)
        {
            AtlasHelpers.WarnOrThrow(
                throwOnMissingState,
                $"Failed to register states: {failedStates}"
            );
        }
        return Task.CompletedTask;
    }

    public bool RegisterState(State state)
    {
        if (!ValidateState(state)) return false;

        if (registry.ContainsKey(state.stateType))
        {
            throw new Exception(
                $"Registry already contains a state: {state.name}!"
            );
        }

        registry.Add(state.stateType, state);
        return true;
    }

    public bool ValidateState(State state)
    {
        if (!state) return false;

        if (state.stateType == StateType.Unset)
        {
            AtlasHelpers.WarnOrThrow(
                throwOnMissingState,
                $"No stateType set for {state.name}"
            );
            return false;
        }

        return true;
    }
}