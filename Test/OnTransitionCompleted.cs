namespace Test;

using Wasp;
using Xunit;

public class OnTransitionCompleted
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
        
        machine.OnTransitioned(OnTransitioned);
        machine.OnTransitionCompleted(OnCompleted);
        
        machine.Fire(Trigger.X);
        
        void OnTransitioned(TriggerParams? triggerParams)
        {
            Assert.True(machine.IsInState(State.A));
            Assert.False(b1);
            Assert.True(b2);
            Assert.False(b3);
        }

        void OnCompleted(TriggerParams? triggerParams)
        {
            Assert.True(machine.IsInState(State.B));
            Assert.True(b1);
            Assert.False(b2);
            Assert.True(b3);
        }
    }
    
}