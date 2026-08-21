# Chain of Responsibility Pattern (Behavioral)

> Pass a request through a chain of handlers until one handles it or the chain ends.

ASP.NET Core middleware is a practical example of this concept.

## Simple .NET example

Imagine an expense approval system:

```csharp
public abstract class ExpenseApprover
{
    protected ExpenseApprover? Next { get; private set; }

    public void SetNext(ExpenseApprover next)
    {
        Next = next;
    }

    public abstract void Approve(decimal amount);
}
```

Different handlers:

```csharp
public class Manager : ExpenseApprover
{
    public override void Approve(decimal amount)
    {
        if (amount <= 1_000)
            Console.WriteLine("Manager approved");
        else
            Next?.Approve(amount);
    }
}
```

```csharp
public class Director : ExpenseApprover
{
    public override void Approve(decimal amount)
    {
        if (amount <= 10_000)
            Console.WriteLine("Director approved");
        else
            Next?.Approve(amount);
    }
}
```

```csharp
public class CEO : ExpenseApprover
{
    public override void Approve(decimal amount)
    {
        if (amount <= 100_000)
            Console.WriteLine("CEO approved");
        else
            Console.WriteLine("Expense rejected");
    }
}
```

Build the chain:

```csharp
var manager = new Manager();
var director = new Director();
var ceo = new CEO();

manager.SetNext(director);
director.SetNext(ceo);

manager.Approve(5_000);
```

Flow:

```
Request: $5,000
 ↓
Manager
 ↓
Director ← handles request
 ↓
CEO
```

The manager doesn't need to know **how** the director or CEO handles the request.

## Very important .NET example: Middleware

ASP.NET Core middleware is conceptually very similar to **Chain of Responsibility**:

```
HTTP Request
 ↓
Exception Middleware
 ↓
Authentication Middleware
 ↓
Authorization Middleware
 ↓
Logging Middleware
 ↓
Controller
```

Each middleware can:

1. Handle the request itself
2. Do something **before** calling the next middleware
3. Call the next middleware
4. Do something **after** it returns
5. Short-circuit the chain

For example:

```csharp
public async Task InvokeAsync(HttpContext context)
{
    // Before
    Console.WriteLine("Before");

    await _next(context);

    // After
    Console.WriteLine("After");
}
```
