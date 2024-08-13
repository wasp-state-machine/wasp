using System.Diagnostics;

namespace Wasp;

public partial class Machine<TState, TTrigger> where TState : notnull where TTrigger : notnull
{
    
    public void Fire(TTrigger trigger, TriggerParams? triggerParams = null)
    {
        var originStateConfigs = GetSuperStateConfigs(_currentState);
        if (originStateConfigs == null) return;
        
        TransitionBehavior? transitionBehavior = DetermineTransitionBehavior(originStateConfigs, trigger, triggerParams);
        if (transitionBehavior is null) return;
        
        var destination = transitionBehavior.Destination;
        var destinationStateConfigs = GetSuperStateConfigs(destination);
        
        var exitActions = originStateConfigs
            .SelectMany(h => h.GetExitActions())
            .ToList();

        var exitFromActions = originStateConfigs
            .SelectMany(h => h.GetExitFromActions(trigger))
            .ToList();
        
        ExecuteActionCollection(exitActions, triggerParams);
        ExecuteActionCollection(exitFromActions, triggerParams);

        _onTransitioned?.Invoke(triggerParams);
        _currentState = transitionBehavior.Destination;
        
        if (destinationStateConfigs == null) return;

        var entryActions = destinationStateConfigs
            .SelectMany(h => h.GetEntryActions())
            .ToList();

        var entryFromActions = destinationStateConfigs
            .SelectMany(h => h.GetEntryFromActions(trigger))
            .ToList();
        
        ExecuteActionCollection(entryActions, triggerParams);
        ExecuteActionCollection(entryFromActions, triggerParams);

    }
    
    private TransitionBehavior? DetermineTransitionBehavior(List<StateConfig> originStateConfigs, TTrigger trigger, TriggerParams? triggerParams)
    {
        var transitionBehaviors = originStateConfigs
            .SelectMany(h => GetTransitionBehaviors(h, trigger) ?? [])
            .ToList();
        
        if (transitionBehaviors.Count == 0) return null;
        
        var passedGuard = transitionBehaviors
            .Where(h => h.GuardIsMet(triggerParams))
            .ToList();
        
        if (passedGuard.Count == 0) return null;
        
        int heaviestWeight = passedGuard.Max(h => h.Weight);
        var heaviestBehaviors = passedGuard
            .Where(h => h.Weight == heaviestWeight)
            .ToList();
        
        if (heaviestBehaviors.Count == 0) return null;
        
        HandleAmbiguousTransitions(trigger, heaviestBehaviors);

        return heaviestBehaviors[0];
    }

    private List<StateConfig>? GetSuperStateConfigs(TState state)
    {
        if (!_stateConfigs.ContainsKey(state)) return null;
        var originStateConfigs = _stateConfigs[state].GetSuperStateConfigs();
        return originStateConfigs;
    }
    
    private static void HandleAmbiguousTransitions(TTrigger trigger, List<TransitionBehavior> transitionBehaviors)
    {
        if (transitionBehaviors.Count <= 1) return;
        var message = "Ambiguous transition on fire trigger " + trigger + 
                      ", found these possible transitions: ";
        foreach (var transitionBehavior in transitionBehaviors)
        {
            message += "\n\t" + transitionBehavior;
        }
        throw (new InvalidOperationException(message));
    }

}