namespace Test;

using Wasp;
using Xunit;

public class Reentry
{
    [Fact]
    public void Basic()
    {
        bool b1 = false;
        
        var machine = new Machine<State, Trigger>(State.A);
        
        machine.Configure(State.A)
            .Permit(Trigger.X, State.A)
            .OnExit(_ => b1 = true);
        
        machine.Fire(Trigger.X);
        
        Assert.False(b1);
        
        var machine2 = new Machine<State, Trigger>(State.A);
        
        machine2.Configure(State.A)
            .Permit(Trigger.X, State.A)
            .AllowReentry(Trigger.X)
            .OnExit(_ => b1 = true);
        
        machine2.Fire(Trigger.X);
        
        Assert.True(b1);
    }

    [Fact]
    public void Super()
    {
        bool b1 = false;
        
        var machine = new Machine<State, Trigger>(State.A);
        
        machine.Configure(State.A)
            .SubstateOf(State.B)
            .OnExit(_ => b1 = true);

        machine.Configure(State.B)
            .Permit(Trigger.X, State.A);
        
        machine.Fire(Trigger.X);
        
        Assert.False(b1);

    }
    
    [Fact]
    public void Super2()
    {
        bool b1 = false;
        
        var machine = new Machine<State, Trigger>(State.A);
        
        machine.Configure(State.A)
            .SubstateOf(State.B)
            .AllowReentry(Trigger.Y)
            .OnExit(_ => b1 = true);

        machine.Configure(State.B)
            .Permit(Trigger.X, State.A);
        
        machine.Fire(Trigger.X);
        
        Assert.False(b1);

    }
}