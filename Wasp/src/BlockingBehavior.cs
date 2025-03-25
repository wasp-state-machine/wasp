namespace Wasp;

public partial class Machine<TState, TTrigger> where TState : notnull where TTrigger : notnull
{
    internal class BlockingBehavior
    {
        internal BlockingBehavior(TTrigger trigger, TState origin, TState blockedDestination, Func<TriggerParams?,
                bool>? when = null)
        {
            Trigger = trigger;
            BlockedDestination = blockedDestination;
            Origin = origin;
            When = when;
        }

        internal bool GuardIsMet(TriggerParams? triggerParams)
        {
            return (When == null || When(triggerParams));
        }
        
        public override string ToString()
        {
            return Box(Origin) + " --BLOCKED-->| " + Box(BlockedDestination);
            
            string Box(TState state)
            {
                return "[ " + state + " ]";
            }
        }
        
        internal readonly TTrigger Trigger;
        internal readonly TState BlockedDestination;
        internal readonly TState Origin;
        internal Func<TriggerParams?, bool>? When;
    }
}