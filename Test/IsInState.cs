namespace Test;

using Wasp;
using Xunit;

public class IsInState
{
    [Fact]
    public void Basic()
    {
        var machine = new Machine<State, Trigger>(State.A);

        machine.Configure(State.A)
            .SubstateOf(State.B)
            .SubstateOf(State.C);
        
        Assert.True(machine.IsInState(State.A));
        Assert.True(machine.IsInState(State.B));
        Assert.True(machine.IsInState(State.C));
        Assert.False(machine.IsInState(State.D));

        machine.Configure(State.C)
            .SubstateOf(State.D);
        
        Assert.True(machine.IsInState(State.D));
    }
    
}