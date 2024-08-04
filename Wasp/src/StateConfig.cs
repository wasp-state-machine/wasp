namespace Wasp;

public partial class Machine<TState, TTrigger> where TState : notnull where TTrigger : notnull
{
    public class StateConfig
    {
        public StateConfig(TState state)
        {
            State = state;
            TriggerBehaviors = new Dictionary<TTrigger, List<TransitionBehavior>>();
            _superStates = new List<TState>();
        }
    
        public StateConfig Permit(TTrigger trigger, TState destination)
        {
            TransitionBehavior transitionBehavior = new TransitionBehavior(trigger, destination, null);
            AddTriggerBehavior(transitionBehavior);
            return this;
        }

        private void AddTriggerBehavior(TransitionBehavior transitionBehavior)
        {
            if (!TriggerBehaviors.ContainsKey(transitionBehavior.Trigger))
            {
                TriggerBehaviors[transitionBehavior.Trigger] = [];
            }
            TriggerBehaviors[transitionBehavior.Trigger].Add(transitionBehavior);
        }
        
        public Dictionary<TTrigger, List<TransitionBehavior>> TriggerBehaviors;
        private List<TState> _superStates;
        public TState State;
    }
}