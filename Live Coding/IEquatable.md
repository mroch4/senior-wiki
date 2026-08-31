# IEquatable for CompositeKey

```csharp
public class CompositeKey : IEquatable<CompositeKey>
{
    public Int64 Id { get; }
    public String? Name { get; }

    public MyKey(Int64 id, String? name)
    {
        Id = id;
        Name = name;
    }

    public bool Equals(CompositeKey? other)
    {
        return other != null &&
               Id == other.Id &&
               Name == other.Name;
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as CompositeKey);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Id, Name);
    }
}
```

`IEquatable<MyKey>` provides the typed equality logic, while
`GetHashCode()` provides the hash used for efficient lookup.

So think:

```text
GetHashCode()  → Which bucket should I look in?
IEquatable<T>  → Are these two objects actually equal?
```

For example:

```csharp
var a = new CompositeKey(10, "PDF");
var b = new CompositeKey(10, "PDF");

Console.WriteLine(a.Equals(b)); // true
Console.WriteLine(a.GetHashCode() == b.GetHashCode()); // true
```

# Interview tip

> `IEquatable<T>` allows a type to define efficient, strongly typed equality without relying on `object` comparisons. It's particularly useful for value-like types used as keys in `Dictionary` or elements in `HashSet`.
