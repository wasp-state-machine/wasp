namespace Wasp;

public partial class Machine<TState, TTrigger> where TState : notnull where TTrigger : notnull
{

    public void Fire(TTrigger trigger, TriggerParams? triggerParams = null)
    {
        var transitionBehaviors = GetTransitionBehaviors(trigger);
        if (transitionBehaviors == null) return;
        
        // check guard clauses here
        
        HandleAmbiguousTransition(trigger, transitionBehaviors);
        
        foreach (var transitionBehavior in transitionBehaviors)
        {
            
            _currentState = transitionBehavior.Destination;
            // do callbacks here
            return;
        }
    }

    private void HandleAmbiguousTransition(TTrigger trigger, List<TransitionBehavior> transitionBehaviors)
    {
        if (transitionBehaviors.Count > 1)
        {
            string message = "Found multiple transitions available for state " + _currentState.ToString() +
                             " with trigger " + trigger.ToString();
            throw (new InvalidOperationException(message));
        }
    }

}