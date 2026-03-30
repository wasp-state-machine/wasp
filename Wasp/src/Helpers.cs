namespace Wasp;

public partial class Machine<TState, TTrigger> where TState : notnull where TTrigger : notnull
{
    private ICollection<TransitionBehavior>? GetTransitionBehaviors(StateConfig stateConfig, TTrigger trigger)
    {
        if (!stateConfig.TransitionBehaviorDict.ContainsKey(trigger)) return null;
        return stateConfig.TransitionBehaviorDict[trigger];
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
            if (item == null) continue;
            Console.Out.WriteLine(item.ToString());
        }
    }
}
