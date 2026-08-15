## Table of Content

1. [IEnumerable vs IQueryable](#1-ienumerable-vs-iqueryable)
   - [IEnumerable](#ienumerable)
   - [IQueryable](#iqueryable)
   - [Interview question](#interview-question)
2. [Expression Trees](#2-expression-trees)
3. [Query Composition](#3-query-composition)
4. [Multiple Enumeration](#4-multiple-enumeration)
5. [Streaming vs Buffering](#5-streaming-vs-buffering)
   - [Streaming](#streaming)
   - [Buffering](#buffering)
6. [Lazy Evaluation Pipeline](#6-lazy-evaluation-pipeline)
7. [Short-Circuit Operators](#7-short-circuit-operators)
8. [Side Effects](#8-side-effects)
9. [Captured Variables](#9-captured-variables)
10. [Client vs Server Evaluation (EF Core)](#10-client-vs-server-evaluation-ef-core)
11. [AsEnumerable()](#11-asenumerable)
12. [Materialization](#12-materialization)
13. [Async Queries](#13-async-queries)
14. [Senior-Level Mental Model](#senior-level-mental-model)
15. [Interview Tips](#interview-tips)

## 1. IEnumerable vs IQueryable

### IEnumerable

```csharp
IEnumerable<User> users = context.Users;
```

Operations happen **in memory**.

Example:

```csharp
var users = context.Users.ToList();

var adults = users.Where(u => u.Age >= 18);
```

Execution:

```
Database
 |
SELECT *
 |
Application Memory
 |
Where(Age >=18)
```

The database returns **all users**.

Filtering happens in .NET.

### IQueryable

```csharp
IQueryable<User> users = context.Users;

var adults = users.Where(u => u.Age >= 18);
```

Nothing executes yet.

When:

```csharp
await adults.ToListAsync();
```

EF Core generates:

```sql
SELECT *
FROM Users
WHERE Age >= 18
```

Filtering happens inside SQL Server.

Huge performance difference.

### Interview question

❌ Bad:

```csharp
var users = context.Users.ToList();

var active = users.Where(u => u.IsActive);
```

✅ Good:

```csharp
var active = await context.Users.Where(u => u.IsActive).ToListAsync();
```

## 2. Expression Trees

Why can EF convert this

```csharp
.Where(u => u.Age > 18)
```

into SQL?

Because with `IQueryable`, the lambda is **not compiled into executable code**.

Instead it becomes an expression tree.

```
u => u.Age > 18
```

becomes `Expression<Func<User,bool>>`

Instead of saying:

> Execute this code

it says:

> Here is the structure of this code.

EF reads the tree and translates it.

```
Where
 └── Age
      >
     18
```

That becomes SQL.

With IEnumerable

```csharp
Func<User,bool>
```

is compiled code.

EF cannot inspect compiled machine code.

### Senior interview question:

> Why can EF translate one lambda but not another?

Because only expression trees are translatable.

## 3. Query Composition

Deferred execution allows building queries gradually:

```csharp
var query = context.Users.AsQueryable();

if (onlyActive)
    query = query.Where(u => u.IsActive);

if (country != null)
    query = query.Where(u => u.Country == country);

if (age != null)
    query = query.Where(u => u.Age >= age);

return await query.ToListAsync();
```

SQL is generated only once.

Example

```
SELECT *
FROM Users
WHERE IsActive = 1
AND Country='PL'
AND Age>=18
```

This is one of EF Core's greatest strengths.

## 4. Multiple Enumeration

One of the biggest hidden performance issues.

```csharp
var query = numbers.Where(x => x > 100);
```

Later:

```csharp
query.Count();

query.First();

query.Last();
```

The filter runs **three** times.

For EF, even worse:

```csharp
query.CountAsync();

query.FirstAsync();

query.ToListAsync();
```

**Three** SQL queries:

```
SELECT COUNT(*)

SELECT TOP(1)

SELECT *
```

Three round trips.

Often better:

```csharp
var list = await query.ToListAsync();
```

Then:

```csharp
list.Count

list.First()

list.Last()
```

Only one SQL query.

## 5. Streaming vs Buffering

Deferred execution **streams**.

Immediate execution **buffers**.

### Streaming

```csharp
foreach(var item in query)
{
}
```

```
Database
 |
record 1
record 2
record 3
```

Memory stays low.

### Buffering

```csharp
var list = await query.ToListAsync();
```

```
Database
 |
All records
 |
Memory
```

Everything loads before processing.

### Interview question

When should you avoid ToList()?

> When processing millions of rows.

## 6. Lazy Evaluation Pipeline

Suppose:

```csharp
var result = numbers.Where(x => x > 2).Select(x => x * 10).Take(2);
```

People imagine:

```
Where
 |
entire collection
 |
Select
 |
Take
```

Wrong.

LINQ works element-by-element.

Actual execution

```
Item 1
 ↓
Where
 ↓
rejected
---------------
Item 2
 ↓
Where
 ↓
rejected
---------------
Item 3
 ↓
Where
 ↓
Select
 ↓
Take
---------------
Item 4
 ↓
Where
 ↓
Select
 ↓
Take
---------------
STOP
```

No more elements processed. This is why deferred execution is fast.

## 7. Short-Circuit Operators

Some operators stop early:

```csharp
numbers.Any(x => x > 10)
```

Stops on first match.

```csharp
numbers.First()
```

Reads one item.

```csharp
numbers.Take(5)
```

Reads five items.

Whereas:

```csharp
Count()
```

Must examine the entire sequence (unless the underlying collection already exposes a count, like `List<T>`).

## 8. Side Effects

❌ Bad:

```csharp
var query = numbers.Where(x =>
{
    Log(x);
    return x > 5;
});
```

If enumerated twice:

```
Log
Log
Log
```

Everything repeats. LINQ should ideally be pure.

## 9. Captured Variables

A classic interview trick:

```csharp
int threshold = 10;

var query = numbers.Where(x => x > threshold);

threshold = 20;

query.ToList();
```

What happens?

It uses **20** - because the lambda captures the **variable**, not its **value**.

## 10. Client vs Server Evaluation (EF Core)

Suppose:

```csharp
.Where(u => MyMethod(u.Name))
```

EF cannot translate `MyMethod()` into SQL.

Older EF versions sometimes fetched all rows and evaluated the method in memory, which could cause major performance issues.

Modern EF Core generally throws an exception instead of silently switching to client-side evaluation, helping you catch translation problems early.

## 11. AsEnumerable()

This switches execution from SQL translation to in-memory LINQ.

```csharp
context.Users.Where(u => u.Age > 18).AsEnumerable().Where(u => MyMethod(u.Name));
```

Before `AsEnumerable()`: SQL

After `AsEnumerable()` : LINQ to Objects

Useful when part of the query can't be translated but you still want the database to do as much filtering as possible.

## 12. Materialization

Materialization means converting query results into actual CLR objects:

```
SQL rows
 ↓
User objects
 ↓
List<User>
```

Materialization occurs at:

```csharp
ToList()

ToArray()

First()

Single()
```

## 13. Async Queries

EF Core keeps **deferred execution** with async methods:

```csharp
var query = context.Users.Where(u => u.IsActive);
```

Still no SQL.

Only here execution begins:

```csharp
await query.ToListAsync();
```

## Senior-Level Mental Model

Think of a LINQ query as a **pipeline**:

```
Source
 ↓
Where
 ↓
Select
 ↓
OrderBy
 ↓
Skip
 ↓
Take
 ↓
Execution
```

For `IEnumerable<T>`, each stage processes items as they are requested (pull-based iteration). For `IQueryable<T>`, each stage builds an expression tree describing the query. Only when you materialize the results does the provider (such as EF Core) translate that tree—often into SQL—and execute it.

# Interview Tips

If you can comfortably explain **expression trees**, **query providers**, **`IQueryable` vs `IEnumerable`**, **multiple enumeration**, **streaming vs buffering**, and **client vs server evaluation**, you're answering LINQ questions at the level typically expected of a strong mid-level or senior .NET developer.

Why is deferred execution beneficial?

> It enables query composition, avoids unnecessary work, and can reduce memory usage by streaming results.

Why is `IQueryable` faster than `IEnumerable` for databases?

> Because filtering, sorting, and projection are translated into SQL and executed by the database server.

Why avoid calling `ToList()` too early?

> It materializes the query, preventing further operations from being translated to SQL and often loading more data than necessary.

What is multiple enumeration?

> Enumerating the same deferred query multiple times, causing repeated computation or repeated database queries.

What is an expression tree?

> A data structure representing code (`Expression<Func<T,...>>`) that providers like EF Core can inspect and translate.

What does `AsEnumerable()` do?

> It changes from provider-specific query translation (e.g., SQL) to LINQ-to-Objects, so subsequent operators execute in memory.

Why is `Take(10)` efficient?

> LINQ stops after producing ten results, and EF Core translates it to SQL (e.g., `TOP (10)` or `LIMIT 10`), so unnecessary rows are not processed.
