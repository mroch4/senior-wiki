Absolutely. For a **C# code-review / PR interview**, async/await is full of traps. Here are the most common **bad-practice patterns**, what is wrong, and how I would fix them.

## 1. `async void` when you don't need it

### ❌ Bad

```csharp
private async void ProcessPaymentAsync()
{
    await paymentService.ProcessAsync();
}
```

### ✅ Fix

```csharp
private async Task ProcessPaymentAsync()
{
    await paymentService.ProcessAsync();
}
```

### Why?

`async void` is mainly intended for **event handlers**.

With `Task`, the caller can:

- `await` the operation
- catch exceptions
- compose it with other tasks
- know when it has completed

With `async void`, the caller has no `Task` representing the operation.

```csharp
await ProcessPaymentAsync();
```

is only possible when it returns `Task`.

---

# 2. Fire-and-forget with `Task.Run`

### ❌ Bad

```csharp
public void Process()
{
    Task.Run(() => SendEmailAsync());
}
```

This is particularly suspicious in a PR.

### Problems

The caller doesn't know:

- whether the email succeeded
- whether it failed
- when it finished

Exceptions can also become difficult to observe.

### ✅ Fix

If the caller needs the operation:

```csharp
public async Task ProcessAsync()
{
    await SendEmailAsync();
}
```

If it genuinely must be background work, use an appropriate background-processing mechanism rather than casually creating a detached task.

---

# 3. `Task.Run` around naturally asynchronous I/O

### ❌ Bad

```csharp
public async Task<User> GetUserAsync(int id)
{
    return await Task.Run(() => userRepository.GetUserAsync(id));
}
```

This is usually pointless.

If `GetUserAsync()` already performs asynchronous I/O, you're adding a ThreadPool work item without making the I/O faster.

### ✅ Fix

```csharp
public Task<User> GetUserAsync(int id)
{
    return userRepository.GetUserAsync(id);
}
```

Or:

```csharp
public async Task<User> GetUserAsync(int id)
{
    return await userRepository.GetUserAsync(id);
}
```

The first version is preferable if you're simply forwarding the task.

---

# 4. `Task.Run` just to make a method "async"

### ❌ Bad

```csharp
public async Task<int> CalculateAsync()
{
    return await Task.Run(() => Calculate());
}
```

This doesn't magically make CPU work asynchronous in the scalability sense.

You're moving CPU work onto a ThreadPool thread.

### When can `Task.Run` be appropriate?

For example, a **UI application** where expensive CPU work would otherwise block the UI thread:

```csharp
var result = await Task.Run(() => ExpensiveCalculation());
```

But don't use it as a generic replacement for proper async I/O.

---

# 5. Blocking with `.Result`

### ❌ Bad

```csharp
public User GetUser()
{
    return userService.GetUserAsync().Result;
}
```

or:

```csharp
var user = userService.GetUserAsync().GetAwaiter().GetResult();
```

You're taking an asynchronous operation and synchronously blocking the current thread.

This can cause:

- ThreadPool starvation
- deadlocks in some synchronization-context environments
- reduced scalability

### ✅ Fix

Make the call chain asynchronous:

```csharp
public async Task<User> GetUserAsync()
{
    return await userService.GetUserAsync();
}
```

And:

```csharp
var user = await GetUserAsync();
```

### Interview phrase

> "Async should generally flow all the way up the call chain rather than blocking on a Task."

---

# 6. Mixing async and sync unnecessarily

### ❌ Bad

```csharp
public async Task ProcessAsync()
{
    var data = GetData();

    await SaveAsync(data);
}
```

This isn't necessarily wrong, but if `GetData()` performs I/O synchronously, you're blocking before reaching the asynchronous operation.

### Better

```csharp
public async Task ProcessAsync()
{
    var data = await GetDataAsync();

    await SaveAsync(data);
}
```

The important question in a PR is:

> "Does `GetData()` perform expensive or blocking I/O?"

---

# 7. Sequential awaits when operations are independent

This is a **very common interview gotcha**.

### ❌ Potentially bad

```csharp
var user = await GetUserAsync();
var orders = await GetOrdersAsync();
var settings = await GetSettingsAsync();
```

If these operations don't depend on one another, you're unnecessarily serializing them.

Conceptually:

```text
GetUser ───────>
               GetOrders ───────>
                              GetSettings ─────>
```

### ✅ Better

```csharp
var userTask = GetUserAsync();
var ordersTask = GetOrdersAsync();
var settingsTask = GetSettingsAsync();

await Task.WhenAll(userTask, ordersTask, settingsTask);

var user = await userTask;
var orders = await ordersTask;
var settings = await settingsTask;
```

Now they can make progress concurrently.

```text
GetUser    ─────────>
GetOrders  ───────>
GetSettings ────────>
```

### Important

`Task.WhenAll` **doesn't mean multiple threads**.

For I/O operations, they can be concurrently waiting without occupying a thread for the entire duration.

---

# 8. `Task.WhenAll` but still doing sequential work

### ❌ Bad

