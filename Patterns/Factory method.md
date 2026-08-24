# Factory Method Pattern (Creational)

> Define a method for creating an object, while allowing subclasses to decide which concrete object gets created.

## Simple .NET example

Imagine a notification system.

### Product

```csharp
public interface INotification
{
    void Send(string message);
}
```

Concrete products:

```csharp
public class EmailNotification : INotification
{
    public void Send(string message)
    {
        Console.WriteLine($"Email: {message}");
    }
}
```

```csharp
public class SmsNotification : INotification
{
    public void Send(string message)
    {
        Console.WriteLine($"SMS: {message}");
    }
}
```

### Creator

The base class defines the **factory method**:

```csharp
public abstract class NotificationService
{
    protected abstract INotification CreateNotification();

    public void Notify(string message)
    {
        var notification = CreateNotification();

        notification.Send(message);
    }
}
```

### Concrete creators

```csharp
public class EmailNotificationService : NotificationService
{
    protected override INotification CreateNotification()
    {
        return new EmailNotification();
    }
}
```

```csharp
public class SmsNotificationService : NotificationService
{
    protected override INotification CreateNotification()
    {
        return new SmsNotification();
    }
}
```

## Usage

```csharp
NotificationService service = new EmailNotificationService();

service.Notify("Order shipped");
```

The flow is:

```
NotificationService
 │ CreateNotification()
 ↓
EmailNotificationService
 │
 ↓
EmailNotification
```

The base class knows **that it needs an `INotification`**, but doesn't know which concrete implementation will be created.

## Why is this useful?

Without Factory Method:

```csharp
if (type == "email")
    return new EmailNotification();

if (type == "sms")
    return new SmsNotification();
```

The creation logic can become scattered throughout your application.

Factory Method **moves the creation decision into specialized creators**.

## Factory Method vs Simple Factory

This is important because people often call both "Factory."

**Simple Factory**:

```csharp
public INotification Create(string type)
{
    return type switch
    {
        "email" => new EmailNotification(),
        "sms" => new SmsNotification(),
        _ => throw new Exception()
    };
}
```

One class decides what to create.

**Factory Method**:

```
Base Creator
 ↓
Concrete Creator → decides what to create
```

The creation method is typically **overridden by subclasses**.
