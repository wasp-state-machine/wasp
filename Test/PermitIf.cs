namespace Test;

using Wasp;
using Xunit;

public class PermitIf
{
    
    private static bool GtZero(TriggerParams? t)
    {
        if (t is null) return false;
        var testParams = (TestParams)t;
        return testParams.ParamA > 0;
    }
        
    private static bool LtZero(TriggerParams? t)
    {
        if (t is null) return false;
        var testParams = (TestParams)t;
        return testParams.ParamA < 0;
    }
    
    [Fact]
    public void Basic()
    {
        var machine = new Machine<State, Trigger>(State.A);

        machine.Configure(State.A)
            .Permit(Trigger.X, State.B)
            .PermitIf(Trigger.X, State.C, _ => false);

        machine.Configure(State.B)
            .PermitIf(Trigger.Y, State.C, _ => false)
            .PermitIf(Trigger.Y, State.D, _ => true);
        
        Assert.Equal(State.A, machine.State());
        
        machine.Fire(Trigger.X);
        
        Assert.Equal(State.B, machine.State());
        
        machine.Fire(Trigger.Y);
        
        Assert.Equal(State.D, machine.State());
    }

    [Fact]
    public void AssertAmbiguousTransition()
    {
        var machine1 = new Machine<State, Trigger>(State.A);

        machine1.Configure(State.A)
            .PermitIf(Trigger.X, State.D, _ => true)
            .PermitIf(Trigger.X, State.C, _ => true);

        Assert.Throws<InvalidOperationException>(() => { machine1.Fire(Trigger.X); });
        
        
        var machine2 = new Machine<State, Trigger>(State.A);

        machine2.Configure(State.A)
            .PermitIf(Trigger.X, State.D, GtZero)
            .PermitIf(Trigger.X, State.C, GtZero);

        var testParams = new TestParams() { ParamA = 3 };
        Assert.Throws<InvalidOperationException>(() => { machine2.Fire(Trigger.X, testParams); });
    }

    [Fact]
    public void WithParams()
    {
        var machine = new Machine<State, Trigger>(State.A);

        machine.Configure(State.A)
            .PermitIf(Trigger.X, State.B, GtZero)
            .PermitIf(Trigger.X, State.C, LtZero);

        machine.Configure(State.B)
            .Permit(Trigger.X, State.A);
        
        Assert.Equal(State.A, machine.State());
        
        machine.Fire(Trigger.X, new TestParams() { ParamA = 3});
        
        Assert.Equal(State.B, machine.State());
        
        machine.Fire(Trigger.X);
        
        Assert.Equal(State.A, machine.State());
        
        machine.Fire(Trigger.X, new TestParams() { ParamA = -3});
        
        Assert.Equal(State.C, machine.State());
    }

    [Fact]
    public void IgnoreUnnecessaryParams()
    {
        var machine = new Machine<State, Trigger>(State.A);

        machine.Configure(State.A)
            .Permit(Trigger.X, State.B);
        
        Assert.Equal(State.A, machine.State());
        
        machine.Fire(Trigger.X, new TestParams() { ParamA = 3});
        
        Assert.Equal(State.B, machine.State());
    }
}