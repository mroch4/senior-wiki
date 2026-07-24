# Records

## Table of Content

1. [What is a record type?](#what-is-a-record-type)
2. [Class vs Record](#class-vs-record)
3. [Example with a class](#example-with-a-class)
4. [Example with a record](#example-with-a-record)
5. [`with` expression](#with-expression)
6. [ToString()](#tostring)
7. [Immutability](#immutability)
8. [Record class vs Record struct](#record-class-vs-record-struct)
9. [Typical use cases](#typical-use-cases)
   - [DTOs](#dtos)
   - [CQRS Commands](#cqrs-commands)
   - [Queries](#queries)
   - [Events](#events)
   - [Configuration models](#configuration-models)
10. [When NOT to use records](#when-not-to-use-records)
11. [Interview Tips](#interview-tips)

## What is a record type?

> `record` is a **reference** type introduced in C# 9 designed for **immutable data models**. While a `record` looks similar to a `class`, it has very different default behavior.

## Class vs Record

| Feature      | Class                            | Record                           |
| ------------ | -------------------------------- | -------------------------------- |
| Equality     | Reference equality (same object) | Value equality (same data)       |
| Default use  | Objects with behavior            | Data transfer/data models        |
| Mutability   | Usually mutable                  | Encourages immutability          |
| `ToString()` | Type name                        | Prints property names and values |
| Copying      | Manual                           | `with` expression                |
| Hash code    | Based on reference               | Based on property values         |

## Example with a class

```csharp
public class Person
{
    public string Name { get; init; }
    public int Age { get; init; }
}

var p1 = new Person { Name = "John", Age = 30 };
var p2 = new Person { Name = "John", Age = 30 };

Console.WriteLine(p1 == p2);      // False
Console.WriteLine(p1.Equals(p2)); // False
```

Even though the data is identical, they're different objects in memory.

## Example with a record

```csharp
public record Person(string Name, int Age);

var p1 = new Person("John", 30);
var p2 = new Person("John", 30);

Console.WriteLine(p1 == p2);      // True
Console.WriteLine(p1.Equals(p2)); // True
```

Records compare **property values**, not **memory addresses**.

## `with` expression

One of the nicest features is easy cloning.

Without records:

```csharp
var p2 = new Person
{
    Name = p1.Name,
    Age = 31
};
```

With records:

```csharp
var p2 = p1 with { Age = 31 };
```

This creates a new object while leaving the original unchanged.

## ToString()

Class:

```csharp
Console.WriteLine(person);

// Namespace.Person
```

Record:

```csharp
Console.WriteLine(person);

// Person { Name = John, Age = 30 }
```

Useful for logging.

## Immutability

Records encourage immutable objects.

```csharp
public record CreateOrderCommand(Guid CustomerId, decimal Amount);
```

You normally don't change commands after creating them.

---

## Record class vs Record struct

A `record` is actually shorthand for a **record class**.

```csharp
public record Person(string Name);
```

is equivalent to

```csharp
public record class Person(string Name);
```

You can also have

```csharp
public record struct Point(int X, int Y);
```

which is a **value** type.

## Typical use cases

### DTOs

```csharp
public record UserDto(Guid Id, string Name, string Email);
```

### CQRS Commands

```csharp
public record CreateUserCommand(string Name, string Email) : IRequest<Guid>;
```

This is why you often see records with MediatR.

### Queries

```csharp
public record GetUserQuery(Guid Id) : IRequest<UserDto>;
```

### Events

```csharp
public record UserCreatedEvent(Guid UserId, DateTime CreatedAt);
```

Events represent **facts** that shouldn't change.

### Configuration models

```csharp
public record JwtOptions
{
    public string Issuer { get; init; }
    public string Audience { get; init; }
}
```

## When NOT to use records

If the object has **identity** and its state changes over time, a class is usually a better fit.

```csharp
public class ShoppingCart
{
    public Guid Id { get; }
    private readonly List<Item> _items = [];

    public void AddItem(Item item)
    {
        _items.Add(item);
    }
}
```

or

```csharp
public class BankAccount
{
    public decimal Balance { get; private set; }

    public void Deposit(decimal amount)
    {
        Balance += amount;
    }
}
```

These objects have **behavior** and **mutable** state, so value-based equality would be misleading.

# Interview Tips

> "A record is a C# reference type optimized for representing immutable data. Unlike a class, records use value-based equality, so two record instances with the same property values are considered equal. They also provide built-in support for cloning with the `with` expression and a useful `ToString()` implementation. I typically use records for DTOs, CQRS commands and queries, API contracts, and domain events. I use classes for entities or services that have identity, behavior, and mutable state."
