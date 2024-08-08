namespace Wasp;

public partial class Machine<TState, TTrigger> where TState : notnull where TTrigger : notnull
{
    internal class TransitionBehavior
    {
        internal TransitionBehavior(TTrigger trigger, TState origin, TState destination, Func<TriggerParams?,
                bool>? guardClause = null, int weight = 0)
        {
            Trigger = trigger;
            Destination = destination;
            Origin = origin;
            GuardClause = guardClause;
            _actions = new List<Action<TriggerParams?>>();
            Weight = weight;
        }

        internal bool GuardIsMet(TriggerParams? triggerParams)
        {
            return (GuardClause == null || GuardClause(triggerParams));
        }
        
        internal void AddAction(Action<TriggerParams?> action)
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
        
        internal readonly TTrigger Trigger;
        internal readonly TState Destination;
        internal readonly TState Origin;
        internal Func<TriggerParams?, bool>? GuardClause;
        private List<Action<TriggerParams?>> _actions;
        internal readonly int Weight;
    }
}