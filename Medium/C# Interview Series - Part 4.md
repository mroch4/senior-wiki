# C# Interview Series — Part 4 (Finale): Senior-Level Signals (DI, Design Patterns & Production-Ready Code)

[source](https://medium.com/@thecurlybrace/c-interview-series-part-4-finale-senior-level-signals-di-design-patterns-production-ready-d46195d05c08)

Now we’re at the part of the interview where the panel leans back, makes eye contact, and starts asking questions like:

“Walk me through how you’d structure this.” “What pattern would you use here?” “How would you test that?”

These aren’t trick questions. They’re the questions that reveal how you think when no one’s watching the code. Get them right and the conversation turns from interview to negotiation.

## 1. What is Dependency Injection?

Instead of a class **creating** its dependencies, the dependencies are **passed in** — usually through the constructor.

Tightly coupled:

```csharp
public class OrderService {
    private SqlRepo _repo = new SqlRepo();
}
```

✅ DI

```csharp
public class OrderService {
    private readonly IRepo _repo;
    public OrderService(IRepo repo) => _repo = repo;
}
```

**Why it matters:**

- Testable (inject a mock)
- Swappable (Sql → Mongo without touching `OrderService`)
- Honest (constructor declares what the class _actually_ needs)

In .NET, the container is built in:

```csharp
services.AddScoped<IRepo, SqlRepo>();
services.AddSingleton<ICache, RedisCache>();
services.AddTransient<IEmailSender, SmtpSender>();
```

The 3 lifetimes you must know cold:

- **Singleton** — one instance for the whole app
- **Scoped** — one per HTTP request
- **Transient** — new instance every time

---

## 2. What is Inversion of Control?

DI is the mechanism. **IoC is the principle behind it.**

> Don’t call us, we’ll call you.

Instead of your code reaching out to grab what it needs, the framework hands it to you when needed.

You don’t `new` up a controller in ASP.NET - the framework does, and injects what you asked for. You don't write `Main()` that calls your code - `Program.cs` configures the host and the host calls your code.

**What to say:** _'IoC is the broader principle. DI is one way to implement it. Other ways include service locators, events, and the template method pattern.'_

---

## 3. Strategy pattern — what and when?

When you have **multiple ways to do the same thing**, and you want to swap between them at runtime.

```csharp
interface IPaymentStrategy {
    void Pay(decimal amount);
}
```

```csharp
class StripeStrategy : IPaymentStrategy { /* ... */ }
class PayPalStrategy : IPaymentStrategy { /* ... */ }

class Checkout {
    private readonly IPaymentStrategy _strategy;
    public Checkout(IPaymentStrategy strategy) => _strategy = strategy;
    public void Process(decimal amount) => _strategy.Pay(amount);
}
```

**Where you’ve already used it:** Sorting comparers (`IComparer<T>`), validation rules, pricing engines, anywhere `if-else` chains start growing.

---

## 4. Template Method pattern — what and when?

When the **algorithm is the same**, but specific steps differ between subclasses.

```csharp
abstract class ReportGenerator {
    public void Generate() {              // template — the algorithm
        var data = FetchData();
        var formatted = Format(data);
        Save(formatted);
    }
    protected abstract object FetchData();
    protected abstract string Format(object data);
    protected abstract void Save(string content);
}
```

**When to use it:** ETL pipelines, report generators, workflow steps where the order is fixed but each step’s implementation varies.

---

## 5. Decorator pattern — what and when?

When you need to add behavior to an object without changing its class.

```csharp
interface ILogger { void Log(string msg); }
```

```csharp
class FileLogger : ILogger { /* writes to file */ }

class TimestampedLogger : ILogger {
    private readonly ILogger _inner;

    public TimestampedLogger(ILogger inner) => _inner = inner;

    public void Log(string msg) =>
        _inner.Log($"[{DateTime.UtcNow:O}] {msg}");
}

// usage — stack as many as you want
ILogger logger = new TimestampedLogger(new FileLogger());
```

**Where you’ve seen it:** ASP.NET Core middleware (`app.UseAuthentication()` decorates the pipeline), HTTP `DelegatingHandler`, EF Core interceptors.

---

## 6. Observer pattern — what and when?

When one object’s state change should **notify many others** without them being tightly coupled.

C# bakes this into the language via events:

```csharp
class Stock {
    public event Action<decimal> PriceChanged;

    public void SetPrice(decimal p) => PriceChanged?.Invoke(p);
}
```

```csharp
var stock = new Stock();
stock.PriceChanged += p => Console.WriteLine($"Trader: {p}");
stock.PriceChanged += p => Logger.Log($"Audit: {p}");
stock.PriceChanged += p => Notifier.Push(p);
```

One change → multiple subscribers react. No tight coupling.

**Modern alternative:** `IObservable<T>` (Reactive Extensions) or message queues (Azure Service Bus, RabbitMQ) for cross-process observers.

---

## 7. What are mocks (and why do you need them)?

A **mock** is a fake object that pretends to be a real one for testing.

```csharp
var mockRepo = new Mock<IRepo>();
mockRepo.Setup(r => r.GetUser(1)).Returns(new User("Deep"));
var service = new UserService(mockRepo.Object);

var result = service.GetGreeting(1);
Assert.Equal("Hello, Deep!", result);
mockRepo.Verify(r => r.GetUser(1), Times.Once);
```

**Why interviewers ask:** Mocking is only possible when your dependencies are **injected**. So asking about mocks is sneakily asking _'Do you actually understand DI?'_

**What to say:** _'I mock the boundaries - database, HTTP clients, queues, file systems - and let the rest of the logic run for real.'_

---

## 8. What are NuGet packages?

The **package manager for .NET**. Like npm for Node, pip for Python.

```
dotnet add package Serilog
dotnet add package AutoMapper
```

**What interviewers want you to know:**

- `PackageReference` in `.csproj` is the modern way (forget `packages.config`)
- You can host **private NuGet feeds** (Azure Artifacts, GitHub Packages, Nexus) for internal libraries
- Lock files (`packages.lock.json`) ensure reproducible builds

---

## 9. Debug vs Release builds — what changes?

Debug Release Optimizations Off On (JIT inlines, removes dead code) `DEBUG` symbol Defined Not defined `[Conditional("DEBUG")]` methods Run Stripped out PDB symbols Full Stripped or portable Performance Slower Faster

**Why interviewers ask:** They want to make sure you don’t ship Debug builds to production. (You’d be shocked how often this happens.)

**Bonus trap:** _'Have you ever shipped a bug that only happens in Release?'_ Common cause: relying on side effects in an expression that the optimizer eliminated.

---

## 10. What are preprocessor directives?

Instructions to the compiler, not the runtime.

```
#define FEATURE_X
```

```
#if FEATURE_X
    Console.WriteLine("New feature on");
#else
    Console.WriteLine("Old behavior");
#endif

#region Internal helpers
// folded in IDE
#endregion

#nullable enable    // turn on nullable reference types

#pragma warning disable CS8618  // suppress a specific warning
```

When you’ll use them:

- Conditional compilation across .NET versions (`#if NET8_0_OR_GREATER`)
- Platform-specific code (`#if WINDOWS`)
- Toggling debug-only blocks

---

## 11. What’s the difference between unit, integration, and end-to-end tests?

- **Unit** — one class/method, all dependencies mocked. Fast (ms).
- **Integration** — multiple components together (controller + service + DB). Slower (seconds).
- **E2E** — the whole app, from HTTP request to database write. Slow (10s of seconds).

**The test pyramid:** Many unit tests, fewer integration, very few E2E.

**What to say:** _'Unit tests catch logic bugs fast. Integration tests catch wiring bugs. E2E tests prove the whole thing actually works. You need all three, in that ratio.'_

---

## 12. Bonus: How to answer “Tell me about a hard bug you fixed”

This question alone has decided more offers than any technical question on this list. The structure that works:

1. Context — What system, why it mattered. (1 sentence)
2. The symptom — What was breaking, how it surfaced. (1 sentence)
3. The investigation — What you tried, what didn’t work, what you learned. (2–3 sentences)
4. The root cause — What it actually was. (1 sentence)
5. The fix — What you changed. (1 sentence)
6. The lesson — What you’d do differently. (1 sentence)

Example:

> On our healthcare platform, we had transactions silently failing in production but not staging. Symptom: ~2% of insurance lookups returned empty data. Tried logging - nothing useful. Tried reproducing in staging - couldn’t. Eventually traced it to a SQL collation difference between environments; staging was case-insensitive, prod wasn’t, and member names with diacritics didn’t match. Fixed by normalizing inputs at the API boundary. Lesson: lock down environment parity in your infra-as-code, not your README.

That’s 60 seconds. Specific, structured, ends with a lesson. **Practice this story for your own work** — every interview asks some version of it.

---

# The thing nobody tells you about senior-level interviews

The questions in this part are not really about C#. They’re about whether you can be trusted to make architectural decisions.

Notice the shift:

- Part 1: _'Do you know the syntax?'_
- Part 2: _'Do you know the language?'_
- Part 3: _'Do you understand the runtime?'_
- Part 4: _'Can we trust you to design something?'_

When you answer questions in this part, **speak from your actual production experience**. Not textbook definitions. Pull examples from real systems you’ve built. Mention real trade-offs you faced.

'_We used Strategy pattern for payment processing because we needed to swap providers per region'_ hits 10x harder than _'Strategy pattern lets you swap algorithms at runtime.'_
