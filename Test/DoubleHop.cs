namespace Test;

using Wasp;
using Xunit;

public class DoubleHop
{
    [Fact]
    public void Basic()
    {
        var machine = new Machine<State, Trigger>(State.A);

        bool b1 = false;
        bool b2 = false;
        bool b3 = false;

        
        // todo test value assignment,
        // onexit, etc
        
        machine.Configure(State.A)
            .Permit(Trigger.X, State.B)
            .OnExit(_ => machine.Fire(Trigger.X));

        machine.Configure(State.B)
            .Permit(Trigger.X, State.C);
        
        machine.Fire(Trigger.X);
        
        Assert.True(machine.IsInState(State.C));
    }
}