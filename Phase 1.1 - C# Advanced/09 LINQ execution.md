# LINQ execution

## Table of content

1. [What LINQ Execution?](#what-linq-execution)
2. [Deferred Execution](#deferred-execution)
   - [Deferred execution reflects changes](#deferred-execution-reflects-changes)
   - [Multiple enumerations](#multiple-enumerations)
   - [Deferred execution with EF Core](#deferred-execution-with-ef-core)
3. [Immediate Execution](#immediate-execution)
   - [Immediate execution does not reflect changes](#immediate-execution-does-not-reflect-changes)
4. [Deferred vs Immediate](#deferred-vs-immediate)
5. [Query operators](#query-operators)
   - [Deferred operators](#deferred-operators)
   - [Immediate operators](#immediate-operators)
6. [Performance Example](#performance-example)
7. [Interview trap](#interview-trap)
8. [Deferred execution and side effects](#deferred-execution-and-side-effects)
9. [Cheat Sheet](#cheat-sheet)
10. [Interview Tips](#interview-tips)

## What LINQ Execution?

LINQ queries execute in **two different ways**:

1. **Deferred Execution (Lazy Execution)** ← default
2. **Immediate Execution (Eager Execution)**

The difference is **when the query actually runs**.

## Deferred Execution

The query is **not executed when you write it**.

It only executes when you actually **enumerate** the collection.

Examples of enumeration:

- `foreach`
- `.ToList()`
- `.ToArray()`
- `.First()`
- `.Count()`
- `.Any()`

Example:

```csharp
var numbers = new List<int> { 1, 2, 3, 4 };

var evenNumbers = numbers.Where(x => x % 2 == 0);

foreach (var n in evenNumbers)
{
    Console.WriteLine(n);
}
```

Output:

```
2
4
```

The filtering didn't happen when we called `Where()`.

It happened **only** inside the `foreach`.

Why?

`Where()` doesn't return a list. It returns an `IEnumerable<T>` that simply remembers:

> "When somebody asks me for data, I'll filter it."

It's like a **recipe** instead of the finished meal.

### Deferred execution reflects changes

```csharp
var numbers = new List<int> { 1, 2, 3 };

var query = numbers.Where(x => x > 1);

numbers.Add(4);

foreach(var n in query)
{
    Console.WriteLine(n);
}
```

Output:

```
2
3
4
```

Why?

> Because the query wasn't executed until the `foreach` - at execution time the list already contained 4.

### Multiple enumerations

```csharp
var query = numbers.Where(x => x > 1);

foreach(var x in query)
{
}

foreach(var x in query)
{
}
```

The filtering happens **twice**.

Every enumeration re-executes the query.

This is important if the query:

- hits a database
- calls a web API
- performs expensive calculations

### Deferred execution with EF Core

```csharp
var users = context.Users.Where(u => u.IsActive);
```

Has SQL been executed?

> No.

SQL executes only when:

```csharp
await users.ToListAsync();
```

or

```csharp
await users.FirstAsync();
```

This allows EF Core to continue building the SQL query until execution.

## Immediate Execution

Sometimes we want the results **right now**.

Example

```csharp
var list = numbers.Where(x => x > 2).ToList();
```

Now filtering happens immediately.

`list` contains actual values.

### Immediate execution does not reflect changes

```csharp
var numbers = new List<int> { 1,2,3 };

var result = numbers.Where(x => x > 1).ToList();

numbers.Add(4);

foreach(var n in result)
{
    Console.WriteLine(n);
}
```

Output:

```
2
3
```

Not:

```
2
3
4
```

Because the snapshot was already created.

## Deferred vs Immediate

### Deferred

```
List
 ↓
Where
 ↓
Select
 ↓
OrderBy
 ↓
Nothing happens
 ↓
foreach
 ↓
Execute
```

### Immediate

```
List
 ↓
Where
 ↓
Select
 ↓
ToList
 ↓
Execute immediately
```

## Query operators

### Deferred operators

```csharp
Where()

Select()

OrderBy()

ThenBy()

Skip()

Take()

Distinct()

GroupBy()

Reverse()

Concat()
```

These return another `IEnumerable<T>`.

### Immediate operators

```csharp
ToList()

ToArray()

Count()

Any()

All()

First()

FirstOrDefault()

Single()

Max()

Min()

Average()

Sum()
```

These return a concrete result and execute immediately.

## Performance Example

Suppose you have 1,000,000 records

Deferred:

```csharp
var query = numbers.Where(...).Select(...).Take(5);
```

Nothing happens yet.

Only when you enumerate:

```csharp
query.ToList();
```

the pipeline runs, and because of `Take(5)`, it can stop after finding five matching elements instead of processing everything.

## Interview trap

```csharp
var query = numbers.Where(x =>
{
    Console.WriteLine($"Checking {x}");
    return x > 2;
});
```

Nothing is printed.

Only:

```csharp
query.ToList();
```

prints:

```
Checking 1
Checking 2
Checking 3
Checking 4
```

Because `Where()` itself **never executes the predicate until enumeration**.

## Deferred execution and side effects

Avoid relying on side effects inside LINQ queries:

```csharp
int counter = 0;

var query = numbers.Where(x =>
{
    counter++;
    return x > 2;
});

query.ToList();
query.ToList();

Console.WriteLine(counter);
```

`counter` is incremented **twice** because the query is executed twice. LINQ queries are intended to be **declarative** and **free of side effects**.

## Cheat Sheet

| Deferred Execution                                     | Immediate Execution                                    |
| ------------------------------------------------------ | ------------------------------------------------------ |
| Executes on enumeration                                | Executes immediately                                   |
| Returns `IEnumerable<T>`/`IQueryable<T>`               | Returns materialized data or a scalar value            |
| Reflects latest data changes                           | Produces a snapshot of the data                        |
| Re-executes on each enumeration                        | Executes only once unless the method is called again   |
| Examples: `Where`, `Select`, `OrderBy`, `Skip`, `Take` | Examples: `ToList`, `ToArray`, `Count`, `First`, `Any` |

# Interview Tips

Think of **deferred execution** as building a recipe: each LINQ operator adds another step, but nothing is cooked yet. The recipe is only executed when you ask for the results. **Immediate execution** is like preparing the meal right away and storing it, so later changes to the ingredients don't affect what you've already made.

What is deferred execution?

> Deferred execution means a LINQ query is **not executed when it is defined**. It runs only when the results are enumerated (for example, by `foreach` or `ToList()`).

Why is deferred execution useful?

- Improves performance by delaying work until needed.
- Enables query composition before execution.
- Reflects the latest state of the underlying data source.
- Avoids unnecessary processing if the query is never enumerated.

> What methods trigger immediate execution?

- Materialization: `ToList()`, `ToArray()`, `ToDictionary()`, `ToHashSet()`
- Scalar operators: `Count()`, `Any()`, `First()`, `Single()`, `Max()`, `Min()`, `Average()`, `Sum()`

Why does EF Core use deferred execution?

> It allows multiple LINQ operators (`Where`, `Select`, `OrderBy`, etc.) to be combined into a **single SQL query**, which is sent to the database only when the query is materialized (e.g., `ToListAsync()`).
