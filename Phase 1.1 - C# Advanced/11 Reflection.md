# Reflection

## Table of content

1. [What is Reflection?](#what-is-reflection)
2. [Why does Reflection exist?](#why-does-reflection-exist)
3. [What information can Reflection read?](#what-information-can-reflection-read)
4. [Example class](#example-class)
   - [Getting Type information](#getting-type-information)
   - [Reading class name](#reading-class-name)
   - [Reading properties](#reading-properties)
   - [Reading methods](#reading-methods)
   - [Reading fields](#reading-fields)
   - [Reading constructors](#reading-constructors)
   - [Creating an object dynamically](#creating-an-object-dynamically)
   - [Setting property dynamically](#setting-property-dynamically)
   - [Reading property value](#reading-property-value)
   - [Calling methods dynamically](#calling-methods-dynamically)
   - [Reading custom attributes](#reading-custom-attributes)
5. [Loading assemblies dynamically](#loading-assemblies-dynamically)
6. [Real-world examples](#real-world-examples)
   - [Dependency Injection](#dependency-injection)
   - [Entity Framework Core](#entity-framework-core)
   - [ASP.NET Core Model Binding](#aspnet-core-model-binding)
   - [JSON Serialization](#json-serialization)
   - [AutoMapper](#automapper)
   - [Swagger / OpenAPI](#swagger--openapi)
7. [Why Reflection is slower](#why-reflection-is-slower)
8. [Can Reflection access private members?](#can-reflection-access-private-members)
9. [Advantages](#advantages)
10. [Disadvantages](#disadvantages)
11. [Reflection vs Dynamic](#reflection-vs-dynamic)
12. [Reflection vs Source Generators](#reflection-vs-source-generators)
13. [Interview Tips](#interview-tips)

## What is Reflection?

> Reflection is the ability of a .NET application to examine metadata about assemblies, types, methods, properties, and fields at runtime, and optionally create objects or invoke members dynamically. It allows your code to **inspect, analyze, and interact with types at runtime** without knowing them at compile time.

## Why does Reflection exist?

Normally C# is strongly typed.

```csharp
Person p = new Person();
p.Name = "John";
p.Print();
```

The compiler already knows everything.

But sometimes:

- You don't know the type beforehand.
- You're loading plugins.
- You're building a serializer.
- You're creating a dependency injection container.
- You're implementing an ORM.

Then Reflection becomes useful.

## What information can Reflection read?

Reflection can inspect almost everything.

```
Assembly
 ├── Namespace
 ├── Types (classes, interfaces, structs)
 │      ├── Constructors
 │      ├── Methods
 │      ├── Properties
 │      ├── Fields
 │      ├── Events
 │      ├── Attributes
 │      └── Base classes
```

## Example class

```csharp
public class Person
{
    public string Name { get; set; }

    public int Age { get; set; }

    public void SayHello()
    {
        Console.WriteLine($"Hello {Name}");
    }
}
```

### Getting Type information

```csharp
Type type = typeof(Person);
```

or

```csharp
Person p = new Person();

Type type = p.GetType();
```

Now `type` contains metadata describing the class.

### Reading class name

```csharp
Console.WriteLine(type.Name);
```

Output:

```
Person
```

### Reading properties

```csharp
foreach(var property in type.GetProperties())
{
    Console.WriteLine(property.Name);
}
```

Output:

```
Name
Age
```

### Reading methods

```csharp
foreach(var method in type.GetMethods())
{
    Console.WriteLine(method.Name);
}
```

Output includes:

```
SayHello
ToString
Equals
GetHashCode
```

Notice **inherited methods** also appear.

### Reading fields

```csharp
type.GetFields();
```

### Reading constructors

```csharp
type.GetConstructors();
```

### Creating an object dynamically

Instead of:

```csharp
var person = new Person();
```

Reflection allows:

```csharp
Type type = typeof(Person);

object person = Activator.CreateInstance(type);
```

The compiler didn't know the type - reflection created it anyway.

### Setting property dynamically

```csharp
PropertyInfo property = type.GetProperty("Name");

property.SetValue(person, "John");
```

Equivalent to

```csharp
person.Name = "John";
```

### Reading property value

```csharp
var value = property.GetValue(person);

Console.WriteLine(value);
```

Output:

```
John
```

### Calling methods dynamically

```csharp
MethodInfo method = type.GetMethod("SayHello");

method.Invoke(person, null);
```

Output:

```
Hello John
```

The compiler never knew about `SayHello()`.

Reflection discovered it at runtime.

### Reading custom attributes

Reflection can inspect metadata annotations:

```csharp
[Obsolete]
public class Person
{
}
```

Then:

```csharp
var attributes = type.GetCustomAttributes(false);

foreach(var attr in attributes)
{
    Console.WriteLine(attr.GetType().Name);
}
```

Output:

```
ObsoleteAttribute
```

This is heavily used by:

- ASP.NET Core
- EF Core
- AutoMapper
- Swagger
- Validation

## Loading assemblies dynamically

```csharp
Assembly assembly = Assembly.LoadFrom("Plugin.dll");
```

Now you can inspect everything inside that DLL.

This is how plugin systems work.

## Real-world examples

### Dependency Injection

```csharp
builder.Services.AddScoped<IMailService, MailService>();
```

The DI container scans assemblies, discovers constructors, and creates object graphs using Reflection.

### Entity Framework Core

```csharp
public class Product
{
    public int Id { get; set; }
}
```

EF Core discovers:

- properties
- relationships
- keys
- attributes

using Reflection during model building.

### ASP.NET Core Model Binding

```csharp
public IActionResult Save(Product product)
```

ASP.NET Core:

- creates `Product`
- finds properties
- assigns values from HTTP requests

using Reflection.

### JSON Serialization

```csharp
JsonSerializer.Serialize(person);
```

The serializer:

- inspects properties
- reads values
- converts them to JSON

Reflection (or generated metadata in newer optimizations) makes this possible.

### AutoMapper

```csharp
CreateMap<User, UserDto>();
```

AutoMapper inspects source and destination properties and maps matching members.

### Swagger / OpenAPI

Swagger scans your controllers:

```csharp
[HttpGet]
public Product Get()
```

It generates API documentation by inspecting types, routes, parameters, and attributes via Reflection.

## Why Reflection is slower

Normally:

```
person.Name
```

The compiler knows the exact memory location.

With Reflection:

```
Find property called "Name"
 |
Check metadata
 |
Locate getter
 |
Invoke getter
 |
Return value
```

There are additional metadata lookups and dynamic invocations, making Reflection significantly slower than direct access.

## Can Reflection access private members?

Yes.

```csharp
var field = type.GetField("_name", BindingFlags.NonPublic | BindingFlags.Instance);
```

Then

```csharp
field.SetValue(person, "John");
```

This bypasses normal access restrictions, which is powerful but should be used **carefully**.

## Advantages

- Dynamic object creation.
- Plugin architectures.
- Generic libraries.
- Serialization.
- ORMs.
- Dependency Injection.
- Runtime inspection.
- Attribute processing.

## Disadvantages

- Slower than direct code.
- More memory overhead.
- Less compile-time safety (errors appear at runtime).
- Harder to debug.
- Can access private members, potentially breaking encapsulation.

## Reflection vs Dynamic

```csharp
dynamic person = GetPerson();

person.SayHello();
```

- `dynamic` defers member resolution to runtime, giving syntax similar to normal method calls.
- Reflection explicitly inspects metadata (`Type`, `MethodInfo`, `PropertyInfo`) and invokes members.
- Many `dynamic` scenarios are implemented internally using reflection or the Dynamic Language Runtime (DLR), but the concepts are different.

# Reflection vs Source Generators

Modern .NET increasingly prefers **source generators** or **compile-time generated metadata** where possible.

| Reflection                    | Source Generator             |
| ----------------------------- | ---------------------------- |
| Runtime                       | Compile time                 |
| Slower                        | Faster                       |
| Flexible                      | Less dynamic                 |
| Discovers metadata at runtime | Generates code ahead of time |

For example, `System.Text.Json` can use source generation to avoid reflection during serialization, improving startup time and reducing allocations—especially important for Native AOT.

# Interview Tips

What is Reflection?

> Reflection is the ability to inspect and interact with assemblies, types, methods, properties, and attributes at runtime.

Why is Reflection slower?

Because it performs metadata lookups and dynamic invocation at runtime instead of direct, compile-time-bound member access.

> Where is Reflection used?

- ASP.NET Core model binding
- Dependency Injection
- Entity Framework Core
- JSON serialization
- AutoMapper
- Swagger/OpenAPI
- Plugin architectures
- Test frameworks (e.g., discovering test methods)

> When should you avoid Reflection?

Avoid it in performance-critical paths (tight loops, high-throughput code). If repeated reflection is necessary, cache `Type`, `MethodInfo`, or `PropertyInfo` objects, or consider compiled delegates or source generators where appropriate.
