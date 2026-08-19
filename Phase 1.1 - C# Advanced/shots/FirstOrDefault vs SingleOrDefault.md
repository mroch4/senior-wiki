# `FirstOrDefault()` vs `SingleOrDefault()`

## Core difference

|              | `FirstOrDefault()`                | `SingleOrDefault()`                                  |
| ------------ | --------------------------------- | ---------------------------------------------------- |
| 0 elements   | `default`                         | `default`                                            |
| 1 element    | returns it                        | returns it                                           |
| 2+ elements  | returns **first**                 | throws **exception**                                 |
| Main meaning | "Give me any/first matching item" | "There must be zero or one matching item"            |
| Complexity   | Can stop after first match        | Must inspect enough/all elements to prove uniqueness |

Example:

```csharp
var users = new[]
{
    new User { Id = 1 },
    new User { Id = 2 },
    new User { Id = 2 }
};

var a = users.FirstOrDefault(x => x.Id == 2);
// returns User with Id = 2

var b = users.SingleOrDefault(x => x.Id == 2);
// throws InvalidOperationException
```

## The important conceptual distinction

Think of them as expressing **different business assumptions**:

```csharp
FirstOrDefault()
```

means:

> I don't care if there are multiple matches. Give me the first one.

Whereas:

```csharp
SingleOrDefault()
```

means:

> According to my domain rules, there can be at most one match. If there are duplicates, that's an error.

That's why `SingleOrDefault()` can be useful as an **invariant check**.

For example, if `Email` is supposed to be unique:

```csharp
var user = users.SingleOrDefault(x => x.Email == email);
```

If two users have the same email, silently returning one user would potentially hide a data-integrity problem.

## What about `First()` and `Single()`?

Same distinction, but they behave differently when **nothing is found**:

|                   | 0 matches | 1 match | multiple matches |
| ----------------- | --------- | ------- | ---------------- |
| First()           | exception | value   | first value      |
| FirstOrDefault()  | default   | value   | first value      |
| Single()          | exception | value   | exception        |
| SingleOrDefault() | default   | value   | exception        |

## Database queries

With EF Core, the distinction is also meaningful at the database level.

```csharp
await db.User.SingleOrDefaultAsync(x => x.Email == email);
```

communicates that `Email` should identify **at most one user**.

If the database has a unique constraint on `Email`, that's an especially strong combination:

```text
Database constraint
 ↓
Email is unique
 ↓
SingleOrDefault()
 ↓
Application code expresses the same invariant
```

But don't use `SingleOrDefault()` just because it "sounds safer." If multiple records are legitimately possible and you simply want the first according to some ordering, use `FirstOrDefault()`:

```csharp
var latestOrder = orders
    .Where(x => x.CustomerId == customerId)
    .OrderByDescending(x => x.CreatedAt)
    .FirstOrDefault();
```

Here multiple orders are expected. `SingleOrDefault()` would be incorrect.

# Interview Tips

This:

```csharp
users.FirstOrDefault(x => x.Id == id);
```

does **not** mean:

> "There is exactly one user with this ID."

It means:

> "Find the first user whose ID matches."

If uniqueness is part of the requirement, `SingleOrDefault()` better expresses that requirement.

**Rule of thumb:**

> First = multiple matches are okay.

> Single = multiple matches indicate a problem.

> OrDefault = zero matches are okay.
