# IL (Intermediate Language)

## Intermediate Language (IL)

**Intermediate Language (IL)**, also called **CIL — Common Intermediate Language**, is the code produced when C# is compiled.

The simplified pipeline is:

```
C# source code
 ↓
C# Compiler
 ↓
IL / CIL + metadata
 ↓
.NET Runtime / CLR
 ↓
JIT compiler
 ↓
Native machine code
 ↓
CPU
```

## Example

You write:

```csharp
int Add(int a, int b)
{
    return a + b;
}
```

The C# compiler does **not normally compile this directly into CPU machine code**. It produces IL conceptually similar to:

```text
ldarg.1        // Load argument a
ldarg.2        // Load argument b
add            // Add them
ret            // Return
```

This IL is **platform-independent**. The same compiled `.NET` assembly can contain IL that runs on different architectures, provided an appropriate .NET runtime exists.

## What happens at runtime?

When the program starts:

1. The CLR loads the assembly.
2. It reads the **IL and metadata**.
3. The **JIT (Just-In-Time) compiler** converts needed IL into **native machine code**.
4. The CPU executes that native code.

For example:

```
MyApp.dll
│
├── IL
├── Metadata
└── Resources
        ↓ Runtime
JIT compiler
        ↓
x64 / ARM64 machine code
        ↓
CPU
```

## Why use IL?

IL gives .NET several advantages:

- **Language interoperability** — C#, F#, VB.NET, etc. can compile to the same underlying representation.
- **Portability** — IL can be JIT-compiled for different CPU architectures.
- **Runtime optimizations** — the JIT knows information about the machine and runtime environment.
- **Metadata** — assemblies contain rich information about types, methods, properties, attributes, and more.

## Important distinction

> **C# is compiled at compile time into IL. IL is then compiled into native machine code by the JIT at runtime.**

This connects directly to your previous topic:

```text
Compile time:  C# → IL
Runtime:       IL → Native machine code → Execution
```

One advanced detail: .NET does **not always rely purely on JIT**. There are also approaches such as **ReadyToRun** and **Native AOT**, where native compilation can happen partly or entirely ahead of execution.
