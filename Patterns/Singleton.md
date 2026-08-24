# Singleton Pattern (Creational)

> Ensure a class has only one instance and provide a way to access that instance.

## Simple .NET example

```csharp
public sealed class AppConfiguration
{
    private static readonly Lazy<AppConfiguration> _instance = new(() => new AppConfiguration());

    public static AppConfiguration Instance => _instance.Value;

    private AppConfiguration()
    {
    }

    public string Environment { get; set; } = "Production";
}
```

Usage:

```csharp
var config1 = AppConfiguration.Instance;
var config2 = AppConfiguration.Instance;

Console.WriteLine(ReferenceEquals(config1, config2)); // True
```

Because the constructor is private, nobody can do:

```csharp
new AppConfiguration(); // ❌
```

They must use:

```csharp
AppConfiguration.Instance
```

## Why `Lazy<T>`?

This:

```csharp
Lazy<AppConfiguration>
```

gives you **lazy initialization** and handles thread-safe initialization.

The instance isn't created until it's actually needed.

## Singleton in ASP.NET Core

This is especially important for .NET interviews.

You normally **don't implement Singleton manually**. ASP.NET Core's DI container manages it:

```csharp
builder.Services.AddSingleton<IAppConfiguration, AppConfiguration>();
```

Then:

```csharp
public class OrderService
{
    private readonly IAppConfiguration _configuration;

    public OrderService(IAppConfiguration configuration)
    {
        _configuration = configuration;
    }
}
```

The DI container ensures the same instance is reused throughout the application's DI container lifetime.

### Singleton vs Scoped vs Transient

Very important in ASP.NET Core:

| Lifetime      | Instance                                             |
| ------------- | ---------------------------------------------------- |
| **Singleton** | One instance for the application's service container |
| **Scoped**    | One instance per scope — typically one HTTP request  |
| **Transient** | New instance each time requested                     |

```csharp
services.AddSingleton<IMyService, MyService>();
services.AddScoped<IMyService, MyService>();
services.AddTransient<IMyService, MyService>();
```

## Important interview warning

Singleton is **not automatically appropriate for shared state**.

For example, this can be dangerous:

```csharp
public class MySingleton
{
    public List<Order> Orders { get; set; }
}
```

Multiple HTTP requests can access the same instance concurrently, creating **thread-safety and state-management problems**.

Also, don't inject a **scoped service into a singleton** directly:

```
Singleton
 ↓
Scoped ❌
```

because the **scoped dependency can effectively live too long**.
