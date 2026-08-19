# `init`

## Table of content

1. [What is init?](#what-is-init)
2. [Comparison](#comparison)
   - [`get; set;` (Mutable)](#get-set-mutable)
   - [`get; init;` (Immutable after creation)](#get-init-immutable-after-creation)
3. [Why is it useful?](#why-is-it-useful)
4. [Records and `init`](#records-and-init)
5. [How is it different from `private set`?](#how-is-it-different-from-private-set)
6. [Interview Tips](#interview-tips)
7. [Rule of thumb](#rule-of-thumb)

## What is init?

`get; init;` is a C# feature (introduced in C# 9) that creates an **init-only property**. It means the property can be assigned **only during object initialization**, after which it becomes immutable.

## Comparison

### `get; set;` (Mutable)

```csharp
public class Person
{
    public string Name { get; set; }
}

var person = new Person();
person.Name = "John";   // ✅
person.Name = "Mike";   // ✅ Can change anytime
```

The property can be modified at any point.

### `get; init;` (Immutable after creation)

```csharp
public class Person
{
    public string Name { get; init; }
}

var person = new Person
{
    Name = "John"
};

person.Name = "Mike";   // ❌ Compile-time error
```

The property can only be assigned:

- in an object initializer,
- in a constructor,
- or within a `with` expression for records.

## Why is it useful?

Imagine a message:

```csharp
public record OrderCreated
{
    public Guid OrderId { get; init; }
    public Guid CustomerId { get; init; }
}
```

When you publish it:

```csharp
var message = new OrderCreated
{
    OrderId = Guid.NewGuid(),
    CustomerId = customerId
};
```

Once created, nobody can accidentally change the `OrderId` or `CustomerId`.

This is ideal because messages represent **facts**—for example, "Order 1234 was created." That fact shouldn't change after it's been published.

## Records and `init`

You'll often see `init` used with records:

```csharp
public record OrderCreated(Guid OrderId, Guid CustomerId);
```

or

```csharp
public record OrderCreated
{
    public Guid OrderId { get; init; }
    public Guid CustomerId { get; init; }
}
```

Records are designed to represent immutable data, so `init` fits naturally.

## How is it different from `private set`?

```csharp
public string Name { get; private set; }
```

A `private set` means:

- External code **cannot** change the property.
- The class itself **can** change it at any time.

```csharp
public void Rename(string newName)
{
    Name = newName;   // ✅ Allowed
}
```

With `init`:

```csharp
public string Name { get; init; }
```

Even the class cannot change the property after initialization.

# Interview Tips

`Why would you use `init`instead of`set`?`

> "`init` allows properties to be assigned only during object creation, making objects immutable afterward. Immutable objects are safer in concurrent applications, prevent accidental modification, and are well-suited for DTOs, events, configuration objects, and messages in distributed systems like MassTransit."

## Rule of thumb

- **`get; set;`** → Mutable business entities whose state changes over time.
- **`get; init;`** → Immutable DTOs, API models, configuration objects, and event/command messages.
- **`get; private set;`** → State can change, but only through the class's own methods, preserving encapsulation.
