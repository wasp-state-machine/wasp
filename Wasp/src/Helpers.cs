namespace Wasp;

public partial class Machine<TState, TTrigger> where TState : notnull where TTrigger : notnull
{
    private ICollection<TransitionBehavior>? GetTransitionBehaviors(TTrigger trigger)
    {
        if (!_stateConfigs.TryGetValue(_currentState, out var stateConfig)) return null;
        return stateConfig.TransitionBehaviorDict.GetValueOrDefault(trigger);
    }

    private static void ExecuteActionCollection(List<Action<TriggerParams?>> actions, TriggerParams? triggerParams)
    {
        foreach (var action in actions)
        {
            action(triggerParams);
        }
    }

    private static void PrintCollection<T>(IEnumerable<T> collection)
    {
        foreach (var item in collection)
        {
            Console.Out.WriteLine(item.ToString());
        }
    }
}
