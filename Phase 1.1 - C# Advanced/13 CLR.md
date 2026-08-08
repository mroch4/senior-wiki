# CLR

## Table of content

1. [What is CLR?](#what-is-clr)
2. [What does the CLR do?](#what-does-the-clr-do)
   - [JIT (Just-In-Time) Compilation](#1-jit-just-in-time-compilation)
   - [Memory Management](#2-memory-management)
   - [Garbage Collection](#3-garbage-collection)
   - [Exception Handling](#4-exception-handling)
   - [Type Safety](#5-type-safety)
   - [Security](#6-security)
   - [Thread Management](#7-thread-management)
   - [Assembly Loading](#8-assembly-loading)
   - [Metadata & Reflection](#9-metadata--reflection)
3. [Managed vs Unmanaged Code](#managed-vs-unmanaged-code)
4. [CLR Execution Lifecycle](#clr-execution-lifecycle)
5. [Interview Tips](#interview-tips)

## What is CLR?

> **CLR (Common Language Runtime)** is the execution engine of .NET. Think of it as the **virtual machine** that runs your .NET applications, similar to the JVM in Java.

When you write C# code, it doesn't run directly on the CPU.

```
C# Code
 ↓
C# Compiler (Roslyn)
 ↓
IL (Intermediate Language)
 ↓
CLR
 ↓
Machine Code (JIT Compilation)
 ↓
CPU
```

## What does the CLR do?

### 1. JIT (Just-In-Time) Compilation

Your C# code is first compiled into **Intermediate Language (IL)**.

Example:

```csharp
int Add(int a, int b)
{
    return a + b;
}
```

becomes IL.

When the method is called for the first time:

```
IL
 ↓
JIT Compiler
 ↓
Native Machine Code
```

The native code is cached in memory, so subsequent calls don't need recompilation.

### 2. Memory Management

The CLR allocates memory for objects on the **managed heap**.

```csharp
var person = new Person();
```

Instead of you manually freeing memory:

```cpp
delete person;
```

the CLR automatically cleans it up using the **Garbage Collector (GC)**.

Benefits:

- Prevents many memory leaks
- Prevents dangling pointers
- Eliminates manual memory management

### 3. Garbage Collection

The CLR periodically checks:

> "Is this object still reachable?"

If not:

```
Object
 ↓
No references
 ↓
Marked for collection
 ↓
Memory reclaimed
```

The GC uses **generations (0, 1, and 2)** to optimize collection frequency.

### 4. Exception Handling

The CLR provides a unified exception model.

```csharp
try
{
    ...
}
catch(Exception ex)
{
}
```

Unhandled exceptions propagate up the call stack. If none are caught, the CLR terminates the process (unless handled by a host or global exception handler).

### 5. Type Safety

The CLR verifies that code is type-safe.

For example:

```csharp
string s = "Hello";
int x = s;
```

This won't compile, and even at runtime the CLR prevents invalid memory access that unmanaged languages may permit.

### 6. Security

The CLR isolates applications and validates assemblies before execution. Older versions of .NET Framework also supported **Code Access Security (CAS)**, but CAS is not used in modern .NET.

### 7. Thread Management

The CLR works with the operating system to manage threads.

Examples:

```csharp
Task.Run(...)
```

```csharp
Parallel.ForEach(...)
```

```csharp
async/await
```

The CLR provides services such as:

- Thread pool management
- Synchronization primitives
- Execution contexts

### 8. Assembly Loading

The CLR loads assemblies (`.dll` and `.exe`) only when needed.

```
MyApp.exe
 ↓
Business.dll
 ↓
Data.dll
```

It resolves dependencies, loads metadata, and prepares types for execution.

### 9. Metadata & Reflection

Every .NET assembly contains metadata describing its types.

This enables reflection:

```csharp
Type t = typeof(Person);

foreach (var property in t.GetProperties())
{
    Console.WriteLine(property.Name);
}
```

Frameworks such as ASP.NET Core, dependency injection containers, serializers, and ORMs like EF Core rely heavily on this metadata.

## Managed vs Unmanaged Code

| Managed Code              | Unmanaged Code                       |
| ------------------------- | ------------------------------------ |
| Runs under the CLR        | Runs directly on the OS              |
| Garbage collected         | Manual memory management             |
| Type safe                 | Can access raw pointers              |
| JIT compiled              | Compiled directly to machine code    |
| Exceptions managed by CLR | Platform-specific exception handling |

Examples:

Managed:

```csharp
Console.WriteLine("Hello");
```

Unmanaged:

- C
- C++
- Native Windows APIs

## CLR Execution Lifecycle

```
Write C#
 ↓
Compile to IL
 ↓
CLR loads assembly
 ↓
CLR verifies IL
 ↓
JIT compiles methods
 ↓
Native machine code executes
 ↓
CLR manages memory
 ↓
Garbage Collector frees unused objects

```

# Interview Tips

> "The CLR, or Common Language Runtime, is the .NET execution engine. It loads assemblies, verifies IL, JIT-compiles it into native machine code, manages memory through the garbage collector, handles exceptions, provides type safety, manages threads, and supports features like reflection and assembly loading. It abstracts many low-level runtime concerns so developers can focus on application logic."

A common follow-up question is: **"What's the difference between the CLR and the .NET runtime?"**

A good answer is:

- **CLR** is the execution engine responsible for running managed code (JIT, GC, exceptions, threading, etc.).
- **The .NET runtime** includes the CLR **plus** the Base Class Library (BCL), runtime libraries, and other infrastructure needed to run .NET applications.

In modern .NET, people often say "the .NET runtime" in conversation, but technically the **CLR is one component of that runtime**.
