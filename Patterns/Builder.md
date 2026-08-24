# Builder Pattern (Creational)

> Build complex objects step by step, separating the construction process from the final object.

## Simple .NET example

Imagine creating a complex `Computer`:

```csharp
public class Computer
{
    public string Cpu { get; set; } = "";
    public int Ram { get; set; }
    public bool HasGpu { get; set; }
    public bool HasWifi { get; set; }
}
```

Without Builder, you might end up with a constructor like:

```csharp
new Computer("Intel i9", 32, true, true);
```

As the object becomes more complex, this gets difficult to read.

## Builder

```csharp
public class ComputerBuilder
{
    private readonly Computer _computer = new();

    public ComputerBuilder WithCpu(string cpu)
    {
        _computer.Cpu = cpu;
        return this;
    }

    public ComputerBuilder WithRam(int ram)
    {
        _computer.Ram = ram;
        return this;
    }

    public ComputerBuilder WithGpu()
    {
        _computer.HasGpu = true;
        return this;
    }

    public ComputerBuilder WithWifi()
    {
        _computer.HasWifi = true;
        return this;
    }

    public Computer Build()
    {
        return _computer;
    }
}
```

Usage:

```csharp
var computer = new ComputerBuilder()
    .WithCpu("Intel i9")
    .WithRam(32)
    .WithGpu()
    .WithWifi()
    .Build();
```

Much easier to understand:

```
ComputerBuilder
 │
 ├── WithCpu()
 ├── WithRam()
 ├── WithGpu()
 ├── WithWifi()
 │
 └── Build()
        ↓
    Computer
```

## When is Builder useful?

Especially when an object has:

- Many properties
- Many optional properties
- Different construction variations
- Complicated validation/construction logic

## Real .NET examples

You see Builder-style APIs all over .NET:

```csharp
var app = WebApplication.CreateBuilder(args);

app.Services.AddControllers();
app.Services.AddAuthentication();
app.Services.AddAuthorization();

var application = app.Build();
```

`WebApplicationBuilder` provides a fluent, step-by-step configuration/building experience.

Another familiar example is:

```csharp
var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        // configuration
    })
    .Build();
```
