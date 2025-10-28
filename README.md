## Wasp

Wasp is an open-source state machine Nuget package designed for use in game controllers.

Wasp is heavily inspired by [Stateless](https://github.com/dotnet-state-machine/stateless/tree/dev), and mirrors most of its patterns. Wasp adds a few features, such as

- Mutually-inclusive guard clauses
- Non-linear state inheritance
- Weakly-typed parameterized transitions
- A significantly lighter codebase

## Overview

Wasp provides a Machine class which represents the State of an entity. Machines are affected by Triggers, which are basically an event that is invoked from somewhere else. States of the Machine can be individually permit the Machine to transition to another State when a particular Trigger is fired.

```csharp
Machine.Configure(State.Idle)
    .Permit(Trigger.InputMoveVector2, State.Walking);

Machine.Configure(State.Falling)
    .Permit(Trigger.InputMoveVector2, State.Gliding);
```

Then, somewhere else, you can fire a Trigger on the Machine without caring what State it's in.

```csharp
void Update() {
    Vector2 moveVector2 = _playerInput.actions["Move"].ReadValue<Vector2>();
    if (moveVector2.magnitude > 0.05f) return;
    Machine.Fire(Trigger.InputMoveVector2);
}
```

In this example, if the Machine is in the Idle State when the Trigger is fired, the Machine would transition into the Walking State. But if the Machine was in the Falling State, the Machine would go into the Gliding State instead. Simple!

Wasp Machines act as a source-of-truth that can track entity state, but how you read that state is entirely up to you. A simple approach is to conditionally run code based on the Machine state using if-statements.

```csharp
void Update() {
    if (Machine.IsInState(State.Walking)) {
        transform.position += transform.forward * Time.deltaTime;
    }
    if (Machine.IsInState(State.Falling)) {
        transform.position -= transform.up * Time.deltaTime;
    }
}
```

Another approach could be using a Dictionary to bind Machine States to delegates that represent behaviors.

```csharp
void WalkingBehavior()
{
    transform.position += transform.forward * Time.deltaTime;
}

void Start()
{
    _behaviors = new Dictionary<TState, Action>();
    _behaviors[State.Walking] = WalkingBehavior;
}

void Update()
{
    if (_behaviors.TryGetValue(Machine.State(), out var behavior))
    {
        behavior();
    }
}
```

## Clauses

In addition to the basic Permit function that allows a State to transition to another given a particular Trigger, the PermitIf function supports the use of conditional transitions. The PermitIf function takes a boolean Func (a "clause") that must evaulate to true before the transition can occur.

```csharp
Machine.Configure(State.Alive)
    .PermitIf(Trigger.TakeDamage, State.KnockedOver, _ => _health > 0);

Machine.Configure(State.Alive)
    .PermitIf(Trigger.TakeDamage, State.Dead, _ => _health <= 0);
```

## Transition Actions

You can embed csharp Actions directly into Wasp Machines to run code when a State is entered or exited.

```csharp
Machine.Configure(State.KnockedOver)
    .OnEntry(_ => { _animator.Play("KnockedOver"); })
    .OnExit(_ => { SoundManager.Play("Grunt_01") });

Machine.Configure(State.Dead)
    .OnEntry(_ => { _animator.Play("Dead"); });
```

## Transition parameters

When firing a Trigger on a Machine, you can optionally pass parameters in the form of a TriggerParams object. TriggerParams is an abstract class that must be implemented within your code to include the data you would like to pass.

```csharp
public class StringParam : Wasp.TriggerParams
{
    public String s;
}

Machine.Configure(State.MyState)
    .OnEntry(param => { 
        // downcast from TriggerParams into your custom class
        if (param is not StringParam stringParam) return; 
        print(stringParam.s);
    });

Machine.Fire(Trigger.MyTrigger, new StringParam() { s = "Hello World" } );
```

## State inheritance

In Wasp, States can be declared as Substates of other States. Doing so will cause the Substate to inherit all of the configurations of the superstate.

```csharp
Machine.Configure(State.Grounded)
    .Permit(Trigger.GroundCheckFailed, State.Falling);

Machine.Configure(State.Idle)
    .SubstateOf(State.Grounded);

Machine.Configure(State.Walking)
    .SubstateOf(State.Grounded);
```

In this example, the Idle and Walking States have both been configured as Substates of the Grounded State. This means when the Machine is in the Idle State or in the Walking State, it's also effectively in the Grounded State. Both of these Substates will respond to the GroundCheckFailed Trigger because they inherit the permitted transition from their Grounded superstate.

This pattern allows you to write shared configurations a single time and then share that configuration across any State you need.

### Nested and non-linear inheritance

One of Wasp's most powerful features is that it enables entirely abstract inheritance patterns.

```csharp
Machine.Configure(State.Moving);
Machine.Configure(State.Grounded);
Machine.Configure(State.Airborne);

Machine.Configure(State.Idle)
    .SubstateOf(State.Grounded);

Machine.Configure(State.Walking)
    .SubstateOf(State.Grounded)
    .SubstateOf(State.Moving);

Machine.Configure(State.Gliding)
    .SubstateOf(State.Airborne)
    .SubstateOf(State.Moving);
```

In this more complex example, we have 3 superstates: Grounded, Airborne, and Moving. Notice how the Walking State inherits Grounded and Moving, while the Gliding State inherits Airborne and Moving. Crucially, Grounded, Airborne, and Moving do not inherit from eachother. You can see here that Substates are free to mix-and-match superstates however they see fit. In this way, it can be helpful to think of Wasp States as based on a composition pattern rather than inheritance.

Substates also chain inheritance if their parent superstates happen to also be Substates.

```csharp
Machine.Configure(State.Actionable)
    .Permit(Trigger.AttackInput, State.Attacking);

Machine.Configure(State.Grounded)
    .SubstateOf(State.Actionable);

Machine.Configure(State.Idle)
    .SubstateOf(State.Grounded);
```

In this example, if the Machine is in the Idle State when the AttackInput is fired, the Machine will transition to the Attacking State. The Idle State inherits the Grounded State, which in turn inherits the Actionable State, which permits the transition.

### State() vs. IsInState(TState state)

Two functions exist to read the current State of a Machine. The most basic is State() which returns the current State of the Machine. The other is IsInState(TState state), which returns a boolean representing whether the Machine is in the provided State. IsInState respects inheritance.

```csharp
Machine.Configure(State.Actionable)
    .Permit(Trigger.AttackInput, State.Attacking);

Machine.Configure(State.Airborne)
    .SubstateOf(State.Actionable);

Machine.Configure(State.Grounded)
    .SubstateOf(State.Actionable);

Machine.Configure(State.Idle)
    .SubstateOf(State.Grounded);

Machine.Assume(State.Idle); // sets the Machine to the Idle State

print(Machine.State()); // prints "Idle"
print(Machine.IsInState(State.Idle)); // prints "true"
print(Machine.IsInState(State.Grounded)); // prints "true"
print(Machine.IsInState(State.Airborne)); // prints "false"
print(Machine.IsInState(State.Actionable)); // prints "true"
```

### Inherited Actions

When a State is entered or exited, it invokes not only its own Actions, but those of its superstates as well.

```csharp
Machine.Configure(State.Actionable)
    .OnEntry(_ => { print("Actionable entered"); });

Machine.Configure(State.Grounded)
    .SubstateOf(State.Actionable)
    .OnEntry(_ => { print("Grounded entered"); });

Machine.Configure(State.Idle)
    .SubstateOf(State.Grounded)
    .OnEntry(_ => { print("Idle entered"); });
```

Given this example, when the Machine enters the Idle State, all three print statements will be executed.
