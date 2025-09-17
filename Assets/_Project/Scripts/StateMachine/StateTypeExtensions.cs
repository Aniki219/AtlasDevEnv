public static class StateTypeExtensions
{
    public static bool isA(this StateType self, StateRegistry registry, StateType stateType)
    {
        return registry.GetSuperStates(self).Contains(stateType);
    }
}