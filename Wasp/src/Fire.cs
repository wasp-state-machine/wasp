using System.Diagnostics;

namespace Wasp;

public partial class Machine<TState, TTrigger> where TState : notnull where TTrigger : notnull
{

    public void Fire(TTrigger trigger, TriggerParams? triggerParams = null)
    {
        var transitionBehaviors = GetTransitionBehaviors(trigger);
        if (transitionBehaviors == null) return;
        
        // Execute guard clauses
        var passedGuard = transitionBehaviors
            .Where(h => h.GuardIsMet(triggerParams))
            .ToList();
        
        // Discard low-weight behaviors
        int heaviestWeight = passedGuard.Max(h => h.Weight);
        var heaviestBehaviors = passedGuard
            .Where(h => h.Weight == heaviestWeight)
            .ToList();
        
        // heaviestBehaviors.Count should now be 1
        HandleAmbiguousTransition(trigger, heaviestBehaviors);
        
        _currentState = heaviestBehaviors[0].Destination;
    }
    
    private void HandleAmbiguousTransition(TTrigger trigger, IEnumerable<TransitionBehavior> transitionBehaviors)
    {
        if (transitionBehaviors.Count() <= 1) return;
        var message = "Found multiple transitions available for state " + _currentState.ToString() +
                      " with trigger " + trigger.ToString();
        throw (new InvalidOperationException(message));
    }

}