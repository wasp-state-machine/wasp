using System.Diagnostics;

namespace Wasp;

public partial class Machine<TState, TTrigger> where TState : notnull where TTrigger : notnull
{
    
    public void Fire(TTrigger trigger, TriggerParams? triggerParams = null)
    {
        var originStateConfigs = GetOriginStateConfigs(_currentState, trigger, triggerParams);
        TransitionBehavior? transitionBehavior = DetermineTransitionBehavior(originStateConfigs, trigger, triggerParams);
        if (transitionBehavior is null) return;

        
        _currentState = transitionBehavior.Destination;
    }
    
    private TransitionBehavior? DetermineTransitionBehavior(List<StateConfig>? originStateConfigs, TTrigger trigger, TriggerParams? triggerParams)
    {
        if (originStateConfigs == null) return null;

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

    private List<StateConfig>? GetOriginStateConfigs(TState originState, TTrigger trigger, TriggerParams? triggerParams)
    {
        if (!_stateConfigs.ContainsKey(originState)) return null;
        var originStateConfigs = _stateConfigs[originState].GetSuperStateConfigs();
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