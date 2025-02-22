namespace Test;

using Wasp;
using Xunit;

public class Jump
{
    [Fact]
    public void Basic()
    {
        var machine = new Machine<State, Trigger>(State.A);

        bool b1 = false;
        bool b2 = true;
        bool b3 = false;
        
        machine.Configure(State.A)
            .OnExit(_ => b1 = true)
            .OnExit(_ => b2 = false);

        machine.Configure(State.B)
            .OnEntry(_ => b3 = true);
        
        machine.Jump(State.B);
        
        Assert.True(machine.IsInState(State.B));
        Assert.True(b1);
        Assert.False(b2);
        Assert.True(b3);
    }
}