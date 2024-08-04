namespace Wasp;

public partial class Machine<TState, TTrigger> where TState : notnull where TTrigger : notnull
{
    public class Guard
    {

        public Guard(Func<TriggerParams?, bool> clause)
        {
            Clause = clause;
        }
        
        public bool Value(TriggerParams? triggerParams)
        {
            return (Clause(triggerParams));
        }
        public Func<TriggerParams?, bool> Clause;
        
    }
}