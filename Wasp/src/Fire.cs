using System.Diagnostics;

namespace Wasp;

public partial class Machine<TState, TTrigger> where TState : notnull where TTrigger : notnull
{

    /// <summary>
    /// Transition from the current state via the specified trigger.
    /// The destination state is determined by the configuration of the current state.
    /// Actions associated with leaving the current state and entering the new one
    /// will be invoked.
    /// </summary>
    /// <param name="trigger">The trigger used to initiate the transition</param>
    /// <param name="triggerParams">The parameters passed to transition guard clauses, state entry actions,
    /// and state exit actions</param>
    public void Fire(TTrigger trigger, TriggerParams? triggerParams = null)
    {
        TransitionBehavior? transitionBehavior = DetermineTransitionBehavior(trigger, triggerParams);
        if (transitionBehavior is null) return;
        
        
        _currentState = transitionBehavior.Destination;
    }
    
    /// <summary>
    /// Judges guard clauses and weights for all possible transitions to reduce them down to one. If more than one
    /// qualifies, throws an InvalidOperationException
    /// </summary>
    /// <param name="trigger">Trigger fired</param>
    /// <param name="triggerParams">TriggerParams passed to Fire</param>
    /// <returns></returns>
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
    
    /// <summary>
    /// Enforces that at most 1 TransitionBehavior matches the Trigger. If not, throws an InvalidOperationException.
    /// </summary>
    /// <param name="trigger">Trigger fired</param>
    /// <param name="transitionBehaviors">TriggerParams passed to Fire</param>
    /// <exception cref="InvalidOperationException">If more than one behavior matches Trigger</exception>
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