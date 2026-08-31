# C# Interview Series — Part 3: Where Mid-Levels Trip Up (Memory, Exceptions & the .NET Runtime)

[source](https://medium.com/@thecurlybrace/c-interview-series-part-3-where-mid-levels-trip-up-memory-exceptions-the-net-runtime-3f35f37f4f64)

They start asking about what happens when your code runs. Memory. Threads. Exceptions. The Garbage Collector. The stuff most tutorials skip because it’s not “fun.”

But here’s the thing — these are the questions that separate the developer who knows C# from the one who can debug a production memory leak at 2 AM. And interviewers know it.

## 1. How does the Garbage Collector decide what to remove?

Simple rule: **if no one is referencing an object, it goes.**

The GC starts from “roots” - local variables on the stack, static fields, CPU registers - and walks every reference. Whatever it can reach is **live**. Everything else is garbage. Collected.

```csharp
{
    var person = new Person("Deep"); // referenced
    // ...
} // 'person' goes out of scope. Now unreachable. Eligible for GC.
```

**What to say:** _'Mark-and-sweep. It marks everything reachable from roots, then sweeps the rest.'_

---

## 2. What are GC generations?

The GC divides the heap into 3 **generations** as an optimization.

- **Gen 0** — brand-new objects. Most die young.
- **Gen 1** — survived one GC pass. A buffer.
- **Gen 2** — long-lived objects (caches, singletons).

```csharp
new Customer() → Gen 0
survives GC → Gen 1
survives GC → Gen 2
```

**Why this matters:** Gen 0 collections are cheap — small heap, fast scan. Gen 2 collections are _expensive_ — full heap walk. Most GC time is spent in Gen 0, which is exactly what you want.

**Interviewer’s follow-up:** _'What’s the Large Object Heap (LOH)?'_ → A separate heap for objects ≥ 85 KB. Collected with Gen 2. Notoriously hard to compact. **Avoid allocating large objects in hot paths.**

---

## 3. `Dispose` vs `Finalize` — what's the difference?

|              | `Dispose`                   | `Finalize` |
| ------------ | --------------------------- | ---------- |
| Who calls it | You (or using)              | The GC     |
| When         | Right now                   | Eventually |
| Predictable  | Yes                         | No         |
| Purpose      | Release unmanaged resources | Safety net |

```csharp
public class FileWriter : IDisposable {
    private FileStream _stream;

    public void Dispose() {
        _stream?.Dispose();
        GC.SuppressFinalize(this);
    }
    ~FileWriter() => Dispose(); // safety net if Dispose() forgotten
}
```

**What to say:** _'Always prefer `Dispose` via a `using` block. Finalizers are a safety net for resources I really can't afford to leak — but they make GC slower, so avoid them when possible.'_

---

## 4. What’s a `delegate` ?

A `delegate` is a **variable that holds a method.**

```csharp
public delegate int MathOp(int a, int b);
```

```csharp
MathOp add = (x, y) => x + y;
Console.WriteLine(add(2, 3)); // 5
```

You rarely declare your own - `Func<>` and `Action<>` cover 99% of cases.

- `Func<int, int, int>` → takes 2 ints, returns int
- `Action<string>` → takes string, returns nothing
- `Predicate<int>` → takes int, returns bool

**Where you’ll meet them:** Every LINQ method. Every event handler. Every callback.

---

## 5. What are events (and how are they different from delegates)?

An **event** is a delegate with safety rails. It restricts the outside world to only `+=` and `-=`. They can't invoke it, reassign it, or clear it.

```csharp
public class OrderService {
    public event Action<Order> OrderPlaced;

    public void Place(Order o) {
        // ... save order ...
        OrderPlaced?.Invoke(o); // raise the event
    }
}
```

Subscriber:

```csharp
orderService.OrderPlaced += o => Console.WriteLine($"Order {o.Id} placed");
```

**Why interviewers ask:** Events are the foundation of the observer pattern, which is everywhere in .NET — UI, SignalR, EF Core change tracking, you name it.

---

## 6. Why is `catch(Exception)` almost always a bad idea?

Because it swallows bugs.

Don't:

```csharp
try {
    DoStuff();
} catch (Exception) {
    // now what? You don't even know what broke.
}
```

You catch what you can _handle_. If you can’t handle it, let it bubble up.

**Acceptable use cases for `catch(Exception)`:**

- Top-level handlers (Program.Main, ASP.NET middleware) — to log and return a friendly error
- Background jobs that must not crash the host
- And only when you **log the exception** and ideally **rethrow**

**What to say:** _'I catch specific exceptions. The only place I catch Exception is at the application boundary, and even then I log and rethrow.'_

---

## 7. What is reflection?

Reflection lets you **inspect and manipulate types at runtime** - read properties, invoke methods, find attributes, even create instances.

```csharp
Type t = typeof(Person);
foreach (var prop in t.GetProperties())
    Console.WriteLine(prop.Name);
```

```csharp
// invoke a method by name
var method = t.GetMethod("Greet");
method?.Invoke(person, null);
```

**Where you’ve already used it without knowing:**

- ASP.NET Core’s DI container
- Entity Framework
- JSON.NET / System.Text.Json
- xUnit / NUnit / MSTest
- AutoMapper

**Trade-off:** Powerful, but slow. Don’t use it in hot paths. Use source generators (C# 9+) instead when you need runtime-like behavior at compile-time speed.

---

## 8. What are attributes?

Metadata you attach to code, readable via reflection.

```csharp
[Obsolete("Use NewMethod instead")]
public void OldMethod() { }
```

```csharp
[Required]
[StringLength(50)]
public string Name { get; set; }
```

**Where they show up everywhere:**

- `[HttpGet]`, `[Authorize]` in ASP.NET Core
- `[Required]`, `[Range]` for validation
- `[Fact]`, `[Theory]` in xUnit
- `[Serializable]` for serialization

**What to say:** _'Attributes declare intent. Some frameworks read them at runtime via reflection, others at compile-time via source generators.'_

---

## 9. What is serialization?

Converting an object into a format that can be **stored or transmitted** - JSON, XML, binary, Protobuf - and rebuilt later.

```csharp
var person = new Person("Deep", 28);
string json = JsonSerializer.Serialize(person);
// {"Name":"Deep","Age":28}
var back = JsonSerializer.Deserialize<Person>(json);
```

**Real-world gotchas interviewers love to probe:**

- Circular references → `JsonSerializerOptions { ReferenceHandler = ReferenceHandler.IgnoreCycles }`
- DateTime timezone surprises → always use `DateTimeOffset` or UTC
- `Newtonsoft.Json` vs `System.Text.Json` → STJ is faster but stricter; some legacy code still needs Newtonsoft

**What to say:** _'I use `System.Text.Json` by default — it's faster, ships with .NET, and source-generates serializers for AOT scenarios.'_

---

## 10. What is caching, and what are the trade-offs?

Caching = storing the **result of expensive work** so you don’t redo it.

In-memory cache (single-instance apps):

```csharp
services.AddMemoryCache();
```

Distributed cache (multi-instance, microservices):

```csharp
services.AddStackExchangeRedisCache(opt => opt.Configuration = "...");
```

The 4 questions interviewers want you to ask about any cache:

1. **Where does it live?** (in-process / Redis / CDN)
2. **How long?** (TTL — too short = useless, too long = stale data)
3. **How is it invalidated?** (event-driven / write-through / sliding expiration)
4. **What’s the failure mode?** (does the app survive if the cache is down?)

**The famous line:** _'There are only two hard things in computer science: cache invalidation and naming things.'_ ~ Phil Karlton. Bring this up and watch interviewers smile.

---

## 11. What’s the difference between `ref`, `out`, and `in` parameters?

This one comes up more than you’d think.

```csharp
void Swap(ref int a, ref int b)    { /* both read AND written */ }
void TryParse(string s, out int x) { x = 0; /* must be assigned */ }
void Compute(in BigStruct s)       { /* read-only reference */ }
```

- `ref` → pass by reference, two-way
- `out` → must be assigned inside; caller doesn't have to initialize
- `in` → pass by reference, but read-only (perf optimization for large structs)

---

# The mental model that ties this part together

The C# language is a thin layer on top of a runtime — the CLR — that manages memory, exceptions, threading, and types for you. Every question in this part is really asking:

> Do you understand the cost of what you wrote?

- `new` allocates
- Catching exceptions has overhead
- Reflection is slow
- Caching has trade-offs
- Disposing matters

If you can articulate the _cost_ of your code — not just what it does — you’ve already moved from mid-level to senior in the interviewer’s mental model.
