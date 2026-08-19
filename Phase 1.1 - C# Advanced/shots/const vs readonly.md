# `const` vs `readonly`

The main difference is **when the value is assigned and how the value is stored**.

## `const`

A `const` value is a **compile-time constant**.

```csharp
public const int MaxRetries = 3;
```

Rules:

- Must be assigned **at declaration**
- Cannot be changed
- Must be a compile-time constant
- Is implicitly `static`
- Accessed through the type

```csharp
public class Config
{
    public const int MaxRetries = 3;
}

var x = Config.MaxRetries;
```

You cannot do:

```csharp
const int x = GetValue(); // ❌
```

because `GetValue()` executes at runtime.

## `readonly`

A `readonly` field can be assigned **at runtime**, but only during initialization or in the constructor.

```csharp
public class Config
{
    public readonly int MaxRetries;

    public Config(int retries)
    {
        MaxRetries = retries;
    }
}
```

Now:

```csharp
var config = new Config(5);

Console.WriteLine(config.MaxRetries); // 5
```

But after construction:

```csharp
config.MaxRetries = 10; // ❌
```

`readonly` is **not automatically static**:

```csharp
public readonly int Value;
```

Each object can have a different value.

## The important comparison

|                                  | `const`          | `readonly`                 |
| -------------------------------- | ---------------- | -------------------------- |
| Assignment                       | Declaration only | Declaration or constructor |
| Value known at                   | Compile time     | Runtime                    |
| Can change after initialization? | ❌               | ❌                         |
| Implicitly static?               | ✅               | ❌                         |
| Can depend on constructor input? | ❌               | ✅                         |
| Can use `DateTime.Now`?          | ❌               | ✅                         |
| Can use method result?           | ❌               | ✅                         |

## Example that makes the distinction clear

```csharp
public class User
{
    public const string ApplicationName = "MyApp";

    public readonly DateTime CreatedAt;

    public User()
    {
        CreatedAt = DateTime.Now;
    }
}
```

`ApplicationName` is the same compile-time constant for everyone.

`CreatedAt` is different for each `User`, and its value is determined **when the object is created**.

### ⚠️ Senior-level detail: `const` is substituted at compile time

This is an important interview point.

Suppose library A contains:

```csharp
public const int MaxRetries = 3;
```

Library B compiles:

```csharp
Console.WriteLine(Config.MaxRetries);
```

The compiler can effectively embed `3` into library B's compiled code.

So if library A changes:

```csharp
public const int MaxRetries = 5;
```

but library B is **not recompiled**, it can still use `3`.

With `readonly`, the value is obtained at runtime:

```csharp
public static readonly int MaxRetries = LoadFromConfig();
```

This is one reason `static readonly` is often preferable for values that are conceptually constants but **aren't compile-time constants**.

# Interview Tips

> **`const` is a compile-time constant and is implicitly static. `readonly` is a runtime-initialized field that can only be assigned during declaration or construction. `const` is suitable when the value is truly known at compile time; `readonly` is used when the value needs to be determined at runtime but must not change afterward.**
