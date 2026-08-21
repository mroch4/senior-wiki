# Mediator Pattern (Behavioral)

> Centralize (encapsulate) communication between multiple objects, so they don't need to communicate with each other directly.

## Without Mediator

```
OrderService ────── PaymentService
     │                     │
InventoryService ── NotificationService
```

Classes become tightly coupled because they know about each other.

## With Mediator

```
OrderService ─────┐
PaymentService ───┤
InventoryService ─┤── Mediator
Notification ─────┘
```

Each component communicates **through the mediator**.

For example, in .NET you might have:

```csharp
public interface IMediator
{
    Task SendAsync(object message);
}
```

Then:

```csharp
await mediator.SendAsync(new OrderPlaced(orderId));
```

The mediator decides which handlers/services should respond.

## Common .NET example

**MediatR** is a popular implementation of the Mediator pattern:

```text
Controller
 ↓
IMediator.Send()
 ↓
Command / Query
 ↓
Handler
 ↓
Domain / Services
```
