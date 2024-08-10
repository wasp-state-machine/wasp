namespace Test;

using Wasp;
using Xunit;

public class SuperStates
{
    private static bool GeZero(TriggerParams? t)
    {
        if (t is null) return false;
        var testParams = (TestParams)t;
        return testParams.ParamA >= 0;
    }
        
    private static bool LeZero(TriggerParams? t)
    {
        if (t is null) return false;
        var testParams = (TestParams)t;
        return testParams.ParamA <= 0;
    }
    
    [Fact]
    public void Basic()
    {
        var machine = new Machine<State, Trigger>(State.A);

        machine.Configure(State.A)
            .Permit(Trigger.X, State.B)
            .SubstateOf(State.C);

        machine.Configure(State.C)
            .Permit(Trigger.Y, State.D);
        
        machine.Fire(Trigger.Y);
        
        Assert.Equal(State.D, machine.State());
        
        machine.Fire(Trigger.X);
        
        Assert.Equal(State.D, machine.State());
    }

    [Fact]
    public void Ambiguous()
    {
        var machine = new Machine<State, Trigger>(State.A);

        machine.Configure(State.A)
            .Permit(Trigger.X, State.B)
            .SubstateOf(State.C);

        machine.Configure(State.C)
            .Permit(Trigger.X, State.D);
        
        Assert.Throws<InvalidOperationException>(() => { machine.Fire(Trigger.X); });
    }
    
    [Fact]
    public void Guard()
    {
        var machine = new Machine<State, Trigger>(State.A);

        machine.Configure(State.A)
            .PermitIf(Trigger.X, State.B, GeZero)
            .SubstateOf(State.C);

        machine.Configure(State.C)
            .PermitIf(Trigger.X, State.D, LeZero);


        TestParams testParams = new TestParams() { ParamA = -1 };
        
        machine.Fire(Trigger.X, testParams);
        
        Assert.Equal(State.D, machine.State());
        
    }

    [Fact]
    public void AmbiguousGuard()
    {
        var machine = new Machine<State, Trigger>(State.A);

        machine.Configure(State.A)
            .PermitIf(Trigger.X, State.B, GeZero)
            .SubstateOf(State.C);

        machine.Configure(State.C)
            .PermitIf(Trigger.X, State.D, LeZero);


        TestParams testParams = new TestParams() { ParamA = 0 };
        Assert.Throws<InvalidOperationException>(() => { machine.Fire(Trigger.X, testParams); });
    }
    
}