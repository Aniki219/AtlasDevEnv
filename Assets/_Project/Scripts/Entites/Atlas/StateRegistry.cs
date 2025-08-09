using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(StateMachine))]
public class StateRegistry : MonoBehaviour
{
    [SerializeField] private bool throwOnMissingState;

    private Dictionary<StateType, State> registry;

    private void Awake()
    {
        registry = new Dictionary<StateType, State>();
        DiscoverAndRegisterStates();
    }

    public State GetState(StateType stateType)
    {
        State state;
        registry.TryGetValue(stateType, out state);

        if (state == null)
        {
            AtlasHelpers.WarnOrThrow(
                throwOnMissingState,
                $"Failed to get state {stateType}"
            );
        }

        return state;
    }

    private void DiscoverAndRegisterStates()
    {
        var stateRegistrations = GetComponentsInChildren<State>()
        .Select((state) =>
            new KeyValuePair<StateType, bool>(
                state.stateType,
                RegisterState(state)
            )
        )
        .ToList();

        int successful = stateRegistrations.Count(r => r.Value);
        string failedStates = string.Join(", ",
            stateRegistrations
            .Where(r => !r.Value)
            .Select(r => r.Key.ToString()));

        Debug.Log($"Registered {successful} states");
        if (failedStates.Length > 0)
        {
            AtlasHelpers.WarnOrThrow(
                throwOnMissingState,
                $"Failed to register states: {failedStates}"
            );
        }
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