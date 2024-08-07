namespace Wasp;

public partial class Machine<TState, TTrigger> where TState : notnull where TTrigger : notnull
{
    private ICollection<TransitionBehavior>? GetTransitionBehaviors(TTrigger trigger)
    {
        if (!_stateConfigs.TryGetValue(_currentState, out var stateConfig)) return null;
        return stateConfig.TransitionBehaviorDict.GetValueOrDefault(trigger);
    }

    private void ExecuteActionCollection(List<Action<TriggerParams?>> actions, TriggerParams? triggerParams)
    {
        foreach (var action in actions)
        {
            action(triggerParams);
        }
    }
}
