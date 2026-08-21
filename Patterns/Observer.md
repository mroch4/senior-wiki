# Observer Pattern (Behavioral)

> Change behavior when internal state changes

It defines a **one-to-many relationship** so that when one object changes, all interested objects are notified automatically.

## Simple .NET example

Imagine an order status changing. Multiple things need to react:

```
                 ┌── EmailService
                 │
Order ─ notify ──┼── SmsService
                 │
                 └── AuditService
```

## 1. Observer interface

```csharp
public interface IOrderObserver
{
    void OnOrderStatusChanged(string status);
}
```

## 2. Observers

```csharp
public class EmailService : IOrderObserver
{
    public void OnOrderStatusChanged(string status)
    {
        Console.WriteLine($"Email: Order is now {status}");
    }
}
```

```csharp
public class AuditService : IOrderObserver
{
    public void OnOrderStatusChanged(string status)
    {
        Console.WriteLine($"Audit: Status changed to {status}");
    }
}
```

## 3. Subject

The `Order` maintains a list of observers:

```csharp
public class Order
{
    private readonly List<IOrderObserver> _observers = [];

    public void Subscribe(IOrderObserver observer)
    {
        _observers.Add(observer);
    }

    public void Unsubscribe(IOrderObserver observer)
    {
        _observers.Remove(observer);
    }

    public void ChangeStatus(string status)
    {
        Console.WriteLine($"Order status: {status}");

        foreach (var observer in _observers)
        {
            observer.OnOrderStatusChanged(status);
        }
    }
}
```

## 4. Usage

```csharp
var order = new Order();

order.Subscribe(new EmailService());
order.Subscribe(new AuditService());

order.ChangeStatus("Shipped");
```

Output:

```text
Order status: Shipped
Email: Order is now Shipped
Audit: Status changed to Shipped
```

The `Order` doesn't need to know **what** the observers do.

### Very common .NET example: events

C# events are closely related to the **Observer pattern**:

```csharp
public class Order
{
    public event EventHandler? StatusChanged;

    public void ChangeStatus()
    {
        // Change state

        StatusChanged?.Invoke(this, EventArgs.Empty);
    }
}
```

Subscribers:

```csharp
order.StatusChanged += (sender, args) => Console.WriteLine("Send email");
order.StatusChanged += (sender, args) => Console.WriteLine("Write audit log");
```

So in .NET, **events/delegates are a very common implementation of the Observer concept**.

## Observer vs Mediator

### Observer

> "Something changed — notify everyone interested."

```
Subject
 ↓
Observer 1
Observer 2
Observer 3
```

### Mediator

> "I need to communicate with another component — let the mediator coordinate it."

```
Component A
 ↓
Mediator
 ↓
Component B
```
