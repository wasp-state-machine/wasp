namespace Wasp;

public partial class Machine<TState, TTrigger> where TState : notnull where TTrigger : notnull
{
    public Machine(TState initialState)
    {
        _currentState = initialState;
        _stateConfigs = new Dictionary<TState, StateConfig>();
        _onTransitioned = null;
        Configure(_currentState);
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

    public bool IsInState(TState state)
    {
        var superStateConfigs = GetSuperStateConfigs(_currentState);
        if (superStateConfigs is null) return false;

        var matchingSuperStates = superStateConfigs
            .Select(h => h.State())
            .Where(h => Equals(h, state));

        return matchingSuperStates.Count() != 0;
    }

    public void OnTransitioned(Action<TriggerParams?>? action)
    {
        _onTransitioned = action;
    }
    
    private IDictionary<TState, StateConfig> _stateConfigs;
    private TState _currentState;
    private Action<TriggerParams?>? _onTransitioned;
}