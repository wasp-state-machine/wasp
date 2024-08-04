# wasp

[WIP]

Quickly create manageable, flexible state machines. Designed for games, but useful for nearly anything.

```csharp
var machine = new Wasp.Machine<State, Trigger>(State.A);

machine.Configure(State.A)
    .Permit(Trigger.X, State.B);

machine.Fire(Trigger.X);
```

Wasp is heavily inspired by [Stateless](https://github.com/dotnet-state-machine/stateless/tree/dev), and mirrors most of its patterns. Wasp adds a few features, such as

- Mutually-inclusive guard clauses
- State overriding
- Multi-state inheritance
- More generalizable parameterized transitions
- A significantly lighter codebase

Wasp currently does not support

- Asynchronous triggers
- Graph generation