# KISS

## Table of Content

1. [What is KISS?](#what-is-kiss)
   - [Why it matters](#why-it-matters)
2. [Example 1](#example-1)
   - [Not KISS](#not-kiss)
   - [KISS](#kiss)
3. [Example 2 - complex LINQ](#example-2---complex-linq)
4. [Example 3 - In architecture](#example-3---in-architecture)
   - [Don't start with](#dont-start-with)
   - [Start with](#start-with)
5. [KISS vs YAGNI vs DRY](#kiss-vs-yagni-vs-dry)
6. [Interview Tips](#interview-tips)

## What is KISS?

> **KISS** stands for **Keep It Simple, Stupid** (sometimes phrased more politely as **Keep It Simple and Straightforward**).

It's a software design principle that says:

> **Choose the simplest solution that correctly solves the problem. Avoid unnecessary complexity.**

### Why it matters

Simple code is:

- ✅ Easier to read
- ✅ Easier to maintain
- ✅ Easier to test
- ✅ Less likely to contain bugs
- ✅ Easier for other developers to understand

## Example 1

### Not KISS

```csharp
public interface IDiscountStrategy
{
    decimal Calculate(decimal price);
}

public class NoDiscountStrategy : IDiscountStrategy
{
    public decimal Calculate(decimal price) => price;
}

public class PriceCalculator
{
    private readonly IDiscountStrategy _strategy;

    public PriceCalculator(IDiscountStrategy strategy)
    {
        _strategy = strategy;
    }

    public decimal GetPrice(decimal price)
        => _strategy.Calculate(price);
}
```

If your application only ever has one pricing rule, introducing interfaces and strategy classes is unnecessary.

### KISS

```csharp
public decimal CalculatePrice(decimal price)
{
    return price;
}
```

Only introduce the Strategy pattern when you actually need multiple pricing algorithms.

## Example 2 - complex LINQ

```csharp
var names = users
    .Where(u => u.IsActive)
    .OrderBy(u => u.LastName)
    .Select(u => u.FirstName)
    .ToList();
```

This is already readable.

But someone writes:

```csharp
var query =
    users
        .Where(x => x.IsActive == true)
        .Select(x => new
        {
            Name = x.FirstName,
            Last = x.LastName
        })
        .OrderBy(x => x.Last)
        .Select(x => x.Name)
        .ToList();
```

The second version does the same thing but is harder to follow.

## Example 3 - In architecture

Suppose you have:

- one API
- one SQL database
- 500 users

### Don't start with

- Microservices
- Kafka
- CQRS
- Event Sourcing
- Kubernetes
- Saga Pattern

### Start with

- ASP.NET Core Web API
- EF Core
- SQL Server
- Clean Architecture (if appropriate)

Add complexity only when there is a real need.

## KISS vs YAGNI vs DRY

| Principle                            | Meaning                                                 |
| ------------------------------------ | ------------------------------------------------------- |
| **KISS**                             | Keep the solution as simple as possible.                |
| **YAGNI** (You Aren't Gonna Need It) | Don't implement features until they're actually needed. |
| **DRY** (Don't Repeat Yourself)      | Avoid duplicating knowledge or logic.                   |

These principles complement each other:

- **KISS** avoids unnecessary complexity.
- **YAGNI** avoids unnecessary features.
- **DRY** avoids unnecessary duplication.

# Interview Tips

> "KISS stands for _Keep It Simple, Stupid_. The principle encourages choosing the simplest design that satisfies the requirements. Simple solutions are easier to understand, test, debug, and maintain. In .NET, this means avoiding unnecessary abstractions, design patterns, or distributed architectures until there's a clear business or technical need for them."

For senior .NET interviews, it's also useful to know that **KISS is about avoiding accidental complexity, not necessary complexity**. If a problem genuinely requires a more sophisticated design (e.g., CQRS for independent read/write scaling or Saga for distributed transactions), then that complexity is justified. The goal is to make the solution **as simple as possible, but no simpler**.
