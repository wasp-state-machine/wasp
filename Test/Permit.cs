namespace Test;

using Wasp;
using Xunit;

public class Permit
{
    [Fact]
    public void Basic()
    {
        var machine = new Machine<State, Trigger>(State.A);

        machine.Configure(State.A)
            .Permit(Trigger.X, State.B);

        machine.Configure(State.B)
            .Permit(Trigger.Y, State.C);
        
        Assert.Equal(State.A, machine.State());
        
        machine.Fire(Trigger.X);
        
        Assert.Equal(State.B, machine.State());
        
        machine.Fire(Trigger.X);
        
        Assert.Equal(State.B, machine.State());
        
        machine.Fire(Trigger.Y);
        
        Assert.Equal(State.C, machine.State());
    }

    [Fact]
    public void AssertAmbiguousTransition()
    {
        var machine = new Machine<State, Trigger>(State.A);

        machine.Configure(State.A)
            .Permit(Trigger.X, State.B)
            .Permit(Trigger.X, State.C);

        Assert.Throws<InvalidOperationException>(() => { machine.Fire(Trigger.X); });

    }
}