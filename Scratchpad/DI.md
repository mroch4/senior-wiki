## IServiceProvider

The DI container is represented by `IServiceProvider`.

Normally you don't use it directly:

```csharp
public class OrderService
{
    private readonly IRepository _repository;

    public OrderService(IRepository repository)
    {
        _repository = repository;
    }
}
```

The framework does:

```
ServiceProvider

|

Find IRepository

|

Create Repository

|

Pass into constructor
```

### You _can_ resolve manually

```csharp
var repo = serviceProvider.GetRequiredService<IRepository>();
```

But this should be rare.

Why?

Because this becomes the **Service Locator anti-pattern**.

Instead of declaring dependencies explicitly, classes secretly fetch them.

Bad:

```csharp
public class OrderService
{
    private readonly IServiceProvider _provider;

    public OrderService(IServiceProvider provider)
    {
        _provider = provider;
    }

    public void Save()
    {
        var repo = _provider.GetRequiredService<IRepository>();
    }
}
```

Nobody knows OrderService actually depends on IRepository.

# 2. IServiceScopeFactory

One of the favourite interview questions.

Suppose you have

```text
BackgroundService (Singleton)
```

but need

```text
DbContext (Scoped)
```

You cannot inject DbContext directly.

Instead:

```csharp
public class Worker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public Worker(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            using var scope = _scopeFactory.CreateScope();

            var db = scope.ServiceProvider
                          .GetRequiredService<AppDbContext>();

            await db.SaveChangesAsync();

            await Task.Delay(5000);
        }
    }
}
```

Each loop creates

```
New Scope

|

New DbContext

|

Dispose
```

Exactly like an HTTP request.

# 3. Factory Registration

Sometimes dependencies require runtime values.

Instead of

```csharp
builder.Services.AddSingleton<MyService>();
```

Use

```csharp
builder.Services.AddSingleton(sp =>
{
    var logger = sp.GetRequiredService<ILogger<MyService>>();

    return new MyService(logger);
});
```

Useful when:

- reading configuration
- selecting implementations
- constructing third-party objects

# 11. Disposable Services

If a service implements

```csharp
IDisposable
```

the container disposes it automatically when its lifetime ends.

For example

```
Scoped

|

DbContext

|

Disposed at end of request
```

Never manually dispose injected services.

# 12. ValidateScopes

During development

```csharp
builder.Host.UseDefaultServiceProvider(options =>
{
    options.ValidateScopes = true;
});
```

Now

```
Singleton

|

Scoped
```

throws immediately.

Excellent debugging aid.

# 13. Middleware and DI

Middleware constructors should only take **singleton-safe** dependencies because middleware instances are typically created once.

Bad:

```csharp
public MyMiddleware(AppDbContext db)
```

Good:

```csharp
public async Task InvokeAsync(
    HttpContext context,
    AppDbContext db)
{
}
```

The scoped service is injected into `InvokeAsync`, giving you the request's scope.

# 14. Compiled Object Graphs

The built-in container doesn't use reflection every time a service is resolved.

At startup, it analyzes registrations and builds efficient factories (using compiled expressions or generated delegates internally). After that, resolving services is essentially calling cached delegates.

This is why the built-in container is quite fast despite its simplicity.

# Typical Senior Interview Questions

1. **Why is `DbContext` registered as Scoped instead of Singleton?**
2. **How do you use a Scoped service inside a `BackgroundService`?**
3. **Why is `IServiceProvider` considered a Service Locator when overused?**
4. **When would you use `IOptionsMonitor` instead of `IOptionsSnapshot`?**
5. **How do you register and resolve multiple implementations of an interface?**
6. **What happens if a Singleton depends on a Scoped service?**
7. **How does the DI container know which constructor to use?** (It selects the "best" resolvable public constructor; if multiple are equally suitable, it throws an ambiguity exception.)
8. **What is the difference between a factory registration and a factory pattern?**
9. **When would you use keyed services instead of `IEnumerable<T>`?**
10. **How are scoped services disposed, and who owns their lifetime?**

Mastering these topics usually puts you at the level expected of a senior ASP.NET Core developer, because they focus on the architectural and lifecycle issues that arise in real production systems rather than just basic registration syntax.
