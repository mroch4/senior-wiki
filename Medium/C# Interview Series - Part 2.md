# C# Interview Series — Part 2: The Second 15 Minutes (OOP, Collections & Modern C#)

[source](https://medium.com/@thecurlybrace/c-interview-series-part-2-the-second-15-minutes-oop-collections-modern-c-21b67960b98e)

Now we’re in the second 15 minutes. This is where interviews quietly split candidates into “promising” and “let’s wrap up early.” The questions here aren’t harder, but they’re deeper. The interviewer wants to know:

> Does this person understand collections, OOP, and modern C# well enough to design code — not just write it?

These are the 12 questions that answer that for them.

## 1. `Array` vs `List` vs `ArrayList` — when to use what?

- `Array` - fixed size, typed. Fast, lean.
- `List<T>` - resizable, typed, generic. **Use this 99% of the time.**
- `ArrayList` - resizable, untyped, boxes every value type. Legacy. Avoid.

```csharp
int[] a = new int[5];           // fixed size
List<int> l = new List<int>();  // grows as needed
ArrayList al = new ArrayList(); // accepts anything — and regrets it
al.Add(1); al.Add("two");       // compiles. cries at runtime.
```

**Why interviewers ask:** They want to see you reach for `List<T>` instinctively and know why `ArrayList` is bad — not just that _'we don't use it anymore.'_

---

## 2. What is a `Dictionary` — and how does `GetHashCode` make it work?

A `Dictionary<TKey, TValue>` gives you near-O(1) lookups by key. Behind the scenes:

1. It calls `GetHashCode()`on your key.
2. That hash decides which 'bucket' the value sits in.
3. Lookup goes straight to that bucket — no scanning the whole collection.

**The trap interviewers love:**

```csharp
public class Person {
    public string Name { get; set; }
}
```

```csharp
var dict = new Dictionary<Person, int>();
var p = new Person { Name = "Deep" };
dict[p] = 1;

// later...
dict[new Person { Name = "Deep" }]; // 💥 KeyNotFoundException
```

Two `Person` objects with the same name aren't equal by default - they have different hash codes. If you want them to be, override `Equals` and `GetHashCode` together.

---

## 3. `Tuple` vs `ValueTuple` — what's the difference?

|            | `Tuple`                     | `ValueTuple`                  |
| ---------- | --------------------------- | ----------------------------- |
| Type       | Reference type (`class`)    | Value type (`struct`)         |
| Mutability | Immutable                   | Mutable                       |
| Access     | `.Item1`, `.Item2`          | named: `(int sum, int count)` |
| Syntax     | `Tuple.Create(1, 2) (1, 2)` |                               |

Old, awkward:

```csharp
Tuple<int, int> result = Tuple.Create(10, 5);
Console.WriteLine(result.Item1);
```

Modern:

```csharp
(int sum, int count) result = (10, 5);
Console.WriteLine(result.sum); // way cleaner
```

**What to say:** _'Use `ValueTuple` for return values bundling 2–4 things. Anything bigger, define a record.'_

---

## 4. `abstract class` vs `interface` — the modern answer

Pre-C# 8, the rule was easy: interfaces had no implementation, abstract classes did. C# 8 broke that rule by adding **default interface implementations**.

So today:

- `interface` → contract. Can now have default methods, but no state (no fields).
- `abstract class` → partial implementation + state. Use when subclasses share both behavior _and_ data.

```csharp
interface ILogger {
    void Log(string msg);
    void LogError(string msg) => Log("ERROR: " + msg); // C# 8+ default
}
```

```csharp
abstract class Repository {
    protected DbContext Db; // state — only abstract class can have this
    public abstract void Save();
}
```

**Rule of thumb:** Default to interfaces. Reach for abstract classes only when you genuinely need shared state.

---

## 5. What are records (and record structs)?

Records (C# 9) are reference types built for immutability and value-based equality.

```csharp
public record Person(string Name, int Age);
```

```csharp
var p1 = new Person("Deep", 28);
var p2 = new Person("Deep", 28);
Console.WriteLine(p1 == p2); // true ✅ compares VALUES, not references
```

Compare that with a regular class - `p1 == p2` would be `false` unless you override `Equals` and `GetHashCode` manually. Records do all of it for you.

**Record structs** (C# 10) are the same idea, but value types - no heap allocation. Use them for small, hot, immutable data.

**What to say in interviews:** _'Records are perfect for DTOs, value objects, and anything that’s data without behavior. They eliminate ~30 lines of `Equals`/`GetHashCode`/`ToString` boilerplate.'_

---

## 6. What are anonymous types?

A way to create a lightweight, read-only object inline - no class definition needed.

```csharp
var person = new { Name = "Deep", Age = 28 };
Console.WriteLine(person.Name);
```

Where you’ll actually use them: LINQ projections.

```csharp
var summary = orders.Select(o => new { o.Id, o.Total });
```

**Catch:** They can’t leave the method (no named type). For anything that crosses a method boundary, use a `record`.

---

## 7. What are indexers?

They let your class be accessed like an array.

```csharp
public class Inventory {
    private Dictionary<string, int> _stock = new();

    public int this[string sku] {
        get => _stock.GetValueOrDefault(sku, 0);
        set => _stock[sku] = value;
    }
}

var inv = new Inventory();
inv["SKU-123"] = 50;
Console.WriteLine(inv["SKU-123"]); // 50
```

Small feature, but a great signal of polish when shown in a live coding round.

---

## 8. What is operator overloading?

Defining what operators like `+`, `==`, `<` mean for your custom types.

```csharp
public record Money(decimal Amount, string Currency) {
    public static Money operator +(Money a, Money b) {
        if (a.Currency != b.Currency) throw new InvalidOperationException();
        return a with { Amount = a.Amount + b.Amount };
    }
}
```

```csharp
var price = new Money(100, "USD") + new Money(50, "USD"); // USD 150
```

**When to use it:** Domain types like `Money`, `Vector`, `Distance`. **When to avoid it:** Anything where the meaning isn't obvious to the next developer.

---

## 9. What is deconstruction?

Pulling fields out of an object in one line.

```csharp
var person = new Person("Deep", 28);
var (name, age) = person; // deconstruction
```

```csharp
Console.WriteLine($"{name} is {age}");
```

`Record` support this automatically. For your own classes, define a `Deconstruct` method.

**What interviewers love:** Pairing it with pattern matching in a switch expression — instant senior-dev vibes.

---

## 10. What are immutable types and why do they matter?

An immutable type **cannot be modified after creation**. Every “change” returns a new instance.

```csharp
public record Point(int X, int Y);
```

```csharp
var p1 = new Point(1, 2);
var p2 = p1 with { X = 5 }; // new object — p1 untouched
```

**Why interviewers care:**

- Thread-safe by default (no shared mutable state = no race conditions)
- Easier to reason about
- Perfect for functional programming patterns

**Anything that crosses threads** — config objects, value types, events — should be immutable if you can swing it.

---

## 11. Cohesion vs coupling

- **Cohesion** — how _focused_ a class is. High cohesion = does one thing well.
- **Coupling** — how _dependent_ classes are on each other. Low coupling = changes don’t ripple.

> Goal: high cohesion, low coupling.

LOW cohesion - does everything:

```csharp
class UserManager {
    public void Save();
    public void SendEmail();
    public void ValidatePassword();
    public void GenerateReport();
}
```

HIGH cohesion - each class does ONE thing:

```csharp
class UserRepository { public void Save(); }
class EmailService   { public void Send(); }
class PasswordValidator { public bool Validate(); }
```

The interviewer’s follow-up: “How do you reduce coupling?” → Dependency Injection.

---

## 12. Composition over inheritance — what’s the principle?

Instead of inheriting behavior:
`class Dog : Animal`

**inject** the behavior you need:
`class Dog { IMover mover; ISounder sounder; }`

INHERITANCE - rigid, deep hierarchies:

```csharp
class Animal { void Move() {} }
class Bird : Animal { void Fly() {} }
class Penguin : Bird { } // 😬 Penguin inherits Fly()
```

COMPOSITION - flexible:

```csharp
class Penguin {
    IMover mover = new SwimMover();
    ISounder sounder = new SquawkSounder();
}
```

**What to say:** _'Inheritance models ‘is-a’, composition models ‘has-a’. When the hierarchy starts feeling forced, switch to composition.'_

# The pattern that ties this whole part together

Notice something? Almost every “modern” answer in this list - records, default interface methods, deconstruction, pattern matching - exists to make C# code less boilerplate and more declarative.

Interviewers in 2026 aren’t just testing if you know C#. They’re testing if you write C# like it’s 2026 or like it’s 2014.

Use modern syntax in your live coding rounds. It costs nothing and signals everything.
