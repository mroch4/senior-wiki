# Pattern matching

## Table of content

1. [What is Pattern Matching?](#what-is-pattern-matching)
   - [What advantages does it provide?](#what-advantages-does-it-provide)
   - [Why was it introduced?](#why-was-it-introduced)
2. [Difference between `is` and `as`?](#difference-between-is-and-as)
3. [Types of Patterns](#types-of-patterns)
   - [1. Type Pattern](#1-type-pattern)
   - [2. Constant Pattern](#2-constant-pattern)
   - [3. Relational Pattern](#3-relational-pattern)
   - [4. Logical Pattern](#4-logical-pattern)
     - [and](#and)
     - [or](#or)
     - [not](#not)
   - [5. Property Pattern](#5-property-pattern)
   - [6. Nested Property Pattern](#6-nested-property-pattern)
   - [7. Positional Pattern](#7-positional-pattern)
   - [8. List Pattern (C# 11)](#8-list-pattern-c-11)
4. [Pattern Matching inside switch](#pattern-matching-inside-switch)
5. [Type matching in switch](#type-matching-in-switch)
6. [Conditions with when](#conditions-with-when)
7. [Interview Tips](#interview-tips)
   - [Why is pattern matching popular in modern .NET?](#why-is-pattern-matching-popular-in-modern-net)
   - [Real ASP.NET Core Example](#real-aspnet-core-example)
   - [Cheat Sheet](#cheat-sheet)

## What is Pattern Matching?

Pattern matching is a feature that lets you ask:

> "Does this object match a particular pattern?"

If it does, you can immediately use the matched values. Instead of writing:

```csharp
if (obj is Customer)
{
    Customer customer = (Customer)obj;
    Console.WriteLine(customer.Name);
}
```

you write:

```csharp
if (obj is Customer customer)
{
    Console.WriteLine(customer.Name);
}
```

Notice:

- checks the type
- performs the cast
- creates the variable

### What advantages does it provide?

- Less casting
- Less null checking
- Cleaner code
- Safer type handling
- Easier `switch` statements
- Better readability
- More expressive conditions

### Why was it introduced?

Pattern matching removes this boilerplate. Older C# code often looked like this:

```csharp
if (obj != null && obj.GetType() == typeof(Customer))
{
    Customer c = (Customer)obj;
}
```

or

```csharp
Customer c = obj as Customer;

if (c != null)
{
}
```

## Difference between `is` and `as`?

| `is`                                         | `as`                                         |
| -------------------------------------------- | -------------------------------------------- |
| Checks type                                  | Attempts cast                                |
| Can declare a variable (`obj is Customer c`) | Returns `null` if cast fails                 |
| Supports all pattern matching features       | Only performs reference/nullable conversions |

Example:

```csharp
if (obj is Customer customer)
{
    Console.WriteLine(customer.Name);
}
```

vs.

```csharp
Customer? customer = obj as Customer;

if (customer != null)
{
    Console.WriteLine(customer.Name);
}
```

Modern C# generally favors `is` with pattern matching because it is more concise and less error-prone.

## Types of Patterns

### 1. Type Pattern

```csharp
object value = "Hello";

if (value is string text)
{
    Console.WriteLine(text.Length);
}
```

Equivalent to:

```csharp
if (value is string)
{
    string text = (string)value;
}
```

### 2. Constant Pattern

```csharp
if (status is 200)
{
    Console.WriteLine("OK");
}
```

Instead of:

```csharp
if (status == 200)
```

Works with:

```csharp
null
true
false
numbers
strings
enums
```

Example:

```csharp
if (name is null)
```

instead of

```csharp
if (name == null)
```

### 3. Relational Pattern

```csharp
if (age is > 18)
```

```csharp
age is >= 18
```

```csharp
price is < 100
```

```csharp
temperature is <= 0
```

### 4. Logical Pattern

Combine patterns:

#### and

```csharp
if (age is >= 18 and <= 65)
```

Instead of:

```csharp
if (age >= 18 && age <= 65)
```

#### or

```csharp
if (day is DayOfWeek.Saturday or DayOfWeek.Sunday)
```

#### not

```csharp
if (name is not null)
```

Instead of:

```csharp
if (name != null)
```

### 5. Property Pattern

Matches object properties directly without explicitly accessing them. Suppose:

```csharp
public class User
{
    public string Name { get; set; }
    public int Age { get; set; }
}
```

Instead of:

```csharp
if (user.Age >= 18)
```

Write:

```csharp
if (user is { Age: >= 18 })
```

Multiple properties:

```csharp
if (user is
{
    Age: >= 18,
    Name: "John"
})
```

### 6. Nested Property Pattern

Objects inside objects:

```csharp
public class Order
{
    public Customer Customer { get; set; }
}

public class Customer
{
    public string Country { get; set; }
}
```

Pattern:

```csharp
if (order is
{
    Customer:
    {
        Country: "Poland"
    }
})
```

No need for:

```csharp
if (order.Customer != null &&
    order.Customer.Country == "Poland")
```

### 7. Positional Pattern

Works with records or classes supporting deconstruction:

```csharp
public record Point(int X, int Y);
```

Pattern:

```csharp
if (point is (0, 0))
{
    Console.WriteLine("Origin");
}
```

Or:

```csharp
if (point is (>0, >0))
```

### 8. List Pattern (C# 11)

Matches arrays or other sequence-like collections by their contents and structure:

```csharp
int[] numbers = {1,2,3};
```

Pattern:

```csharp
if (numbers is [1,2,3])
```

Beginning:

```csharp
if (numbers is [1, ..])
```

Ends with:

```csharp
if (numbers is [.., 100])
```

Middle:

```csharp
if (numbers is [1, .., 10])
```

## Pattern Matching inside switch

Modern C# prefers switch expressions:

```csharp
switch(animal)
{
    case Dog:
        ...
        break;

    case Cat:
        ...
        break;
}
```

Equivalent to:

```csharp
var sound = animal switch
{
    Dog => "Woof",
    Cat => "Meow",
    _ => "Unknown"
};
```

## Type matching in switch

```csharp
var result = shape switch
{
    Circle c => c.Radius * c.Radius * Math.PI,
    Rectangle r => r.Width * r.Height,
    _ => 0
};
```

## Conditions with when

```csharp
var message = person switch
{
    Student s when s.Grade > 90 => "Excellent",
    Student => "Student",
    Teacher => "Teacher",
    _ => "Unknown"
};
```

# Interview Tips

## Why is pattern matching popular in modern .NET?

If you're asked about modern C# features, mention that pattern matching has evolved significantly since C# 7 and now includes **type, constant, relational, logical, property, positional, and list patterns**, enabling expressive, type-safe code with minimal casting and cleaner control flow. It makes business logic, API request handling, validation, and domain modeling more declarative and maintainable. It's widely used with `switch` expressions, records, Minimal APIs, and CQRS/MediatR handlers.

## Real ASP.NET Core Example

This is common in CQRS and MediatR-based applications. Suppose an endpoint receives:

```csharp
object command
```

Pattern matching lets you handle different command types cleanly.

```csharp
return command switch
{
    CreateOrderCommand c => Handle(c),
    CancelOrderCommand c => Handle(c),
    UpdateOrderCommand c => Handle(c),
    _ => Results.BadRequest()
};
```

## Cheat Sheet

| Pattern           | Example                                        |
| ----------------- | ---------------------------------------------- |
| Type              | `obj is Customer c`                            |
| Constant          | `status is 404`                                |
| Null              | `obj is null`                                  |
| Not               | `obj is not null`                              |
| Relational        | `age is > 18`                                  |
| Logical           | `age is >= 18 and <= 65`                       |
| Property          | `user is { Age: >= 18 }`                       |
| Nested            | `order is { Customer: { Country: "Poland" } }` |
| Positional        | `point is (0, 0)`                              |
| List              | `numbers is [1, .., 10]`                       |
| Switch expression | `shape switch { Circle c => ..., _ => ... }`   |
