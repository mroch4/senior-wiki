# `var` vs `dynamic`

The key difference is **when type checking happens**:

- `var` → **compile-time type**
- `dynamic` → **runtime type checking**

## `var`

`var` does **not** mean "unknown type". The compiler figures out the type **at compile time**.

```csharp
var name = "John";
var age = 30;

name = "Mike";   // OK
name = 123;      // ❌ Compile error
```

The compiler effectively treats it as:

```csharp
string name = "John";
int age = 30;
```

So after compilation, `var` has no special runtime behavior.

```csharp
var person = new Person();

person.Name;        // compiler knows this exists
person.NonExisting; // ❌ compile error
```

**Interview answer:**

> `var` is implicitly typed but still statically typed. The compiler determines its concrete type at compile time.

## `dynamic`

`dynamic` tells the compiler:

> "Don't check this member access now. Resolve it at runtime."

```csharp
dynamic value = "hello";

Console.WriteLine(value.Length); // OK
```

But:

```csharp
dynamic value = "hello";

Console.WriteLine(value.SomeMethod());
```

This **compiles**, but throws at runtime because `string` doesn't have `SomeMethod()`.

You can even change the runtime type:

```csharp
dynamic value = "hello";
Console.WriteLine(value.Length); // string

value = 123;
Console.WriteLine(value + 10);   // int
```

## The important comparison

|                             | `var`            | `dynamic`                         |
| --------------------------- | ---------------- | --------------------------------- |
| Type determined             | Compile time     | Runtime                           |
| Statically typed            | ✅ Yes           | ❌ No                             |
| IntelliSense                | ✅               | Limited                           |
| Compile-time checking       | ✅               | ❌                                |
| Invalid member access       | Compile error    | Runtime exception                 |
| Runtime overhead            | Essentially none | Can have dynamic binding overhead |
| Can change underlying type? | ❌               | ✅                                |

## A very common interview trap

This:

```csharp
var x = GetSomething();
```

does **not** mean:

> "I don't know what type `x` is."

It means:

> "The compiler knows the type; I just don't explicitly write it."

Whereas:

```csharp
dynamic x = GetSomething();
```

means:

> "Defer binding/type-related member resolution until runtime."

## Why would you ever use `dynamic`?

Mostly when working with things whose shape/type isn't conveniently known at compile time, for example:

- COM interop
- some reflection scenarios
- scripting/dynamic languages
- certain JSON/dynamic APIs
- legacy APIs

In normal application code, **prefer static typing (`var`, explicit types, interfaces, generics)** unless you have a specific reason to use `dynamic`.

# Interview Tips

> "`var` is compile-time implicit typing; `dynamic` is runtime binding. `var` gives you full compile-time type safety, while `dynamic` moves member/type resolution to runtime."
