### 1. `async` without `await`

```csharp
public async Task ProcessAsync()
{
    Process();
}
```

🚩 Usually a code smell. The method isn't actually asynchronous.

If `Process()` is synchronous:

```csharp
public Task ProcessAsync()
{
    Process();
    return Task.CompletedTask;
}
```

Or, if it should genuinely be asynchronous, make the underlying operation async.

---

### 2. `return await` vs `return`

```csharp
public async Task<User> GetUserAsync()
{
    return await _service.GetUserAsync();
}
```

This isn't automatically wrong, but often the `async/await` is unnecessary.

You can write:

```csharp
public Task<User> GetUserAsync()
{
    return _service.GetUserAsync();
}
```

However, **`return await` can be intentional** when you need to catch exceptions, use `finally`, or need specific async behavior.

For example:

```csharp
public async Task<User> GetUserAsync()
{
    try
    {
        return await _service.GetUserAsync();
    }
    catch (Exception ex)
    {
        throw new UserException("Failed to get user.", ex);
    }
}
```

---

### 3. Forgetting `await`

```csharp
public async Task ProcessAsync()
{
    SaveAsync();
    SendEmailAsync();
}
```

🚩 Both tasks are started, but neither is awaited.

The method can complete before either operation finishes.

Usually:

```csharp
await SaveAsync();
await SendEmailAsync();
```

Or, if independent:

```csharp
await Task.WhenAll(
    SaveAsync(),
    SendEmailAsync());
```

---

### 4. Sequential when you wanted parallel/concurrent

```csharp
var customer = await GetCustomerAsync();
var orders = await GetOrdersAsync();
var products = await GetProductsAsync();
```

If independent, you're waiting unnecessarily.

Better:

```csharp
var customerTask = GetCustomerAsync();
var ordersTask = GetOrdersAsync();
var productsTask = GetProductsAsync();

await Task.WhenAll(customerTask, ordersTask, productsTask);

var customer = await customerTask;
var orders = await ordersTask;
var products = await productsTask;
```

**Interview phrase:**

> "These operations are independent, so I would consider starting them concurrently."

---

### 5. `Task.WhenAll` doesn't mean multithreading

```csharp
await Task.WhenAll(
    Operation1Async(),
    Operation2Async());
```

🚩 Don't say:

> "This creates two threads."

For I/O-bound operations, both operations can be in progress concurrently while no ThreadPool thread is sitting there blocked waiting for the I/O.

**Concurrency ≠ parallelism.**

---

### 6. `Task.Run` around async I/O

```csharp
await Task.Run(() => httpClient.GetAsync(url));
```

🚩 Usually unnecessary.

Prefer:

```csharp
await httpClient.GetAsync(url);
```

`HttpClient` already provides asynchronous I/O.

---

### 7. `.Result` / `.Wait()`

```csharp
var result = GetDataAsync().Result;
```

🚩 Classic PR comment.

Potential issues:

- deadlocks in some synchronization-context environments
- ThreadPool starvation
- blocks a thread
- defeats async scalability

Prefer:

```csharp
var result = await GetDataAsync();
```

---

### 8. `async void`

```csharp
private async void ProcessAsync()
{
    await Process();
}
```

🚩 Usually wrong.

Use:

```csharp
private async Task ProcessAsync()
```

Main exception: event handlers.

---

### 9. Exception disappears in `async void`

```csharp
async void Process()
{
    await SomethingAsync();
    throw new Exception();
}
```

The caller can't do:

```csharp
try
{
    await Process();
}
catch
{
}
```

because there's nothing to await.

This is one of the major reasons `async void` is dangerous.

---

### 10. `Task.Run` + `async` lambda confusion

```csharp
Task.Run(async () =>
{
    await DoSomethingAsync();
});
```

This is valid, but ask yourself **why `Task.Run` exists**.

If `DoSomethingAsync()` is I/O-bound, normally:

```csharp
await DoSomethingAsync();
```

is enough.

---

### 11. `Task.Factory.StartNew` with async

This is a particularly nasty interview gotcha:

```csharp
var task = Task.Factory.StartNew(async () =>
{
    await DoSomethingAsync();
});
```

You can end up with:

```text
Task<Task>
```

rather than the task you probably expected.

`Task.Run` handles async delegates more naturally.

---

### 12. Cancellation token accepted but ignored

```csharp
public async Task ProcessAsync(CancellationToken cancellationToken)
{
    await httpClient.GetAsync(url);
}
```

🚩 Why accept a token if you're not passing it?

Better:

```csharp
await httpClient.GetAsync(
    url,
    cancellationToken);
```

Cancellation should generally flow through the async call chain.

---

### 13. Cancellation swallowed

```csharp
try
{
    await ProcessAsync(cancellationToken);
}
catch (Exception)
{
    // ignore
}
```

🚩 This catches `OperationCanceledException` too.

Cancellation is often **expected control flow**, not necessarily an error.

Be careful about:

```csharp
catch (OperationCanceledException)
```

versus genuinely unexpected exceptions.

---

### 14. Creating a new `CancellationTokenSource` and never disposing it

```csharp
var cts = new CancellationTokenSource();

await ProcessAsync(cts.Token);
```

`CancellationTokenSource` implements `IDisposable`.

Typically:

