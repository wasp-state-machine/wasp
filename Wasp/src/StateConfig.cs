namespace Wasp;

public partial class Machine<TState, TTrigger> where TState : notnull where TTrigger : notnull
{
    public class StateConfig
    {
        public StateConfig(TState state)
        {
            State = state;
            TransitionBehaviorDict = new Dictionary<TTrigger, ICollection<TransitionBehavior>>();
            _superStates = new List<TState>();
        }
    
        public StateConfig Permit(TTrigger trigger, TState destination)
        {
            TransitionBehavior transitionBehavior = new TransitionBehavior(trigger, destination);
            AddTransitionBehavior(transitionBehavior);
            return this;
        }
        
        public StateConfig PermitIf(TTrigger trigger, TState destination, Func<TriggerParams?, bool> clause, int weight = 0)
        {
            Guard guard = new Guard(clause);
            TransitionBehavior transitionBehavior = new TransitionBehavior(trigger, destination, guard, weight);
            AddTransitionBehavior(transitionBehavior);
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
        private List<TState> _superStates;
        public TState State;
    }
}