```csharp
await Task.WhenAll(
    GetUserAsync(),
    GetOrdersAsync());

var user = await GetUserAsync();
```

You've accidentally started `GetUserAsync()` **twice**.

### ✅ Fix

```csharp
var userTask = GetUserAsync();
var ordersTask = GetOrdersAsync();

await Task.WhenAll(userTask, ordersTask);

var user = await userTask;
var orders = await ordersTask;
```

---

# 9. Forgetting to await a Task

### ❌ Bad

```csharp
public async Task ProcessAsync()
{
    SaveAsync();

    Console.WriteLine("Finished");
}
```

`SaveAsync()` starts, but you're not awaiting it.

The method can reach `"Finished"` before saving is complete.

### ✅ Fix

```csharp
public async Task ProcessAsync()
{
    await SaveAsync();

    Console.WriteLine("Finished");
}
```

This is an excellent PR-review catch.

---

# 10. Calling async code inside a loop sequentially

### ❌ Potentially bad

```csharp
foreach (var item in items)
{
    await ProcessAsync(item);
}
```

Each operation waits for the previous one.

### If operations are independent:

```csharp
var tasks = items.Select(ProcessAsync);

await Task.WhenAll(tasks);
```

But there is another gotcha...

---

# 11. `Task.WhenAll` with thousands of operations

### ❌ Potentially bad

```csharp
var tasks = items.Select(ProcessAsync);

await Task.WhenAll(tasks);
```

If `items` contains 500,000 records, you could create a huge number of concurrent operations.

That can overwhelm:

- the database
- HTTP endpoints
- connection pools
- memory
- ThreadPool/resources

### ✅ Fix

Use bounded concurrency.

For example:

```csharp
await Parallel.ForEachAsync(
    items,
    new ParallelOptions
    {
        MaxDegreeOfParallelism = 10
    },
    async (item, cancellationToken) =>
    {
        await ProcessAsync(item, cancellationToken);
    });
```

Or use a queue/channel/semaphore depending on the architecture.

---

# 12. `async` method without `await`

### ❌

```csharp
public async Task<User> GetUserAsync()
{
    return userService.GetUserAsync();
}
```

This actually doesn't compile because you're returning `Task<User>` from an `async Task<User>` method.

You might instead see:

```csharp
public async Task<User> GetUserAsync()
{
    return await userService.GetUserAsync();
}
```

This is valid, but if you're simply forwarding the call:

### ✅

```csharp
public Task<User> GetUserAsync()
{
    return userService.GetUserAsync();
}
```

No need for `async`/`await`.

### But don't blindly remove `await`

This:

```csharp
public async Task<User> GetUserAsync()
{
    try
    {
        return await userService.GetUserAsync();
    }
    catch
    {
        // logging
        throw;
    }
}
```

has a reason to await because you're doing something around the asynchronous operation.

---

# 13. `async` lambda passed to `Action`

This one is **very interview-worthy**.

### ❌ Bad

```csharp
Action action = async () =>
{
    await SaveAsync();
};

action();
```

`Action` returns `void`, so you've effectively created an `async void` lambda.

### ✅ Fix

Use `Func<Task>`:

```csharp
Func<Task> action = async () =>
{
    await SaveAsync();
};

await action();
```

### Remember

```text
Action      → void
Func<Task>  → Task
```

---

# 14. Async callback with `List<T>.ForEach`

### ❌ Bad

```csharp
items.ForEach(async item =>
{
    await ProcessAsync(item);
});
```

`List<T>.ForEach` expects an `Action<T>`.

Therefore this is effectively an `async void` lambda.

The `ForEach` method doesn't wait for the asynchronous operations.

### ✅ Fix

```csharp
await Task.WhenAll(
    items.Select(ProcessAsync));
```

Or bounded concurrency if there are many items.

---

# 15. Creating a task and immediately awaiting it

### ❌ Unnecessary

```csharp
var task = Task.Run(() => Calculate());

return await task;
```

If the task has no purpose other than immediately being awaited:

### Better

```csharp
return await Task.Run(() => Calculate());
```

And depending on the context, potentially:

```csharp
return Calculate();
```

---

# 16. `await Task.Run(async () => ...)` for I/O

### ❌ Bad

```csharp
await Task.Run(async () =>
{
    await httpClient.GetAsync(url);
});
```

The HTTP operation is already asynchronous.

You're needlessly involving the ThreadPool.

### ✅

```csharp
await httpClient.GetAsync(url);
```

---

# 17. Swallowing exceptions from async operations

### ❌ Bad

```csharp
try
{
    await ProcessAsync();
}
catch
{
}
```

Now failures disappear.

### Better

```csharp
try
{
    await ProcessAsync();
}
catch (Exception ex)
{
    logger.LogError(ex, "Processing failed");
    throw;
}
```

Or handle the exception intentionally if the application can actually recover.

---

# 18. Catching exceptions around `Task.WhenAll` incorrectly

### ❌ Misleading assumption

