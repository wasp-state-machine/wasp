namespace Wasp;

public partial class Machine<TState, TTrigger> where TState : notnull where TTrigger : notnull
{
    public class StateConfig
    {
        public StateConfig(TState state)
        {
            State = state;
            TransitionBehaviorDict = new Dictionary<TTrigger, ICollection<TransitionBehavior>>();
            _entryActions = new List<Action<TriggerParams?>>();
            _exitActions = new List<Action<TriggerParams?>>();
            _superStates = new List<TState>();
        }
    
        public StateConfig Permit(TTrigger trigger, TState destination)
        {
            TransitionBehavior transitionBehavior = new TransitionBehavior(trigger, State, destination);
            AddTransitionBehavior(transitionBehavior);
            return this;
        }
        
        public StateConfig PermitIf(TTrigger trigger, TState destination, Func<TriggerParams?, bool> clause, int weight = 0)
        {
            TransitionBehavior transitionBehavior = new TransitionBehavior(trigger, State, destination, clause, weight);
            AddTransitionBehavior(transitionBehavior);
            return this;
        }

        public StateConfig OnEntry(Action<TriggerParams?> action)
        {
            _entryActions.Add(action);
            return this;
        }
        
        public StateConfig OnExit(Action<TriggerParams?> action)
        {
            _exitActions.Add(action);
            return this;
        }

        private void AddTransitionBehavior(TransitionBehavior transitionBehavior)
        {
            if (!TransitionBehaviorDict.ContainsKey(transitionBehavior.Trigger))
            {
                TransitionBehaviorDict[transitionBehavior.Trigger] = [];
            }
            TransitionBehaviorDict[transitionBehavior.Trigger].Add(transitionBehavior);
        }
        
        public Dictionary<TTrigger, ICollection<TransitionBehavior>> TransitionBehaviorDict;
        private List<Action<TriggerParams?>> _entryActions;
        private List<Action<TriggerParams?>> _exitActions;
        private List<TState> _superStates;
        public TState State;
    }
}