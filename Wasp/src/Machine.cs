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
        if (superStateConfigs == null) return false;
        
        foreach (var t in superStateConfigs)
        {
            // Use EqualityComparer to avoid boxing if TState is a value type (like an Enum)
            if (EqualityComparer<TState>.Default.Equals(t.State(), state))
            {
                return true;
            }
        }

        return false;
    }

    public void OnTransitioned(Action<TriggerParams?>? action)
    {
        _onTransitioned = action;
    }
    
    public void OnTransitionCompleted(Action<TriggerParams?>? action)
    {
        _onTransitionCompleted = action;
    }

    public void Assume(TState state)
    {
        _currentState = state;
    }

    public void BakeRecursiveSuperstates()
    {
        foreach (var kvp in _stateConfigs)
        {
            kvp.Value.BakeRecursiveSuperstates();
        }
    }
    
    
    private IDictionary<TState, StateConfig> _stateConfigs;
    private TState _currentState;
    private Action<TriggerParams?>? _onTransitioned;
    private Action<TriggerParams?>? _onTransitionCompleted;
}