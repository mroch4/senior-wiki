# Async/Await

## 1. `async void`

```csharp
private async void ProcessAsync()
{
    await service.ProcessAsync();
}
```

🚩 **Problem:** `async void` cannot be awaited and exceptions can't be observed normally.

Prefer:

```csharp
private async Task ProcessAsync()
{
    await service.ProcessAsync();
}
```

**Interview follow-up:**

> When is `async void` acceptable?

Answer: essentially **event handlers**.

---

## 2. Blocking on async

```csharp
public void Process()
{
    var result = service.GetDataAsync().Result;
}
```

or:

```csharp
service.GetDataAsync().Wait();
```

🚩 Potential **deadlock** in environments with a synchronization context, plus ThreadPool starvation.

Prefer:

```csharp
public async Task ProcessAsync()
{
    var result = await service.GetDataAsync();
}
```

---

## 3. `Task.Run` around I/O

```csharp
public async Task<User> GetUserAsync(int id)
{
    return await Task.Run(() => repository.GetUserAsync(id));
}
```

🚩 Usually pointless if `GetUserAsync()` is already asynchronous I/O.

You're potentially occupying a ThreadPool thread just to initiate/wait for I/O.

Prefer:

```csharp
return await repository.GetUserAsync(id);
```

---

## 4. Sequential awaits that should be concurrent

```csharp
var users = await GetUsersAsync();
var orders = await GetOrdersAsync();
var products = await GetProductsAsync();
```

If they're independent, this unnecessarily serializes the operations.

Better:

```csharp
var usersTask = GetUsersAsync();
var ordersTask = GetOrdersAsync();
var productsTask = GetProductsAsync();

await Task.WhenAll(usersTask, ordersTask, productsTask);

var users = await usersTask;
var orders = await ordersTask;
var products = await productsTask;
```

🚩 **Gotcha:** `Task.WhenAll` gives concurrency, not necessarily multiple threads.

---

## 5. `Task.WhenAll` with synchronous CPU work

```csharp
await Task.WhenAll(
    CalculateSomething(),
    CalculateSomethingElse(),
    AnotherCalculation());
```

🚩 `WhenAll` doesn't magically make synchronous CPU work parallel.

If those methods perform synchronous CPU calculations, they execute synchronously before `WhenAll` even gets the tasks.

---

## 6. Fire-and-forget

```csharp
public async Task ProcessAsync()
{
    SaveToDatabaseAsync();
}
```

🚩 Task is ignored.

Potentially:

- exceptions disappear
- operation may not finish
- caller can't know whether it succeeded

Better:

```csharp
await SaveToDatabaseAsync();
```

Or explicitly use a proper background-job mechanism when fire-and-forget is genuinely intended.

---

# LINQ gotchas

## 7. Multiple enumeration

```csharp
IEnumerable<User> users = GetUsers();

if (users.Any())
{
    foreach (var user in users)
    {
        ...
    }
}
```

🚩 `GetUsers()` may execute twice.

Especially dangerous if it represents:

- database query
- network call
- expensive computation

Could materialize:

```csharp
var users = GetUsers().ToList();

if (users.Count > 0)
{
    foreach (var user in users)
    {
        ...
    }
}
```

---

## 8. Deferred execution

```csharp
var adults = users.Where(x => x.Age >= 18);

users.Add(new User { Age = 30 });

foreach (var user in adults)
{
    ...
}
```

🚩 `Where()` hasn't necessarily executed when `adults` was created.

It's evaluated during enumeration.

If you need a snapshot:

```csharp
var adults = users
    .Where(x => x.Age >= 18)
    .ToList();
```

---

## 9. `ToList()` too early

```csharp
var users = database.Users.ToList();

var result = users
    .Where(x => x.IsActive)
    .Select(x => x.Name)
    .ToList();
```

🚩 Potentially loads the **entire table into memory**.

With EF Core, you'd usually want filtering/projection translated to SQL:

