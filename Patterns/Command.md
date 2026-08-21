# Command Pattern (Behavioral)

> Encapsulate a request/action as an object

Instead of:

```
Controller
 ↓
Service
 ↓
DoSomething()
```

you represent the action itself as a command:

```
Controller
 ↓
CreateOrderCommand
 ↓
CreateOrderCommandHandler
 ↓
OrderService
```

## Simple .NET example

Command:

```csharp
public record CreateOrderCommand(int OrderId, decimal Amount);
```

Handler:

```csharp
public class CreateOrderCommandHandler
{
    public async Task Handle(CreateOrderCommand command)
    {
        Console.WriteLine($"Creating order {command.OrderId} for {command.Amount}");

        await Task.CompletedTask;
    }
}
```

Usage:

```csharp
var command = new CreateOrderCommand(123, 99.99m);

var handler = new CreateOrderCommandHandler();

await handler.Handle(command);
```

## Why is this useful?

The important part is that the **request becomes an object**:

```csharp
var command = new CreateOrderCommand(123, 99.99m);
```

That makes it easier to:

- queue requests
- log requests
- retry requests
- validate requests
- audit requests
- implement undo/redo
- decouple the sender from the receiver

## Real-world .NET example: MediatR

This is where you'll commonly see Command in modern .NET applications.

Command:

```csharp
public record CreateOrderCommand(int OrderId,decimal Amount) : IRequest;
```

Handler:

```csharp
public class CreateOrderCommandHandler: IRequestHandler<CreateOrderCommand>
{
    public async Task Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        // Create order
        await Task.CompletedTask;
    }
}
```

Controller:

```csharp
[HttpPost]
public async Task<IActionResult> Create(CreateOrderCommand command)
{
    await _mediator.Send(command);

    return Ok();
}
```

The controller doesn't need to know **which class actually performs the operation**.

## Command vs Mediator

### Command

> Represents **what you want to do**.

```
CreateOrderCommand
DeleteOrderCommand
UpdateCustomerCommand
```

### Mediator

> Coordinates **who handles the request**.

```
Mediator
 ↓
CreateOrderCommandHandler
```

### Usage example

```
Controller
 ↓
Mediator
 ↓
Command
 ↓
Command Handler
 ↓
Domain / Services
```
