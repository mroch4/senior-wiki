## Struct in C#

A **struct** is a **value type** in C# used to represent small pieces of data that logically belong together.

```csharp
public struct Point
{
    public int X;
    public int Y;
}
```

Usage:

```csharp
Point p1 = new Point { X = 10, Y = 20 };
Point p2 = p1;

p2.X = 100;

Console.WriteLine(p1.X); // 10
Console.WriteLine(p2.X); // 100
```

### Struct vs class

The key difference is **value semantics vs reference semantics**.

|               | `struct`                                          | `class`                        |
| ------------- | ------------------------------------------------- | ------------------------------ |
| Type          | Value type                                        | Reference type                 |
| Assignment    | Copies the value                                  | Copies the reference           |
| Can be `null` | No\*                                              | Yes                            |
| Inheritance   | Cannot inherit from another class/struct          | Supports inheritance           |
| Typical use   | Small data values                                 | Objects/entities with identity |
| Allocation    | Often inline/stack/register, depending on context | Usually heap allocated         |

- A struct can be nullable with `Point?`.

### Important: "struct = stack" is NOT always correct

This is a common interview trap.

A struct is a **value type**, but that doesn't mean it always lives on the stack.

```csharp
class Person
{
    public Point Location;
}
```

Here `Location` is a struct, but it is stored **inside the `Person` object**, which is on the heap.

Similarly:

```csharp
Point[] points = new Point[100];
```

The array is on the heap, and the `Point` values are stored **inside the array**.

So the better statement is:

> **Struct determines value semantics, not necessarily stack allocation.**

### When should you use a struct?

Good candidates are usually:

- small
- immutable
- represent a single value
- have value-based equality

Examples:

```csharp
DateTime
TimeSpan
Guid
decimal
int
double
```

For your interview, remember this distinction:

**`class` → identity**
**`struct` → value**

For example, two `Person` objects can represent different people even if their data is identical, while two `Point(10, 20)` values are conceptually the same point.

### Advanced point: copying

Structs are copied by value:

```csharp
Point a = new(10, 20);
Point b = a;
```

Now there are **two independent values**.

For large structs, frequent copying can become expensive. That's one reason modern C# provides:

```csharp
in Point point     // readonly reference
ref Point point    // reference to existing value
ref readonly Point point
```

This is particularly relevant when discussing **`Span<T>`, performance, and avoiding unnecessary copies**.
