namespace Test;

using Wasp;
using Xunit;

public class Block
{
    [Fact]
    public void Basic()
    {
        var machine = new Machine<State, Trigger>(State.A);

        machine.Configure(State.A)
            .SubstateOf(State.C)
            .Block(Trigger.X, State.B);

        machine.Configure(State.C)
            .Permit(Trigger.X, State.B);
        
        Assert.Equal(State.A, machine.State());
        
        machine.Fire(Trigger.X);
        
        Assert.Equal(State.A, machine.State());
    }
    

}