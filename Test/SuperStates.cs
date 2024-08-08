namespace Test;

using Wasp;
using Xunit;

public class SuperStates
{
    [Fact]
    public void Basic()
    {
        var machine = new Machine<State, Trigger>(State.A);

        machine.Configure(State.A)
            .Permit(Trigger.X, State.B)
            .SubstateOf(State.C);

        machine.Configure(State.C);
        
        machine.Fire(Trigger.X);
    }
    
}