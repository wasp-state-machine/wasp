namespace Wasp;

public partial class Machine<TState, TTrigger> where TState : notnull where TTrigger : notnull
{
    public Machine(TState initialState)
    {
        _currentState = initialState;
        _stateConfigs = new Dictionary<TState, StateConfig>();
        _onTransitioned = null;
    }

    public StateConfig Configure(TState state)
    {
        if (!_stateConfigs.ContainsKey(state)) _stateConfigs[state] = new StateConfig(state, this);
        return _stateConfigs[state];
    }
    
    public TState State()
    {
        return _currentState;
    }

    public void OnTransitioned(Action<TriggerParams?>? action)
    {
        _onTransitioned = action;
    }
    
    private IDictionary<TState, StateConfig> _stateConfigs;
    private TState _currentState;
    private Action<TriggerParams?>? _onTransitioned;
}