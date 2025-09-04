using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static StateType;

/*
    Wrapper class which holds and returns a StateType if a certain condition is
    true.

    Allows us to use the Decorator pattern for defining available transitions.

    By default the condition is just set to return true, essentially there is no
    condition.

    Additionally defines the Can helper method which takes a List of any amount
    of StateTypes and StateTypeWrappers and compiles them into a single List of
    StateTypeWrappers for the TransitionManager to evaluate.
*/
public class StateTypeWrapper
{
    private StateType stateType;
    private Func<bool> condition;

    public StateTypeWrapper(StateType _stateType, Func<bool> _condition = null)
    {
        stateType = _stateType;
        condition = _condition ?? (() => true);
    }

    //Get the StateType Wrapped by the class
    public virtual StateType Value()
    {
        return condition() ? stateType : Unset;
    }

    /*
        Returns an object which will return a StateType called a StateTypeWrapper.
        This way we can use the Decorator Pattern to apply external conditions
            - OnAnimationEnd
            - OnComplete
        StateTypeWrapper is a class which contains a Value method of type StateType
    */
    public static List<StateTypeWrapper> Can(params object[] items)
    {
        var result = new List<StateTypeWrapper>();

        foreach (var item in items)
        {
            switch (item)
            {
                case StateType single:
                    result.Add(new StateTypeWrapper(single));
                    break;
                case StateTypeWrapper wrapper:
                    result.Add(wrapper);
                    break;
                case IEnumerable<StateType> collection:
                    result.AddRange(collection.Select(s => new StateTypeWrapper(s)));
                    break;
                case IEnumerable<StateTypeWrapper> wrapperCollection:
                    result.AddRange(wrapperCollection);
                    break;
                default:
                    Debug.LogWarning($"Unsupported item type in Can(): {item?.GetType()}");
                    break;
            }
        }

        return result;
    }
}

