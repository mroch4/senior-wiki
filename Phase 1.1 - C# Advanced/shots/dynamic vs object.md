# `dynamic` vs `object`

Both can hold **any object**, but the crucial difference is **when member access is checked**.

## The key difference

```csharp
object obj = "Hello";
dynamic dyn = "Hello";

Console.WriteLine(obj.Length); // ❌ Compile error
Console.WriteLine(dyn.Length); // ✅ Resolved at runtime
```

With `object`, the compiler only knows:

> “This is an `object`.”

With `dynamic`, the compiler says:

> “I'll figure out what this operation means at runtime.”

## Comparison

|                            | `object`                | `dynamic`         |
| -------------------------- | ----------------------- | ----------------- |
| Can hold any object        | ✅                      | ✅                |
| Compile-time type          | `object`                | `dynamic`         |
| Actual runtime type        | Preserved               | Preserved         |
| Member access without cast | ❌                      | ✅                |
| Type safety                | ✅ Compile-time         | ❌ Runtime        |
| Invalid operation          | Compile error           | Runtime exception |
| Casting often required     | Yes                     | Usually no        |
| IntelliSense               | Good after cast         | Limited           |
| Runtime binding overhead   | Normal                  | Yes               |
| Best for                   | Generic/unknown objects | Dynamic APIs/data |

## Example

With `object`:

```csharp
object value = "Hello";

string text = (string)value;

Console.WriteLine(text.Length);
```

You have to tell the compiler what type you expect.

With `dynamic`:

```csharp
dynamic value = "Hello";

Console.WriteLine(value.Length);
```

The runtime determines that `value` is a `string` and resolves `Length`.

## A very important difference

Consider:

```csharp
object value = 10;

Console.WriteLine(value + 5);
```

❌ Doesn't compile, because `object` doesn't define `+`.

But:

```csharp
dynamic value = 10;

Console.WriteLine(value + 5);
```

✅ Prints `15`.

The runtime sees that the actual value is an `int` and performs integer addition.

## What about casting?

`object`:

```csharp
object value = 123;

int number = (int)value;
```

`dynamic`:

```csharp
dynamic value = 123;

int number = value;
```

The second one works because the dynamic binder performs the conversion at runtime.

But this also means errors move from **compile time → runtime**:

```csharp
dynamic value = "123";

int number = value; // RuntimeBinderException
```

# Interview Tips

> `object` gives you a type-safe reference to an unknown object and generally requires casting to access specific members; `dynamic` defers member binding and type checking to runtime, giving more flexibility at the cost of compile-time safety.

`dynamic` is **not the opposite of `object`**.

In fact, a variable declared as `dynamic` is effectively represented as `object` in the CLR, with compiler-generated metadata indicating dynamic behavior.

For example:

```csharp
dynamic x = GetSomething();
```

The CLR still deals with an object reference. The special behavior comes from the **C# compiler + Dynamic Language Runtime**, which performs runtime binding.

### Easy mental model

Think of them like this:

```text
object
 ↓
"I don't know the specific type,
but the compiler will enforce object rules."

dynamic
 ↓
"I don't know the specific type,
so let the runtime figure out the operation."
```