```csharp
var result = await database.Users
    .Where(x => x.IsActive)
    .Select(x => x.Name)
    .ToListAsync();
```

---

## 10. `First()` vs `Single()`

```csharp
var user = users.First(x => x.Email == email);
```

Question:

> Is it valid for multiple users to have this email?

If exactly one should exist:

```csharp
var user = users.Single(x => x.Email == email);
```

`Single()` expresses an **invariant**.

---

# Exceptions

## 11. Catching `Exception` and swallowing it

```csharp
try
{
    await ProcessAsync();
}
catch (Exception)
{
    return;
}
```

🚩 You've potentially hidden:

- database failure
- network failure
- programming bugs
- cancellation
- configuration errors

At minimum, understand which exceptions you're actually expecting.

---

## 12. Losing the stack trace

```csharp
catch (Exception ex)
{
    throw ex;
}
```

🚩 Don't do this.

Use:

```csharp
catch
{
    throw;
}
```

or add context:

```csharp
catch (Exception ex)
{
    throw new ProcessingException(
        "Failed to process order.",
        ex);
}
```

---

## 13. `finally` overriding the exception

```csharp
try
{
    throw new Exception("Original");
}
finally
{
    throw new Exception("New");
}
```

🚩 The `"New"` exception replaces the original one.

This is a good interview trap.

---

# Collections / performance

## 14. `List.Contains()` inside a loop

```csharp
foreach (var user in users)
{
    if (allowedIds.Contains(user.Id))
    {
        ...
    }
}
```

If `allowedIds` is a `List<int>`, each lookup is O(n).

Potentially:

```csharp
var allowedIds = allowedIdsList.ToHashSet();

foreach (var user in users)
{
    if (allowedIds.Contains(user.Id))
    {
        ...
    }
}
```

Average lookup becomes O(1).

---

## 15. Modifying collection during enumeration

```csharp
foreach (var user in users)
{
    if (!user.IsActive)
        users.Remove(user);
}
```

🚩 Usually throws `InvalidOperationException`.

Prefer:

```csharp
users.RemoveAll(x => !x.IsActive);
```

---

# Concurrency

## 16. Race condition with `++`

```csharp
private int _counter;

public void Increment()
{
    _counter++;
}
```

🚩 `++` isn't atomic.

Conceptually:

```text
read
+
write
```

Two threads can interfere.

Possible solution:

```csharp
Interlocked.Increment(ref _counter);
```

---

## 17. Check-then-act race

```csharp
if (!_cache.ContainsKey(key))
{
    _cache[key] = value;
}
```

🚩 Not thread-safe as a compound operation.

If concurrent access is possible, consider:

```csharp
_cache.TryAdd(key, value);
```

with `ConcurrentDictionary`.

---

## 18. `Dictionary` accessed concurrently

```csharp
private readonly Dictionary<int, User> _users = new();
```

Then:

```csharp
Parallel.ForEach(items, item =>
{
    _users[item.Id] = item;
});
```

🚩 `Dictionary<TKey,TValue>` isn't safe for concurrent writes.

Potential solution:

```csharp
ConcurrentDictionary<int, User>
```

---

# Disposal

## 19. `IDisposable` not disposed

```csharp
var stream = File.OpenRead(path);

var data = Read(stream);
```

🚩 Resource leak if `stream` isn't disposed.

Use:

```csharp
using var stream = File.OpenRead(path);
```

---

## 20. Async disposable

```csharp
var connection = new SomeAsyncDisposable();
```

If it implements `IAsyncDisposable`, use:

```csharp
await using var connection = new SomeAsyncDisposable();
```

rather than assuming synchronous `Dispose()` is equivalent.

---

# Dependency Injection

## 21. Singleton depending on scoped service

```csharp
services.AddSingleton<MyService>();
services.AddScoped<MyDbContext>();
```

```csharp
public class MyService
{
    public MyService(MyDbContext context)
    {
        ...
    }
}
```

🚩 Lifetime mismatch.

