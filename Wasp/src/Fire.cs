namespace Wasp;

public partial class Machine<TState, TTrigger> where TState : notnull where TTrigger : notnull
{

    public void Fire(TTrigger trigger, TriggerParams? triggerParams = null)
    {
        var transitionBehaviors = GetTransitionBehaviors(trigger);
        if (transitionBehaviors == null) return;

        var passedGuard = transitionBehaviors
            .Where(h => h.GuardIsMet(triggerParams))
            .ToList();

        Console.Out.WriteLine("passedGuard length: " + passedGuard.Count.ToString());
        
        HandleAmbiguousTransition(trigger, passedGuard);
        
        foreach (var transitionBehavior in passedGuard)
        {
            
            _currentState = transitionBehavior.Destination;
            // do callbacks here
            return;
        }
    }

    private void HandleAmbiguousTransition(TTrigger trigger, IEnumerable<TransitionBehavior> transitionBehaviors)
    {
        if (transitionBehaviors.Count() <= 1) return;
        var message = "Found multiple transitions available for state " + _currentState.ToString() +
                      " with trigger " + trigger.ToString();
        throw (new InvalidOperationException(message));
    }

}