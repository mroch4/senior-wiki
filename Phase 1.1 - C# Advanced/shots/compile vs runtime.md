# compile vs runtime

## Compile time

**Compile time = when your C# source code is converted into IL (Intermediate Language).**

The C# compiler (`csc`) checks things it can determine before the program runs:

- Syntax errors
- Type compatibility
- Method overload resolution
- Accessibility (`private`, `public`, etc.)
- Definite assignment
- Generic constraints
- Many language rules

Example:

```csharp
int x = "hello";
```

This fails at **compile time** because `"hello"` is a `string`, not an `int`.

You won't get an executable program from this code until the error is fixed.

## Runtime

**Runtime = when the compiled program is actually executing.**

The .NET runtime/CLR is responsible for things such as:

- Executing IL
- JIT compilation to machine code
- Memory management / GC
- Exceptions
- Thread management
- Loading assemblies
- Type checking for some operations
- Dynamic dispatch

Example:

```csharp
int x = 10;
int y = 0;

int result = x / y;
```

This **compiles successfully**, but when execution reaches the division:

```text
DivideByZeroException
```

That's a **runtime error**.

## Important interview distinction

> **What is checked at compile time vs runtime?**

| Compile time              | Runtime                     |
| ------------------------- | --------------------------- |
| C# → IL                   | IL → machine code via JIT   |
| Syntax checking           | Program execution           |
| Static type checking      | Runtime behavior            |
| Overload resolution       | Exceptions                  |
| Accessibility checks      | GC                          |
| Generic constraint checks | Dynamic dispatch            |
| Many language rules       | Actual values and resources |

## Example showing both

```csharp
Animal animal = new Dog();

animal.Run();
```

At **compile time**, the compiler asks:

> "Does `Animal` have a `Run()` method?"

If not → compilation error.

At **runtime**, if `Run()` is virtual, the CLR determines which override should actually execute:

```csharp
class Animal
{
    public virtual void Run() => Console.WriteLine("Animal");
}

class Dog : Animal
{
    public override void Run() => Console.WriteLine("Dog");
}
```

```csharp
Animal animal = new Dog();
animal.Run(); // Dog
```

This is the important distinction between **static/compile-time knowledge** and **runtime behavior**.

## One more important concept: `dynamic`

This is where the distinction becomes especially clear:

```csharp
dynamic value = 10;

value.SomeMethod();
```

The compiler doesn't perform the normal static member check on `SomeMethod()`.

The operation is resolved **at runtime**.

So:

```csharp
object x = 10;
x.SomeMethod();       // ❌ compile-time error

dynamic y = 10;
y.SomeMethod();       // ✅ compiles, ❌ runtime error
```

This is why `dynamic` moves some type checking from **compile time → runtime**.

# Interview Tips

> **Compile time is when the C# compiler analyzes source code and produces IL, performing static checks such as type checking and overload resolution. Runtime is when the CLR executes that IL, including JIT compilation, memory management, exception handling, and runtime dispatch. Some features, such as `dynamic`, deliberately move checks from compile time to runtime.**