A singleton holding a scoped dependency can cause incorrect lifetime behavior and effectively keep the scoped object alive too long.

---

## 22. Creating dependencies manually

```csharp
public class OrderService
{
    public void Process()
    {
        var repository = new OrderRepository();
        ...
    }
}
```

🚩 Makes:

- testing harder
- configuration harder
- lifetime management harder
- dependency relationships less explicit

Usually inject the dependency.

---

# Memory/event gotchas

## 23. Event subscription causing object lifetime problems

```csharp
publisher.SomethingHappened += Handler;
```

If a long-lived `publisher` keeps a reference to a short-lived subscriber, the subscriber may remain reachable and therefore not be collected.

Often you need:

```csharp
publisher.SomethingHappened -= Handler;
```

particularly when the subscriber has a shorter lifetime.

---

# Nullability

## 24. `!` hiding a real problem

```csharp
var user = GetUser()!;
user.Name.ToUpper();
```

🚩 The null-forgiving operator doesn't make the value non-null at runtime.

It only tells the compiler:

> "Trust me."

It can therefore hide an actual `NullReferenceException`.

---

# String / equality

## 25. `==` vs reference equality

For strings:

```csharp
if (name == "John")
```

works as value comparison.

But with custom reference types:

```csharp
if (user1 == user2)
```

may mean reference equality unless `==` is overloaded.

An interviewer may ask:

> Does `==` always compare values?

**No. It depends on the type/operator implementation.**

---

# Database / EF Core

## 26. N+1 query

```csharp
var orders = await db.Orders.ToListAsync();

foreach (var order in orders)
{
    var customer = await db.Customers
        .FirstAsync(x => x.Id == order.CustomerId);
}
```

🚩 One query for orders + potentially one query per order.

Classic **N+1 problem**.

---

## 27. Loading unnecessary columns

```csharp
var users = await db.Users.ToListAsync();
```

when you only need:

```text
Id
Name
```

Potentially better:

```csharp
var users = await db.Users
    .Select(x => new UserDto
    {
        Id = x.Id,
        Name = x.Name
    })
    .ToListAsync();
```

---

# A very interview-like PR example

You might get something like:

```csharp
public async void ProcessOrders(List<Order> orders)
{
    foreach (var order in orders)
    {
        try
        {
            await _paymentService.PayAsync(order);

            _processedOrders.Add(order);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
        }
    }
}
```

A strong PR review would identify **multiple things**:

1. 🚩 `async void`
2. 🚩 Sequential processing — is that intentional?
3. 🚩 Catching `Exception`
4. 🚩 Logging only `ex.Message` loses useful exception information
5. 🚩 What happens if `_processedOrders` is concurrently accessed?
6. 🚩 Is payment idempotent if retrying?
7. 🚩 What happens if adding to `_processedOrders` fails after payment succeeds?
8. 🚩 Should cancellation be supported?
9. 🚩 Is processing 100,000 orders sequentially acceptable?
10. 🚩 If parallelizing, what's the concurrency limit?
11. 🚩 Does the caller need to know which orders failed?
12. 🚩 Is `List<Order>` appropriate, or should the method accept `IReadOnlyCollection<Order>`?

That is exactly the style of thinking I'd recommend for a **PR interview**: don't just look for syntax mistakes. Look for **correctness → concurrency → resource lifetime → failure handling → performance → API design**.

### A useful mental checklist

When you receive an unfamiliar C# PR, mentally run:

**A-C-E-P-D**

- **A — Async:** `async void`? blocking? unnecessary `Task.Run`? sequential awaits?
- **C — Concurrency:** races? shared mutable state? thread safety?
- **E — Exceptions:** swallowed? wrong catch? lost stack trace? retry behavior?
- **P — Performance:** N+1? unnecessary allocations? repeated enumeration? wrong collection?
- **D — Disposal/Data:** `IDisposable`? `IAsyncDisposable`? DB queries? excessive data loading?

If you can consistently hit those five areas, you'll catch a **large percentage of deliberate interview PR gotchas**.
