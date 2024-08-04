namespace Wasp;

public partial class Machine<TState, TTrigger> where TState : notnull where TTrigger : notnull
{
    public Machine(TState initialState)
    {
        _currentState = initialState;
        _stateConfigs = new Dictionary<TState, StateConfig>();
    }

    public StateConfig Configure(TState state)
    {
        if (!_stateConfigs.ContainsKey(state)) _stateConfigs[state] = new StateConfig(state);
        return _stateConfigs[state];
    }

    public void Fire(TTrigger trigger)
    {
        if (!_stateConfigs.ContainsKey(_currentState)) return;
        if (!_stateConfigs[_currentState].TriggerBehaviors.ContainsKey(trigger)) return;
        
        var transitionBehaviors = _stateConfigs[_currentState].TriggerBehaviors[trigger];
        if (transitionBehaviors.Count > 1)
        {
            string message = "Found multiple transitions available for state " + _currentState.ToString() +
                             " with trigger " + trigger.ToString();
            throw (new InvalidOperationException(message));
        }
        foreach (var transitionBehavior in transitionBehaviors)
        {
            // check guard clause here
            _currentState = transitionBehavior.Destination;
            return;
        }
    }

    public TState State()
    {
        return _currentState;
    }
    
    private Dictionary<TState, StateConfig> _stateConfigs;
    private TState _currentState;
}