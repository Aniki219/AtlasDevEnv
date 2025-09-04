/*
    This struct allows us to define Keys for the TransitionManager.ToConditions class.

    It creates a To(StateType).From(StateType) pattern where To defines the StateType
    attempting to be transitioned to and From is an optional specification that requires
    the current state to be of a given StateType 
*/
public readonly struct StateTransitionBuilder
{
    private readonly StateType _to;

    public StateTransitionBuilder(StateType to)
    {
        _to = to;
    }

    public (StateType, StateType?) From(StateType from)
    {
        return (_to, from);
    }

    // Implicit conversion to tuple when no From() is called
    public static implicit operator (StateType, StateType?)(StateTransitionBuilder builder)
    {
        return (builder._to, null);
    }

    public static StateTransitionBuilder To(StateType to)
    {
        return new StateTransitionBuilder(to);
    }
}
