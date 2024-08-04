namespace Wasp;

public partial class Machine<TState, TTrigger> where TState : notnull where TTrigger : notnull
{
    public class TransitionBehavior
    {
        public TransitionBehavior(TTrigger trigger, TState destination, Guard? guard = null, int weight = 0)
        {
            Trigger = trigger;
            Destination = destination;
            _guard = guard;
            Weight = weight;
        }

        public bool GuardIsMet(TriggerParams? triggerParams)
        {
            return (_guard == null || _guard.Value(triggerParams));
        }

        public readonly TTrigger Trigger;
        public readonly TState Destination;
        private readonly Guard? _guard;
        public int Weight;
    }
}