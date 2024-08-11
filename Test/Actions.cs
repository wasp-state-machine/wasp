using System.Data;
using System.Security.Authentication;
using System.Text;

namespace Test;

using Wasp;
using Xunit;

public class Actions
{
    
    [Fact]
    public void Basic()
    {
        var machine = new Machine<State, Trigger>(State.A);

        bool b1 = false;
        bool b2 = true;
        bool b3 = false;
        
        machine.Configure(State.A)
            .Permit(Trigger.X, State.B)
            .OnExit(_ => b1 = true)
            .OnExit(_ => b2 = false);

        machine.Configure(State.B)
            .OnEntry(_ => b3 = true);
        
        machine.Fire(Trigger.X);
        
        Assert.True(b1);
        Assert.False(b2);
        Assert.True(b3);
    }
    
    [Fact]
    public void From()
    {
        var machine = new Machine<State, Trigger>(State.A);

        bool b1 = false;
        bool b2 = false;
        bool b3 = false;
        bool b4 = false;

        machine.Configure(State.A)
            .Permit(Trigger.X, State.C)
            .OnExitFrom(Trigger.X, _ => b1 = true)
            .OnExitFrom(Trigger.Y, _ => b2 = true);
        
        machine.Configure(State.C)
            .OnEntryFrom(Trigger.X, _ => b3 = true)
            .OnEntryFrom(Trigger.Y, _ => b4 = true);
        
        machine.Fire(Trigger.X);
        
        Assert.True(b1);
        Assert.False(b2);
        Assert.True(b3);
        Assert.False(b4);
    }
    
    [Fact]
    public void Super()
    {
        var machine = new Machine<State, Trigger>(State.A);

        bool b1 = false;
        bool b2 = false;
        bool b3 = false;
        bool b4 = false;

        machine.Configure(State.A)
            .Permit(Trigger.X, State.C)
            .OnExit(_ => b1 = true)
            .SubstateOf(State.B);

        machine.Configure(State.B)
            .OnExit(_ => b2 = true);

        machine.Configure(State.C)
            .OnEntry(_ => b3 = true)
            .SubstateOf(State.D);

        machine.Configure(State.D)
            .OnEntry(_ => b4 = true);
        
        machine.Fire(Trigger.X);
        
        Assert.True(b1);
        Assert.True(b2);
        Assert.True(b3);
        Assert.True(b4);
    }
}