```csharp
try
{
    await Task.WhenAll(tasks);
}
catch (Exception ex)
{
    // assume only one task failed
}
```

Multiple tasks can fail.

`Task.WhenAll` represents the combined operation. If you need detailed per-task outcomes, inspect the individual tasks or use an appropriate result/error model.

For example:

```csharp
var tasks = items.Select(ProcessAsync).ToArray();

try
{
    await Task.WhenAll(tasks);
}
catch
{
    foreach (var task in tasks.Where(t => t.IsFaulted))
    {
        // inspect task.Exception
    }

    throw;
}
```

---

# 19. Cancellation token ignored

### ❌ Bad

```csharp
public async Task ProcessAsync(CancellationToken cancellationToken)
{
    await httpClient.GetAsync(url);
}
```

You've accepted a cancellation token but aren't passing it downstream.

### ✅

```csharp
public async Task ProcessAsync(CancellationToken cancellationToken)
{
    await httpClient.GetAsync(url, cancellationToken);
}
```

Cancellation should generally **flow through the async call chain**.

---

# 20. Creating a new `HttpClient` inside async code

### ❌ Bad

```csharp
public async Task<string> GetAsync()
{
    using var client = new HttpClient();

    return await client.GetStringAsync(url);
}
```

Repeatedly creating/disposing `HttpClient` can cause connection-management problems and poor resource usage.

### Better

Use `IHttpClientFactory` in typical ASP.NET Core applications:

```csharp
public async Task<string> GetAsync(CancellationToken cancellationToken)
{
    return await httpClient.GetStringAsync(url, cancellationToken);
}
```

where `httpClient` is managed appropriately by the application.

---

# 21. Holding a lock across an await

### ❌ Impossible / invalid pattern

```csharp
lock (_lock)
{
    await DoSomethingAsync();
}
```

C# doesn't allow `await` inside a `lock`.

### Why?

The lock is synchronous/thread-affine, while an async operation can resume later.

### Better

Use an async-compatible synchronization primitive such as `SemaphoreSlim`:

```csharp
await semaphore.WaitAsync(cancellationToken);

try
{
    await DoSomethingAsync();
}
finally
{
    semaphore.Release();
}
```

---

# 22. Not using `ConfigureAwait(false)` in library code

This is a **more advanced interview topic**.

In reusable library code, you may see:

```csharp
await SomethingAsync();
```

versus:

```csharp
await SomethingAsync().ConfigureAwait(false);
```

`ConfigureAwait(false)` tells the awaiter not to attempt to resume on the captured synchronization context.

For modern ASP.NET Core code, there usually isn't a synchronization context to return to, so this isn't normally a major concern.

For general-purpose libraries, it can still be relevant.

---

# 23. Returning `Task` but doing synchronous work first

### ❌ Potentially problematic

```csharp
public async Task ProcessAsync()
{
    DoHugeCalculation();

    await SaveAsync();
}
```

The method is named async, but the expensive calculation happens synchronously **before the first await**.

The caller doesn't get asynchronous behavior for that portion.

### Important interview point

`async` does **not** mean:

> "Run everything on another thread."

The code executes synchronously until it reaches an incomplete `await`.

---

# The big PR-interview checklist

When reviewing async C#, I would immediately look for these:

| Pattern                     | Question to ask                                |
| --------------------------- | ---------------------------------------------- |
| `async void`                | Should this return `Task`?                     |
| `.Result`                   | Why are we blocking?                           |
| `.Wait()`                   | Why are we blocking?                           |
| `GetAwaiter().GetResult()`  | Is sync-over-async necessary?                  |
| `Task.Run` + I/O            | Why are we using a ThreadPool thread?          |
| Missing `await`             | Did we accidentally fire-and-forget?           |
| `async` lambda → `Action`   | Did this become `async void`?                  |
| `List.ForEach(async ...)`   | Is this actually awaited?                      |
| Sequential `await`s         | Could independent operations run concurrently? |
| Huge `Task.WhenAll`         | Is concurrency bounded?                        |
| Ignored `CancellationToken` | Does cancellation propagate?                   |
| `lock` + async              | Should this use `SemaphoreSlim`?               |
| `async` without `await`     | Is `async` actually necessary?                 |
| Exceptions                  | Who observes/handles them?                     |
| `Task.Run` everywhere       | Is CPU work being confused with I/O async?     |

### The 5 I'd memorize for an interview

If you have limited time, focus on these:

**1.**

```csharp
async void
```

→ usually `Task`

**2.**

```csharp
.Result / .Wait()
```

→ avoid sync-over-async; propagate `async`/`await`

**3.**

```csharp
Task.Run(() => httpCallAsync())
```

→ don't wrap naturally async I/O in `Task.Run`

**4.**

```csharp
items.ForEach(async x => ...)
```

→ `async void` trap; use `Task.WhenAll` or bounded concurrency

**5.**

```csharp
await A();
await B();
await C();
```

→ if independent, consider starting them first and `await Task.WhenAll(...)`

Those five alone catch a **large percentage of async-related PR mistakes**.
