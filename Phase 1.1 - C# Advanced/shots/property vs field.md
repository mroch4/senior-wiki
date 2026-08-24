# `property` vs `field`

In C#, **fields and properties both store/access data**, but they serve different purposes.

### Field

A **field is a variable directly declared inside a class/struct**.

```csharp
class Person
{
    private string _name;
    public int Age;
}
```

The field directly represents storage.

```csharp
var person = new Person();
person.Age = 30;
```

Usually fields are **private**:

```csharp
private string _name;
```

and exposed through properties.

### Property

A **property provides controlled access to a value**, using `get` and/or `set`.

```csharp
class Person
{
    public string Name { get; set; }
}
```

You use it like a field:

```csharp
person.Name = "John";
Console.WriteLine(person.Name);
```

But internally, the property can execute logic:

```csharp
public string Name
{
    get => _name;
    set
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException();

        _name = value;
    }
}
```

So the important distinction is:

> Field = storage

> Property = controlled access to a value

### Why properties are preferred for public API

Imagine you expose:

```csharp
public int Age;
```

Anyone can freely modify it:

```csharp
person.Age = -100;
```

With a property:

```csharp
public int Age
{
    get => _age;
    set
    {
        if (value < 0)
            throw new ArgumentException("Age cannot be negative.");

        _age = value;
    }
}
```

Now you control what happens when somebody reads or writes the value.

This is one reason properties are important for **encapsulation**.\

### Auto-property

Most of the time you don't need to explicitly create the backing field:

```csharp
public string Name { get; set; }
```

The compiler generates a hidden backing field for you.

Conceptually, it's similar to:

```csharp
private string _name;

public string Name
{
    get => _name;
    set => _name = value;
}
```

You normally don't need to care about that generated field.

### Properties can be read-only

Very common:

```csharp
public int Id { get; private set; }
```

Code outside the class can read it:

```csharp
var id = person.Id;
```

but cannot modify it:

```csharp
person.Id = 10; // ❌
```

Only the class itself can set it.

Or:

```csharp
public string Name { get; }
```

This can only be assigned during construction:

```csharp
public Person(string name)
{
    Name = name;
}
```

### Important interview distinction

A property **is not necessarily storage**.

For example:

```csharp
public string FullName => $"{FirstName} {LastName}";
```

There is no `FullName` field storing anything.

Every time you access:

```csharp
person.FullName
```

the getter executes and calculates the value.

You can even have:

```csharp
public bool IsAdult => Age >= 18;
```

So a property can represent a **calculated value**, whereas a field represents actual variable storage.

### Senior-level points to remember

|                                 | Field            | Property        |
| ------------------------------- | ---------------- | --------------- |
| Storage                         | Yes              | Not necessarily |
| `get`/`set`                     | ❌               | ✅              |
| Can contain logic               | ❌               | ✅              |
| Encapsulation                   | Poor when public | Good            |
| Typical visibility              | `private`        | `public`        |
| Can be calculated               | ❌               | ✅              |
| Can be used by serializers/ORMs | Depends          | Commonly        |
| Can be virtual/override         | ❌               | ✅              |

# Interview Tips

> A field is a variable that represents storage inside a type. A property provides an abstraction over accessing a value through getters and setters. Properties allow encapsulation, validation, calculated values, and controlled read/write access, so public data is generally exposed through properties rather than public fields.

One subtle point worth knowing for senior interviews: **properties are methods (`get_`/`set_`) at the IL level**, not simply variables with nicer syntax.
