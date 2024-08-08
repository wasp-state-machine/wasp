namespace Wasp;

public partial class Machine<TState, TTrigger> where TState : notnull where TTrigger : notnull
{
    public class StateConfig
    {
        internal StateConfig(TState state, Machine<TState, TTrigger> machine)
        {
            _state = state;
            _machine = machine;
            TransitionBehaviorDict = new Dictionary<TTrigger, ICollection<TransitionBehavior>>();
            _entryActions = new List<Action<TriggerParams?>>();
            _exitActions = new List<Action<TriggerParams?>>();
            SuperStates = new List<TState>();
        }
    
        public StateConfig Permit(TTrigger trigger, TState destination)
        {
            TransitionBehavior transitionBehavior = new TransitionBehavior(trigger, _state, destination);
            AddTransitionBehavior(transitionBehavior);
            return this;
        }
        
        public StateConfig PermitIf(TTrigger trigger, TState destination, Func<TriggerParams?, bool> clause, int weight = 0)
        {
            TransitionBehavior transitionBehavior = new TransitionBehavior(trigger, _state, destination, clause, weight);
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

        public StateConfig SubstateOf(TState superState)
        {
            SuperStates.Add(superState);
            return this;
        }

        internal List<StateConfig> GetSuperStateConfigs()
        {
            return ResolveSuperStateConfigsDepthFirst([], this).ToList();
        }

        private HashSet<StateConfig> ResolveSuperStateConfigsDepthFirst(HashSet<StateConfig> stateConfigs, StateConfig stateConfig)
        {
            if (stateConfigs.Contains(stateConfig)) return stateConfigs;
            
            stateConfigs.Add(stateConfig);
            
            foreach (var state in stateConfig.SuperStates)
            {
                if (!_machine._stateConfigs.ContainsKey(state)) continue;
                ResolveSuperStateConfigsDepthFirst(stateConfigs, _machine._stateConfigs[state]);
            }

            return stateConfigs;
        }

        private void AddTransitionBehavior(TransitionBehavior transitionBehavior)
        {
            if (!TransitionBehaviorDict.ContainsKey(transitionBehavior.Trigger))
            {
                TransitionBehaviorDict[transitionBehavior.Trigger] = [];
            }
            TransitionBehaviorDict[transitionBehavior.Trigger].Add(transitionBehavior);
        }
        
        private TState _state;
        protected List<TState> SuperStates;
        private Machine<TState, TTrigger> _machine;
        internal Dictionary<TTrigger, ICollection<TransitionBehavior>> TransitionBehaviorDict;
        private List<Action<TriggerParams?>> _entryActions;
        private List<Action<TriggerParams?>> _exitActions;
    }
}