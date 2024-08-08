using System.Diagnostics;

namespace Wasp;

public partial class Machine<TState, TTrigger> where TState : notnull where TTrigger : notnull
{
    
    public void Fire(TTrigger trigger, TriggerParams? triggerParams = null)
    {
        TransitionBehavior? transitionBehavior = DetermineTransitionBehavior(trigger, triggerParams);
        if (transitionBehavior is null) return;

        HandleOriginStates(transitionBehavior.Origin, trigger, triggerParams);
        
        _currentState = transitionBehavior.Destination;
    }
    
    private TransitionBehavior? DetermineTransitionBehavior(TTrigger trigger, TriggerParams? triggerParams)
    {
        var transitionBehaviors = GetTransitionBehaviors(trigger);
        if (transitionBehaviors == null) return null;
        
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

    private void HandleOriginStates(TState originState, TTrigger trigger, TriggerParams? triggerParams)
    {
        if (!_stateConfigs.ContainsKey(originState)) return;
        var originStateConfigs = _stateConfigs[originState].GetSuperStateConfigs();
        Console.Out.WriteLine("Superstates:");
        PrintCollection(originStateConfigs);
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