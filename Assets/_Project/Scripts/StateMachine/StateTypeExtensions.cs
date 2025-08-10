using System;

public static class StateTypeExtensions
{
    public static Try When(this StateType stateType, Func<bool> condition)
    => new Try(stateType, condition);
}

public class Try
{
    public StateType TargetState { get; }
    public Func<bool> Condition { get; }
    
    public Try(StateType target, Func<bool> condition)
    {
        TargetState = target;
        Condition = condition;
    }
}