```csharp
using var cts = new CancellationTokenSource();

await ProcessAsync(cts.Token);
```

---

### 15. Timeout with `Task.Delay` doesn't cancel the operation

This is a **very good interview gotcha**.

```csharp
await Task.WhenAny(
    ProcessAsync(),
    Task.Delay(5000));
```

You might think:

> "The operation times out after five seconds."

Not necessarily.

You're only stopping **your wait**. `ProcessAsync()` can continue running in the background.

A proper timeout often needs cancellation:

```csharp
using var cts = new CancellationTokenSource(
    TimeSpan.FromSeconds(5));

await ProcessAsync(cts.Token);
```

---

### 16. `Task.WhenAny` leaves other tasks running

```csharp
var task1 = GetData1Async();
var task2 = GetData2Async();

var completed = await Task.WhenAny(task1, task2);
```

🚩 The other task doesn't automatically stop.

If you need cancellation, explicitly coordinate it.

---

### 17. Starting tasks inside a loop incorrectly

```csharp
foreach (var item in items)
{
    await ProcessAsync(item);
}
```

This is sequential.

Potentially:

```csharp
var tasks = items.Select(ProcessAsync);

await Task.WhenAll(tasks);
```

**BUT:** this introduces another PR question:

> What if `items` contains 100,000 elements?

Launching 100,000 operations simultaneously may be worse.

You may need **bounded concurrency**.

---

### 18. Unbounded concurrency

```csharp
var tasks = items.Select(x => ProcessAsync(x));

await Task.WhenAll(tasks);
```

🚩 Fine for 10 items.

Potentially terrible for 100,000.

Could cause:

- excessive memory usage
- connection exhaustion
- API throttling
- database overload

Interviewers love this one.

---

### 19. `SemaphoreSlim` forgotten

A common solution:

```csharp
using var semaphore = new SemaphoreSlim(10);

var tasks = items.Select(async item =>
{
    await semaphore.WaitAsync();

    try
    {
        await ProcessAsync(item);
    }
    finally
    {
        semaphore.Release();
    }
});

await Task.WhenAll(tasks);
```

🚩 The `finally` is important.

Without it, an exception can prevent `Release()` and eventually block everything.

---

### 20. Holding a lock across `await`

You cannot normally do:

```csharp
lock (_lock)
{
    await SomethingAsync();
}
```

`await` isn't allowed inside a `lock` statement.

If you need async synchronization, consider:

```csharp
SemaphoreSlim
```

with:

```csharp
await semaphore.WaitAsync();

try
{
    await SomethingAsync();
}
finally
{
    semaphore.Release();
}
```

---

### 21. `ConfigureAwait(false)` misunderstood

```csharp
await SomethingAsync().ConfigureAwait(false);
```

🚩 It does **not** mean:

> "Run this on another thread."

It means:

> "Don't capture the current synchronization context for continuation."

It's particularly relevant in library code, while modern ASP.NET Core generally doesn't have the classic UI-style synchronization context.

---

### 22. `Task.CompletedTask` vs `Task.FromResult`

This is a small but useful PR detail.

For no result:

```csharp
return Task.CompletedTask;
```

For a result:

```csharp
return Task.FromResult(value);
```

Don't write:

```csharp
return Task.Run(() => value);
```

just to create a task.

---

### 23. `Task.Delay` mistaken for a thread sleep

```csharp
await Task.Delay(5000);
```

This **doesn't block a ThreadPool thread for five seconds**.

Compare:

```csharp
Thread.Sleep(5000); // blocks thread
```

vs.

```csharp
await Task.Delay(5000); // asynchronous wait
```

Very common interview question.

---

### 24. Async doesn't automatically make CPU work faster

```csharp
public async Task<int> CalculateAsync()
{
    return CalculateHugeNumber();
}
```

🚩 Adding `async` doesn't make `CalculateHugeNumber()` asynchronous.

If it is CPU-bound, it still runs synchronously.

Async is primarily about **not blocking while waiting**, especially for I/O.

---

### 25. `Task` is not a Thread

This is probably the **most important mental model**.

```csharp
Task task = GetDataAsync();
```

doesn't mean:

```text
new Thread(...)
```

A `Task` represents an **operation/future result**.

For I/O:

```text
Thread starts I/O
       ↓
OS/network handles operation
       ↓
Thread is free
       ↓
I/O completes
       ↓
continuation resumes
```

That's why async I/O can scale much better than:

```csharp
Task.Run(() => BlockingDatabaseCall());
```

---

## ⭐ The PR-interview "gotcha ladder"

If you see this:

```csharp
public async Task ProcessAsync(IEnumerable<Item> items)
{
    foreach (var item in items)
    {
        await Task.Run(() => Process(item));
    }
}
```

Don't stop at one issue.

I'd review it in layers:

**1. Why `Task.Run`?**
Is `Process()` CPU-bound or I/O-bound?

**2. Why sequential?**
Could operations run concurrently?

**3. If concurrent, what's the limit?**
Don't blindly `Task.WhenAll` 100k items.

**4. Does `Process()` have cancellation?**

**5. What happens when one operation fails?**

**6. Are operations independent?**

**7. Is `Process()` thread-safe?**

**8. Is there shared mutable state?**

That's the kind of reasoning that makes a PR review look **senior**, rather than just identifying syntax/style issues.
