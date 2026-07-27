# DRY

## Table of Content

1. [What is DRY?](#what-is-dry)
   - [Why DRY matters](#why-dry-matters)
2. [Example 1 – Duplicate business logic](#example-1--duplicate-business-logic)
   - [Not DRY](#not-dry)
   - [DRY](#dry)
3. [Example 2 – Validation](#example-2--validation)
   - [Not DRY](#not-dry-1)
   - [DRY](#dry-1)
4. [Example 3 – Constants](#example-3--constants)
   - [Not DRY](#not-dry-2)
   - [DRY](#dry-2)
5. [Example 4 – Repository](#example-4--repository)
6. [DRY is not "never repeat code"](#dry-is-not-never-repeat-code)
   - [Over-applying DRY](#over-applying-dry)
7. [DRY + KISS](#dry--kiss)
8. [Interview Tips](#interview-tips)

## What is DRY?

> **DRY** stands for **Don't Repeat Yourself**.

It is one of the core software engineering principles and means:

> **Every piece of knowledge or business logic should have a single, authoritative representation in the system.**

The goal isn't just to avoid copy-pasting code—it's to avoid duplicating **knowledge**.

### Why DRY matters

Duplicated logic leads to:

- ❌ More bugs
- ❌ More maintenance
- ❌ Inconsistent behavior
- ❌ Harder refactoring

If you need to change a business rule, you should ideally change it **once**.

## Example 1 – Duplicate business logic

### Not DRY

```csharp
public decimal CalculateOrderTotal(decimal price)
{
    return price * 1.23m;
}

public decimal CalculateInvoiceTotal(decimal price)
{
    return price * 1.23m;
}
```

Suppose VAT changes from **23%** to **22%**.

Now you must remember to update both methods.

### DRY

```csharp
private const decimal VatRate = 1.23m;

public decimal ApplyVat(decimal price)
{
    return price * VatRate;
}
```

Now the VAT calculation exists in one place.

## Example 2 – Validation

### Not DRY

```csharp
if (user.Age < 18)
    throw new Exception();

...

if (customer.Age < 18)
    throw new Exception();

...

if (employee.Age < 18)
    throw new Exception();
```

### DRY

```csharp
public static void ValidateAdult(int age)
{
    if (age < 18)
        throw new ArgumentException("Must be at least 18.");
}
```

## Example 3 – Constants

### Not DRY

```csharp
if (status == "Completed")
```

```csharp
if (order.Status == "Completed")
```

```csharp
return "Completed";
```

One typo breaks the application.

### DRY

```csharp
public static class OrderStatuses
{
    public const string Completed = "Completed";
}
```

Then:

```csharp
if (status == OrderStatuses.Completed)
```

Even better, use an enum if the values are fixed:

```csharp
public enum OrderStatus
{
    Pending,
    Processing,
    Completed
}
```

## Example 4 – Repository

Instead of:

```csharp
var order = db.Orders.First(...);
```

written in 15 places,

create

```csharp
GetOrderById(id);
```

If retrieval changes (e.g., you need to include related entities), you update one method instead of 15. (In EF Core, be careful not to create repositories that simply wrap `DbSet<T>` without adding value.)

## DRY is not "never repeat code"

Sometimes repeating a few lines is better than creating a generic abstraction too early.

### Over-applying DRY

```csharp
public T Execute<T>(Func<T> action)
{
    // 100 lines of generic magic
}
```

just to avoid repeating:

```csharp
Console.WriteLine("Starting");
```

twice.

This violates **KISS** (Keep It Simple) because the abstraction is more complex than the duplication it removes.

## DRY + KISS

Good developers balance these principles.

Imagine two identical methods.

Should you combine them?

- If they're likely to stay the same forever → maybe not.
- If they represent the same business rule → yes.

A common saying is:

> **Duplicate code until the duplication teaches you what the right abstraction is.**

# Interview Tips

> **DRY (Don't Repeat Yourself)** means that each piece of business logic or knowledge should exist in only one place in the codebase. This reduces maintenance effort, prevents inconsistencies, and makes refactoring easier. However, DRY should be balanced with KISS and YAGNI—it's better to tolerate a small amount of duplication than introduce an overly complex abstraction too early.

## Quick comparison

| Principle | Purpose                                    | Example                                                             |
| --------- | ------------------------------------------ | ------------------------------------------------------------------- |
| **KISS**  | Keep solutions simple                      | Don't introduce CQRS for a simple CRUD app.                         |
| **DRY**   | Avoid duplicating knowledge                | Put VAT calculation in one method instead of several.               |
| **YAGNI** | Don't build features before they're needed | Don't implement a plugin system if there's only one implementation. |

A useful way to remember them is:

- **KISS:** _Don't make it more complicated than necessary._
- **DRY:** _Don't define the same business rule in multiple places._
- **YAGNI:** _Don't build for hypothetical future requirements._
