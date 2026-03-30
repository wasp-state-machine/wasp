using System.Diagnostics;

namespace Wasp;

public partial class Machine<TState, TTrigger> where TState : notnull where TTrigger : notnull
{
    
    private readonly List<Action<TriggerParams?>> _actionBuffer = new List<Action<TriggerParams?>>(16);

    public void Fire(TTrigger trigger, TriggerParams? triggerParams = null)
    {
        var originStateConfigs = GetSuperStateConfigs(_currentState);
        if (originStateConfigs == null) return;
        
        TransitionBehavior? transitionBehavior = DetermineTransitionBehavior(originStateConfigs, trigger, triggerParams);
        if (transitionBehavior is null) return;
        
        if (DoesViolateReentryPolicy(transitionBehavior)) return;
        
        var destination = transitionBehavior.Destination;
        
        FillBuffer(originStateConfigs, config => config.GetExitActions());
        FillBuffer(originStateConfigs, config => config.GetExitFromActions(trigger));
        
        _onTransitioned?.Invoke(triggerParams);
        _currentState = destination;
        
        ExecuteBufferedActions(triggerParams);
        
        var destinationStateConfigs = GetSuperStateConfigs(destination);
        if (destinationStateConfigs == null) return;

        FillBuffer(destinationStateConfigs, config => config.GetEntryActions());
        FillBuffer(destinationStateConfigs, config => config.GetEntryFromActions(trigger));
        
        ExecuteBufferedActions(triggerParams);
        
        _onTransitionCompleted?.Invoke(triggerParams);
    }

    private void FillBuffer(List<StateConfig> configs, Func<StateConfig, IEnumerable<Action<TriggerParams?>>> selector)
    {
        for (int i = 0; i < configs.Count; i++)
        {
            var actions = selector(configs[i]);
            if (actions == null) continue;
            foreach (var action in actions)
            {
                if (action != null) _actionBuffer.Add(action);
            }
        }
    }

private void ExecuteBufferedActions(TriggerParams? triggerParams)
{
    if (_actionBuffer.Count == 0) return;

    // We copy the buffer to a local array or loop in a way that allows recursion
    // To be 100% safe against "Double Hops", we iterate a local snapshot
    var actionsToRun = _actionBuffer.ToArray(); 
    _actionBuffer.Clear();

    for (int i = 0; i < actionsToRun.Length; i++)
    {
        actionsToRun[i](triggerParams);
    }
}
    
    public void Jump(TState state, TriggerParams? triggerParams = null)
    {
        var originStateConfigs = GetSuperStateConfigs(_currentState);
        var destinationStateConfigs = GetSuperStateConfigs(state);
        
        _onTransitioned?.Invoke(triggerParams);
        
        if (originStateConfigs != null)
        {
            var exitActions = originStateConfigs
                .SelectMany(h => h.GetExitActions())
                .ToList();
            
            ExecuteActionCollection(exitActions, triggerParams);
        }

        _currentState = state;
        
        if (destinationStateConfigs != null)
        {
            var entryActions = destinationStateConfigs
                .SelectMany(h => h.GetEntryActions())
                .ToList();

            ExecuteActionCollection(entryActions, triggerParams);
        }
        
        _onTransitionCompleted?.Invoke(triggerParams);
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

        var blockingBehaviors = originStateConfigs
            .SelectMany(h => GetBlockingBehaviors(h, trigger) ?? [])
            .ToList();

        var blockingPassedGuard = blockingBehaviors
            .Where(h => h.GuardIsMet(triggerParams))
            .ToList();

        var blockedDestinations = new HashSet<TState>(
            blockingPassedGuard.Select(b => b.BlockedDestination)
                );
        
        var unblocked = passedGuard
            .Where(transitionBehavior => !blockedDestinations.Contains(transitionBehavior.Destination))
            .ToList();


        int heaviestWeight = passedGuard.Max(h => h.Weight);
        var heaviestBehaviors = unblocked
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

    private bool DoesViolateReentryPolicy(TransitionBehavior transitionBehavior)
    {
        if (!Equals(_currentState, transitionBehavior.Destination)) return false;
        var substateConfig = _stateConfigs[transitionBehavior.Origin];
        if (substateConfig is null) return false;
        return !substateConfig.HasReentryTrigger(transitionBehavior.Trigger);
    }

}