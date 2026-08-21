# Decorator Pattern (Structural)

> Add behavior by wrapping an object without modifying original object's code.

A very common .NET example is adding **logging, caching, validation, or authorization** around a service.

## Simple .NET example

Suppose we have:

```csharp
public interface IOrderService
{
    Task CreateOrderAsync(Order order);
}
```

Basic implementation:

```csharp
public class OrderService : IOrderService
{
    public async Task CreateOrderAsync(Order order)
    {
        // Save order
    }
}
```

Now we want logging.

Instead of changing `OrderService`:

```csharp
public class LoggingOrderService : IOrderService
{
    private readonly IOrderService _inner;

    public LoggingOrderService(IOrderService inner)
    {
        _inner = inner;
    }

    public async Task CreateOrderAsync(Order order)
    {
        Console.WriteLine("Creating order...");

        await _inner.CreateOrderAsync(order);

        Console.WriteLine("Order created.");
    }
}
```

The decorator **wraps** the original service:

```
            IOrderService
                  ↑
       ┌──────────┴──────────┐
 OrderService        LoggingOrderService
                             ↓
                        OrderService
```

You can stack decorators:

```
Caching
 ↓
Logging
 ↓
Authorization
 ↓
OrderService
```

Each decorator adds behavior while the underlying `OrderService` remains unchanged.

## .NET DI example

ASP.NET Core's DI container can be used to register the components, although built-in DI doesn't natively provide Scrutor-style decoration APIs. A library such as **Scrutor** makes this convenient:

```csharp
services.AddScoped<IOrderService, OrderService>();

services.Decorate<IOrderService, LoggingOrderService>();
```

Now whenever something requests:

```csharp
IOrderService
```

it effectively receives:

```text
LoggingOrderService
        ↓
   OrderService
```

## Decorator vs inheritance

### Inheritance

```
LoggingOrderService : OrderService
```

The important difference is that the decorator **wraps an object implementing the same interface**.

### Decorator

```
LoggingOrderService
 ↓
IOrderService
 ↓
OrderService
```
