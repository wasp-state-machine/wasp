namespace Test;

using Wasp;
using Xunit;

public class PermitIf
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
            .PermitIf(Trigger.X, State.D, GeZero)
            .PermitIf(Trigger.X, State.C, LeZero);

        var testParams = new TestParams() { ParamA = 0 };
        Assert.Throws<InvalidOperationException>(() => { machine2.Fire(Trigger.X, testParams); });
    }

    [Fact]
    public void WithParams()
    {
        var machine = new Machine<State, Trigger>(State.A);

        machine.Configure(State.A)
            .PermitIf(Trigger.X, State.B, GeZero)
            .PermitIf(Trigger.X, State.C, LeZero);

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

    [Fact]
    public void EmptyTransition()
    {
        var machine1 = new Machine<State, Trigger>(State.A);
        
        machine1.Configure(State.A)
            .PermitIf(Trigger.X, State.D, _ => false, 5)
            .PermitIf(Trigger.X, State.C, _ => false, 5);
        
        machine1.Fire(Trigger.Y);
        
        machine1.Fire(Trigger.X);
    }

    [Fact]
    public void AmbiguousWithWeight()
    {
        var machine1 = new Machine<State, Trigger>(State.A);
        
        machine1.Configure(State.A)
            .PermitIf(Trigger.X, State.D, _ => true, 5)
            .PermitIf(Trigger.X, State.C, _ => true, 5);
        
        Assert.Throws<InvalidOperationException>(() => { machine1.Fire(Trigger.X); });
        
        var machine2 = new Machine<State, Trigger>(State.A);

        machine2.Configure(State.A)
            .PermitIf(Trigger.X, State.D, GeZero, -9)
            .PermitIf(Trigger.X, State.C, GeZero, -9);

        var testParams = new TestParams() { ParamA = 3 };
        
        Assert.Throws<InvalidOperationException>(() => { machine2.Fire(Trigger.X, testParams); });
    }
    
    
    [Fact]
    public void BreakAmbiguityWithWeight()
    {
        var machine1 = new Machine<State, Trigger>(State.A);

        machine1.Configure(State.A)
            .PermitIf(Trigger.X, State.B, _ => true, 0)
            .PermitIf(Trigger.X, State.C, _ => true, 5);

        machine1.Fire(Trigger.X);
        
        Assert.Equal(State.C, machine1.State());
        
        var machine2 = new Machine<State, Trigger>(State.A);

        machine2.Configure(State.A)
            .PermitIf(Trigger.X, State.C, GeZero, -15)
            .PermitIf(Trigger.X, State.D, GeZero, -9);

        var testParams = new TestParams() { ParamA = 3 };
        
        machine2.Fire(Trigger.X, testParams);
        
        Assert.Equal(State.D, machine2.State());
    }
    
    [Fact]
    public void UnambiguousWithEqualWeight()
    {
        var machine1 = new Machine<State, Trigger>(State.A);

        machine1.Configure(State.A)
            .PermitIf(Trigger.X, State.B, GeZero, 5)
            .PermitIf(Trigger.X, State.C, LeZero, 5);

        var testParams1 = new TestParams() { ParamA = 3 };
        
        machine1.Fire(Trigger.X, testParams1);
        
        Assert.Equal(State.B, machine1.State());
        
        var machine2 = new Machine<State, Trigger>(State.A);

        machine2.Configure(State.A)
            .PermitIf(Trigger.X, State.B, GeZero, -9)
            .PermitIf(Trigger.X, State.C, LeZero, -9);

        var testParams2 = new TestParams() { ParamA = -81 };
        
        machine2.Fire(Trigger.X, testParams2);
        
        Assert.Equal(State.C, machine2.State());
    }
}