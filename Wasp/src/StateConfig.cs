namespace Wasp;

public partial class Machine<TState, TTrigger> where TState : notnull where TTrigger : notnull
{
    public class StateConfig
    {
        public StateConfig(TState state)
        {
            State = state;
            TriggerBehaviorDict = new Dictionary<TTrigger, ICollection<TransitionBehavior>>();
            _superStates = new List<TState>();
        }
    
        public StateConfig Permit(TTrigger trigger, TState destination)
        {
            TransitionBehavior transitionBehavior = new TransitionBehavior(trigger, destination);
            AddTriggerBehavior(transitionBehavior);
            return this;
        }
        
        public StateConfig PermitIf(TTrigger trigger, TState destination, Func<TriggerParams?, bool> clause, int weight = 0)
        {
            Guard guard = new Guard(clause);
            TransitionBehavior transitionBehavior = new TransitionBehavior(trigger, destination, guard, weight);
            AddTriggerBehavior(transitionBehavior);
            return this;
        }

        private void AddTriggerBehavior(TransitionBehavior transitionBehavior)
        {
            if (!TriggerBehaviorDict.ContainsKey(transitionBehavior.Trigger))
            {
                TriggerBehaviorDict[transitionBehavior.Trigger] = [];
            }
            TriggerBehaviorDict[transitionBehavior.Trigger].Add(transitionBehavior);
        }
        
        public Dictionary<TTrigger, ICollection<TransitionBehavior>> TriggerBehaviorDict;
        private List<TState> _superStates;
        public TState State;
    }
}