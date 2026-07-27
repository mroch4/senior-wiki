# Dependency Injection

## Table of Content

1. [What is Dependency Injection?](#what-is-dependency-injection)
   - [Without DI](#without-di)
   - [With DI](#with-di)
   - [Why use DI?](#why-use-di)
2. [Dependency Injection vs Dependency Inversion](#dependency-injection-vs-dependency-inversion)
3. [Ways to inject](#ways-to-inject)
   - [Constructor Injection (preferred)](#constructor-injection-preferred)
   - [Property Injection](#property-injection)
   - [Method Injection](#method-injection)
4. [Registering services](#registering-services)
5. [Service Lifetimes](#service-lifetimes)
   - [Singleton](#singleton)
   - [Scoped](#scoped)
   - [Transient](#transient)
6. [Visual comparison](#visual-comparison)
   - [Singleton](#singleton-1)
   - [Scoped](#scoped-1)
   - [Transient](#transient-1)
7. [Why DbContext is Scoped](#why-dbcontext-is-scoped)
   - [Problems](#problems)
     - [Concurrency issues](#concurrency-issues)
     - [Stale data](#stale-data)
     - [Change tracker grows indefinitely](#change-tracker-grows-indefinitely)
     - [Transaction boundaries become unclear](#transaction-boundaries-become-unclear)
     - [Resource management](#resource-management)
   - [Solution](#solution)
   - [Lifetime comparison](#lifetime-comparison)
8. [Injecting into Controllers](#injecting-into-controllers)
9. [Injecting into Minimal APIs](#injecting-into-minimal-apis)
10. [Multiple implementations](#multiple-implementations)
11. [Keyed Services (.NET 8)](#keyed-services-net-8)
12. [Open Generic Registration](#open-generic-registration)
13. [Conditional Resolution](#conditional-resolution)
14. [Options Pattern](#options-pattern)
15. [What is the Service Provider?](#what-is-the-service-provider)
16. [Decorator Pattern](#decorator-pattern)
17. [Interview Tips](#interview-tips)
    - [Why inject interfaces instead of concrete classes?](#why-inject-interfaces-instead-of-concrete-classes)
    - [Can Singleton depend on Scoped?](#can-singleton-depend-on-scoped)
    - [Can Scoped depend on Singleton?](#can-scoped-depend-on-singleton)
    - [Can Scoped depend on Transient?](#can-scoped-depend-on-transient)
    - [Can Singleton depend on Transient?](#can-singleton-depend-on-transient)
    - [Senior interview takeaway](#senior-interview-takeaway)

## What is Dependency Injection?

> Dependency Injection is a **design pattern** where an object receives the objects it depends on from an external source **instead** of creating them itself.

### Without DI:

```csharp
public class OrderService
{
    private readonly EmailService _emailService = new EmailService();

    public void PlaceOrder()
    {
        // business logic

        _emailService.Send();
    }
}
```

❌ Problems:

- Tight coupling
- Difficult to test
- Cannot easily replace EmailService
- Violates Dependency Inversion Principle (SOLID)

### With DI:

```csharp
public class OrderService
{
    private readonly IEmailService _emailService;

    public OrderService(IEmailService emailService)
    {
        _emailService = emailService;
    }

    public void PlaceOrder()
    {
        // business logic

        _emailService.Send();
    }
}
```

Now OrderService doesn't know **how** EmailService is created. Someone else provides it. That "someone else" is the DI Container.

### Why use DI?

Imagine email provider changes `SMTP` => `SendGrid`. Without DI modify **every** place creating EmailService. With DI:

```csharp
builder.Services.AddScoped<IEmailService, SendGridEmailService>();
```

## Dependency Injection vs Dependency Inversion

Dependency Inversion Principle (SOLID):

> High-level modules should depend on **abstractions**, not concrete **implementations**.

Dependency Injection:

> A technique used to satisfy that principle.

```
DIP = Rule

DI = Implementation
```

## Ways to inject

### Constructor Injection (preferred)

```csharp
public class ProductService
{
    private readonly IRepository _repository;

    public ProductService(IRepository repository)
    {
        _repository = repository;
    }
}
```

✅ Advantages:

- immutable dependencies
- impossible to forget
- easy unit testing
- recommended by Microsoft

### Property Injection

```csharp
public class ProductService
{
    public IRepository Repository { get; set; }
}
```

❌ Not recommended (repository may be null)

### Method Injection

```csharp
public void Process(IRepository repository)
{
}
```

Useful when dependency is needed only for one method.

## Registering services

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IRepository, Repository>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddSingleton<ICacheService, CacheService>();
builder.Services.AddTransient<IEmailService, EmailService>();
```

The DI container stores these registrations.

## Service Lifetimes

### Singleton

One instance for the entire application.

```
Application starts
 |
Create once
 |
Everyone uses same object
 |
Application stops
```

```csharp
builder.Services.AddSingleton<ICache, MemoryCache>();
```

✅ Good for:

- configuration
- caching
- logging
- stateless helpers

**Never** store request-specific state.

### Scoped

One instance per HTTP request.

```
Request A => Repository #1 => DbContext #1
Request B => Repository #2 => DbContext #2
```

```csharp
builder.Services.AddScoped<IRepository, Repository>();
```

✅ Used for:

- EF DbContext
- repositories
- business services

Default choice for most application services.

### Transient

New instance **every resolution**.

```csharp
builder.Services.AddTransient<IFormatter, Formatter>();
```

Every injection gets a new object.

✅ Good for:

- lightweight services
- stateless helpers

❌ Avoid for expensive objects.

## Visual comparison

Suppose three controllers request Repository.

### Singleton

```
Controller A => Repository #1
Controller B => Repository #1
Controller C => Repository #1
```

### Scoped

```
Request A => Controller => Repository #1
Request B => Controller => Repository #2
```

### Transient

```
Controller A => Repository #1
Controller B => Repository #2
Controller C => Repository #3
```

## Why DbContext is Scoped

> You should never register DbContext as a singleton in an ASP.NET Core application because DbContext is not thread-safe and is designed to represent a **single unit of work** (typically one HTTP request).

### Problems

#### Concurrency issues

A singleton is shared by all requests and users. If two requests use the same `DbContext` instance simultaneously:

```
Request A => updates Customer
Request B => updates Order
```

You can get exceptions such as:

> "A second operation was started on this context instance before a previous operation completed."

#### Stale data

A singleton may keep tracked entities in memory.

Request 1:

```csharp
var product = context.Products.Find(1);
```

Database: `Price = 100`

Another application updates the price: `Price = 150`

Request 2:

```csharp
context.Products.Find(1);
```

EF Core may return the already tracked entity (price 100) instead of querying the database, resulting in stale data.

#### Change tracker grows indefinitely

`DbContext` tracks every entity it loads. With a singleton:

```csharp
var p1 = context.Products.Find(1);
var p2 = context.Products.Find(2);
var p3 = context.Products.Find(3);
```

The change tracker never resets, leading to:

- Increasing memory usage
- Slower change detection
- Poor performance

A scoped `DbContext` is disposed after each request, clearing the tracker.

#### Transaction boundaries become unclear

A `DbContext` represents single unit of work (typically one HTTP request):

```text
Load data
Modify
SaveChanges()
Dispose
```

With a singleton, changes from unrelated requests can become mixed, making transactions difficult to reason about and increasing the risk of bugs.

#### Resource management

A `DbContext` holds resources such as:

- Database connections (when needed)
- Change tracking state
- Metadata caches
- Internal services

A singleton holds onto its state for the application's lifetime instead of releasing it after each request.

### Solution

Hence In ASP.NET Core, register `DbContext` as **Scoped** (the default):

```csharp
builder.Services.AddDbContext<AppDbContext>();
```

This creates:

- **One `DbContext` per HTTP request**
- Safe usage across concurrent requests
- Automatic disposal at the end of the request

### Lifetime comparison

| Lifetime  | Suitable for `DbContext`? | Why                                                                                                                     |
| --------- | ------------------------- | ----------------------------------------------------------------------------------------------------------------------- |
| Singleton | ❌ No                     | Shared across threads, not thread-safe, stale tracked entities                                                          |
| Scoped    | ✅ Yes                    | One instance per request; matches unit-of-work pattern                                                                  |
| Transient | ⚠️ Sometimes              | Creates a new instance every injection; can make it difficult to coordinate changes across services in the same request |

For most ASP.NET Core applications, **Scoped** is the recommended and default lifetime because it aligns with EF Core's intended usage model.

## Injecting into Controllers

```csharp
public class ProductsController : ControllerBase
{
    private readonly IProductService _service;

    public ProductsController(IProductService service)
    {
        _service = service;
    }
}
```

The framework resolves `IProductService` from the container automatically.

## Injecting into Minimal APIs

```csharp
app.MapGet("/products", (IProductService service) =>
{
    return service.GetProducts();
});
```

The framework resolves `IProductService` from the container automatically.

## Multiple implementations

```csharp
public interface IPaymentService
{
    void Pay();
}

public class StripePaymentService : IPaymentService
{
}

public class PaypalPaymentService : IPaymentService
{
}
```

Register both:

```csharp
builder.Services.AddScoped<IPaymentService, StripePaymentService>();
builder.Services.AddScoped<IPaymentService, PaypalPaymentService>();
```

Inject all implementations:

```csharp
public class CheckoutService
{
    public CheckoutService(IEnumerable<IPaymentService> services)
    {
    }
}
```

Useful for strategy pattern. Or, in .NET 8+, use **keyed services** to resolve a specific implementation by key when appropriate.

## Keyed Services (.NET 8)

Instead of scanning `IEnumerable`.

```csharp
builder.Services.AddKeyedScoped<IPaymentService, StripePayment>(nameof(StripePayment));

builder.Services.AddKeyedScoped<IPaymentService, PaypalPayment>(nameof(PaypalPayment));
```

Resolve

```csharp
public Checkout([FromKeyedServices(nameof(StripePayment))] IPaymentService payment)
{
}
```

## Open Generic Registration

Useful for repositories.

```csharp
public interface IRepository<T>
{
}

public class Repository<T> : IRepository<T>
{
}
```

Register once amd all the following are resolved automatically:

```csharp
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
```

```
IRepository<Customer>
IRepository<Order>
IRepository<Product>
```

## Conditional Resolution

```csharp
if(builder.Environment.IsDevelopment())
{
    builder.Services.AddScoped<IEmailService, FakeEmail>();
}
else
{
    builder.Services.AddScoped<IEmailService, SendGridEmail>();
}
```

## Options Pattern

Don't inject IConfiguration everywhere. For:

```json
{
  "Email": {
    "Host": "smtp.test.com",
    "Port": 25
  }
}
```

Create:

```csharp
public class EmailOptions
{
    public string Host { get; set; }

    public int Port { get; set; }
}
```

Register:

```csharp
builder.Services.Configure<EmailOptions>(builder.Configuration.GetSection("Email"));
```

Inject:

```csharp
public EmailService(IOptions<EmailOptions> options)
{
}
```

Even better, `IOptionsSnapshot` or `IOptionsMonitor` for changing configuration.

| Interface             | Lifetime  | Reload support                    |
| --------------------- | --------- | --------------------------------- |
| `IOptions<T>`         | Singleton | ❌ No                             |
| `IOptionsSnapshot<T>` | Scoped    | ✅ Per request                    |
| `IOptionsMonitor<T>`  | Singleton | ✅ Immediate change notifications |

## What is the Service Provider?

It is the object that creates and manages registered services.

```
builder.Services
 |
Service Collection
 |
Build()
 |
ServiceProvider
 |
Resolve dependencies
```

Generally, you should avoid calling `GetService()` throughout your application ("service locator" pattern). Instead, let ASP.NET Core inject dependencies via constructors or Minimal API parameters.

## Decorator Pattern

Suppose `OrderService` needs logging.

Instead of

```
Controller
 |
OrderService
 |
Logger
```

decorate it:

```
Controller
 |
LoggingOrderService
 |
OrderService
```

```csharp
public class LoggingOrderService : IOrderService
{
    private readonly IOrderService _inner;
    private readonly ILogger<LoggingOrderService> _logger;

    public LoggingOrderService(IOrderService inner, ILogger<LoggingOrderService> logger)
    {
        _inner = inner;
        _logger = logger;
    }

    public async Task Process()
    {
        _logger.LogInformation("Start");

        await _inner.Process();

        _logger.LogInformation("End");
    }
}
```

Very common for:

- logging
- caching
- retry
- metrics

Similar conceptually to MediatR pipeline behaviors.

# Interview Tips

## Why inject interfaces instead of concrete classes?

- Loose coupling
- Easier testing (mocks/fakes)
- Swap implementations without changing consumers
- Follows Dependency Inversion Principle

## Can Singleton depend on Scoped?

No. The scoped service would effectively live as long as the singleton, leading to incorrect behavior. ASP.NET Core will typically throw:

> Cannot consume scoped service from singleton.

If a singleton needs scoped services occasionally (for example, in a background service), inject `IServiceScopeFactory` and create a scope when needed.

## Can Scoped depend on Singleton?

Yes. Perfectly safe.

## Can Scoped depend on Transient?

Yes.

## Can Singleton depend on Transient?

Technically yes, but be careful. If the transient is injected into the singleton's constructor, it is created only once and effectively behaves like a singleton within that object. If you truly need a new instance each time, inject a factory or create a scope as appropriate.

## Senior interview takeaway

A strong answer should cover more than just "DI injects dependencies." Mention that it:

- Promotes **loose coupling** and follows the **Dependency Inversion** Principle.
- **Improves testability** by allowing dependencies to be mocked or substituted.
- **Centralizes** object creation and lifetime management.
- Supports **constructor injection** as the preferred pattern.
- Uses `Transient`, `Scoped`, and `Singleton` lifetimes appropriately.
- Integrates naturally with ASP.NET Core controllers, Minimal APIs, background services, and middleware.
- Helps build maintainable, modular, and extensible applications.

These points demonstrate both conceptual understanding and practical experience with DI in modern ASP.NET Core.
