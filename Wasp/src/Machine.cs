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
    
    public TState State()
    {
        return _currentState;
    }
    
    private Dictionary<TState, StateConfig> _stateConfigs;
    private TState _currentState;
}