namespace Wasp;

public partial class Machine<TState, TTrigger> where TState : notnull where TTrigger : notnull
{
    public class TransitionBehavior
    {
        public TransitionBehavior(TTrigger trigger, TState destination, Guard? guard)
        {
            Trigger = trigger;
            Destination = destination;
            Guard = guard;
        }

        public TTrigger Trigger;
        public TState Destination;
        public Guard? Guard;
    }
}