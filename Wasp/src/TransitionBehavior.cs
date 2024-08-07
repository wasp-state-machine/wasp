namespace Wasp;

public partial class Machine<TState, TTrigger> where TState : notnull where TTrigger : notnull
{
    public class TransitionBehavior
    {
        public TransitionBehavior(TTrigger trigger, TState origin, TState destination, Func<TriggerParams?,
                bool>? guardClause = null, int weight = 0)
        {
            Trigger = trigger;
            Destination = destination;
            Origin = origin;
            GuardClause = guardClause;
            _actions = new List<Action<TriggerParams?>>();
            Weight = weight;
        }

        public bool GuardIsMet(TriggerParams? triggerParams)
        {
            return (GuardClause == null || GuardClause(triggerParams));
        }
        
        public void AddAction(Action<TriggerParams?> action)
        {
            _actions.Add(action);
        }
        
        public override string ToString()
        {
            var weight = GuardClause is null ? "" : "   ( weight: " + Weight + " )";
            return Box(Origin) + " ---> " + Box(Destination) + weight;
            
            string Box(TState state)
            {
                return "[ " + state + " ]";
            }
        }
        
        public readonly TTrigger Trigger;
        public readonly TState Destination;
        public readonly TState Origin;
        public Func<TriggerParams?, bool>? GuardClause;
        private List<Action<TriggerParams?>> _actions;
        public readonly int Weight;
    }
}