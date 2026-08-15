## Table of Content

1. [Pattern Matching and Flow Analysis](#1-pattern-matching-and-flow-analysis)
2. [Variable Patterns with Discards](#2-variable-patterns-with-discards)
3. [Recursive Patterns](#3-recursive-patterns)
4. [Extended Property Patterns (C# 10)](#4-extended-property-patterns-c-10)
5. [Pattern Matching with Records](#5-pattern-matching-with-records)
6. [Exhaustiveness Checking](#6-exhaustiveness-checking)
7. [Combining Patterns with `when`](#7-combining-patterns-with-when)
8. [Pattern Matching and Generic Code](#8-pattern-matching-and-generic-code)
9. [Type Pattern vs Polymorphism](#9-type-pattern-vs-polymorphism)
10. [Span and String Pattern Matching](#10-span-and-string-pattern-matching)
11. [`switch` Expression Evaluation Order](#11-switch-expression-evaluation-order)
12. [Pattern Matching with Nullable Reference Types](#12-pattern-matching-with-nullable-reference-types)
13. [Advanced Example: Minimal API Endpoint](#13-advanced-example-minimal-api-endpoint)
14. [Property Pattern + Logical Pattern Combination](#14-property-pattern--logical-pattern-combination)
15. [Interview Tips](#interview-tips)

## 1. Pattern Matching and Flow Analysis

The compiler understands patterns and performs **definite assignment analysis**.

Example:

```csharp
object? value = "hello";

if (value is string text)
{
    Console.WriteLine(text.Length);
}
```

Inside the block, the compiler knows:

- `value` is not null
- `value` is a string
- `text` is definitely assigned

You don't need:

```csharp
if(value != null && value.GetType() == typeof(string))
{
    var text = (string)value;
}
```

The compiler tracks the state.

## 2. Variable Patterns with Discards

A discard (`_`) means:

> "I only care whether the pattern matches, not the extracted value."

Example:

```csharp
if (request is CreateOrderCommand _)
{
    ProcessCreate();
}
```

Modern style:

```csharp
if (request is CreateOrderCommand)
{
    ProcessCreate();
}
```

But in switch expressions, `_` is extremely important:

```csharp
var result = user switch
{
    Admin => "Full access",
    Manager => "Limited access",
    _ => "Guest"
};
```

`_` is the **catch-all pattern**.

## 3. Recursive Patterns

A recursive pattern is when patterns contain other patterns.

Example:

```csharp
if(order is
{
    Customer:
    {
        Address:
        {
            Country: "Poland"
        }
    }
})
```

The compiler recursively evaluates:

```
Order
 └── Customer
      └── Address
           └── Country
```

No explicit null checks.

Equivalent old code:

```csharp
if(order != null &&
   order.Customer != null &&
   order.Customer.Address != null &&
   order.Customer.Address.Country == "Poland")
```

## 4. Extended Property Patterns (C# 10)

Before:

```csharp
if(order is
{
    Customer:
    {
        Address:
        {
            Country: "Poland"
        }
    }
})
```

C# 10 allows:

```csharp
if(order is
{
    Customer.Address.Country: "Poland"
})
```

## 5. Pattern Matching with Records

Records are designed to work with patterns.

Example:

```csharp
public record User(string Name, int Age);
```

```csharp
var message = user switch
{
    ("John", >18) => "Adult John",
    (_, <18) => "Minor",
    _ => "Unknown"
};
```

The compiler uses the generated `Deconstruct()` method.

## 6. Exhaustiveness Checking

The compiler can detect missing cases.

Example:

```csharp
enum Status
{
    Pending,
    Approved,
    Rejected
}
```

Bad:

```csharp
var text = status switch
{
    Status.Pending => "Waiting"
};
```

Compiler warning:

```
Switch expression does not handle all possible values
```

Better:

```csharp
var text = status switch
{
    Status.Pending => "Waiting",
    Status.Approved => "Done",
    Status.Rejected => "Failed"
};
```

or:

```csharp
_ => "Unknown"
```

## 7. Combining Patterns with `when`

Patterns decide **what it is**. `when` adds additional logic.

Example:

```csharp
var discount = customer switch
{
    PremiumCustomer p when p.Orders.Count > 10 => 20,
    PremiumCustomer => 10,
    _ => 0
};
```

Think of it as a `Pattern + Additional condition`

## 8. Pattern Matching and Generic Code

A common senior-level scenario:

```csharp
public static string Describe<T>(T value)
{
    return value switch
    {
        int i => $"Number {i}",
        string s => $"Text {s}",
        IEnumerable<int> numbers => $"Collection {numbers.Count()}",
        null => "Nothing",
        _ => "Unknown"
    };
}
```

The runtime type determines the match.

## 9. Type Pattern vs Polymorphism

Important architecture discussion.

Example:

```csharp
var result = shape switch
{
    Circle c => CalculateCircle(c),
    Square s => CalculateSquare(s)
};
```

Some developers argue:

> "You are replacing polymorphism with type checking."

Sometimes true.

Alternative:

```csharp
interface IShape
{
    double CalculateArea();
}
```

Then:

```csharp
shape.CalculateArea();
```

### Rule of thumb:

Use pattern matching when:

✅ You own the operation
✅ You have a closed set of types
✅ Adding operations is more common than adding types

Example:

Compiler AST:

```
Expression
 ├── AddExpression
 ├── MultiplyExpression
 └── ConstantExpression
```

Pattern matching is excellent.

Use polymorphism when:

✅ You frequently add new types
✅ Each type owns its behavior

Example:

```
PaymentMethod
 ├── CreditCard
 ├── PayPal
 └── Crypto
```

## 10. Span and String Pattern Matching

Modern .NET avoids allocations.

Example:

```csharp
ReadOnlySpan<char> command = "CREATE";

if(command is "CREATE")
```

No string allocation.

Useful in:

- parsers
- high-performance APIs
- serializers

## 11. `switch` Expression Evaluation Order

Patterns are evaluated top-to-bottom.

Example:

```csharp
var result = value switch
{
    object => "Object",
    string => "String"
};
```

Problem:

`string` is also an `object`.

The second case is unreachable.

Correct:

```csharp
var result = value switch
{
    string => "String",
    object => "Object"
};
```

More specific patterns **first**.

## 12. Pattern Matching with Nullable Reference Types

Example:

```csharp
string? name = null;

if(name is string text)
{
    Console.WriteLine(text.Length);
}
```

The compiler knows:

```
Before:
string?

Inside:
string
```

Pattern matching integrates deeply with nullable analysis.

## 13. Advanced Example: Minimal API Endpoint

Imagine:

```csharp
app.MapPost("/orders", (OrderRequest request) =>
{
    return request switch
    {
        { Items.Count: 0 } => Results.BadRequest(),
        { CustomerId: null } => Results.BadRequest(),
        { Items.Count: > 100 } => Results.Problem(),
        _ => Results.Ok()
    };
});
```

The validation rules become declarative.

## 14. Property Pattern + Logical Pattern Combination

Very powerful:

```csharp
if(user is
{
    Age: >= 18 and <= 65,
    Role: "Admin" or "Manager",
    IsActive: true
})
{
    GrantAccess();
}
```

This reads almost like business rules.

# Interview Tips

For a senior .NET developer, the most important patterns to master are:

1. **Type patterns**
2. **Property patterns**
3. **Recursive patterns**
4. **Switch expressions**
5. **Exhaustiveness**
6. **Pattern matching vs polymorphism trade-offs**
7. **Nullable flow analysis integration**

These are heavily used in modern ASP.NET Core, Minimal APIs, CQRS, and domain logic.

Is pattern matching a replacement for polymorphism?

> No. Pattern matching is better for closed hierarchies where new operations are added frequently. Polymorphism is better for open hierarchies where new types are added frequently.

Does pattern matching use reflection?

> No. A type pattern is compiled into runtime type checks (`isinst` IL instruction), not reflection.

```csharp
obj is Customer
```

Does pattern matching create objects?

> Usually no.

Example:

```csharp
obj is Customer c
```

does not create a new Customer. It creates only a reference variable pointing to the existing object.

Why is pattern matching important in Domain Driven Design?

> Because domain rules often look like:

```
IF Order
AND Customer is Premium
AND Total > threshold
THEN Apply discount
```

Pattern matching expresses these rules directly.
