# `lock` vs `SemaphoreSlim` in .NET

## Table of content

1. [`lock` vs `SemaphoreSlim` in .NET](#lock-vs-semaphoreslim-in-net)
2. [`lock`](#lock)
   - [Characteristics](#characteristics)
   - [Example problem](#example-problem)
3. [`SemaphoreSlim`](#semaphoreslim)
4. [Key Differences](#key-differences)
5. [Example: Protecting a shared cache](#example-protecting-a-shared-cache)
6. [Common real-world use cases](#common-real-world-use-cases)
   - [Protecting singleton state](#protecting-singleton-state)
   - [Limiting API calls](#limiting-api-calls)
   - [Preventing duplicate initialization](#preventing-duplicate-initialization)
7. [Deadlock example with `lock`](#deadlock-example-with-lock)
8. [SemaphoreSlim pitfalls](#semaphoreslim-pitfalls)
9. [In ASP.NET Core applications](#in-aspnet-core-applications)
10. [Interview Tips](#interview-tips)

## `lock` vs `SemaphoreSlim` in .NET

> Both are synchronization primitives used to protect shared resources from concurrent access, but they solve slightly different problems.

## `lock`

`lock` is a C# language feature built on top of `Monitor`.

It provides **mutual exclusion**: only **one thread** can enter the critical section at a time.

Example:

```csharp
private readonly object _lock = new();

private int _counter;

public void Increment()
{
    lock (_lock)
    {
        _counter++;
    }
}
```

Equivalent internally:

```csharp
Monitor.Enter(_lock);
try
{
    _counter++;
}
finally
{
    Monitor.Exit(_lock);
}
```

### Characteristics

✅ Simple
✅ Very fast (in-process)
✅ Synchronous only
✅ Only one owner at a time
❌ Cannot be awaited
❌ Cannot be used across processes

Example problem:

```csharp
lock (_lock)
{
    await SaveToDatabaseAsync(); // ❌ Not allowed
}
```

A thread would be blocked while waiting for I/O.

## `SemaphoreSlim`

`SemaphoreSlim` is a lightweight semaphore that allows **a configurable number of concurrent operations**.

A semaphore maintains a counter:

```
SemaphoreSlim(3)

Available slots:

[Thread A] -> using slot
[Thread B] -> using slot
[Thread C] -> using slot

Thread D waits
```

Example:

```csharp
private readonly SemaphoreSlim _semaphore = new(3);

public async Task ProcessAsync()
{
    await _semaphore.WaitAsync();

    try
    {
        await CallExternalApiAsync();
    }
    finally
    {
        _semaphore.Release();
    }
}
```

Here, maximum 3 requests can execute simultaneously.

## Key Differences

| Feature          | lock             | SemaphoreSlim               |
| ---------------- | ---------------- | --------------------------- |
| Purpose          | Exclusive access | Limit concurrency           |
| Concurrent users | 1                | N                           |
| Async support    | ❌               | ✅                          |
| Wait method      | Blocking         | Async awaitable             |
| Cross process    | ❌               | ❌ (use Semaphore for that) |
| Performance      | Faster           | Slightly slower             |
| Typical use      | Protect memory   | Throttle resources          |

## Example: Protecting a shared cache

Using `lock`:

```csharp
private readonly object _lock = new();

public User GetUser()
{
    lock(_lock)
    {
        return _cache.GetUser();
    }
}
```

Only one thread accesses the cache.

Using `SemaphoreSlim`:

```csharp
private readonly SemaphoreSlim _semaphore = new(1);

public async Task<User> GetUserAsync()
{
    await _semaphore.WaitAsync();

    try
    {
        return await _cache.GetUserAsync();
    }
    finally
    {
        _semaphore.Release();
    }
}
```

This behaves like an async lock.

## Common real-world use cases

### 1. Protecting singleton state

Example:

- in-memory cache
- shared dictionary
- counters

Use:

```
lock
```

Example:

```csharp
lock(_cacheLock)
{
    cache[key] = value;
}
```

### 2. Limiting API calls

Example:

You call a third-party API that allows only 10 requests per second.

```csharp
private readonly SemaphoreSlim _apiLimiter = new(10);

public async Task CallApi()
{
    await _apiLimiter.WaitAsync();

    try
    {
        await httpClient.GetAsync(url);
    }
    finally
    {
        _apiLimiter.Release();
    }
}
```

Maximum 10 concurrent API calls.

### 3. Preventing duplicate initialization

Example:

Multiple requests hit your application at startup.

```csharp
private readonly SemaphoreSlim _initLock = new(1);

public async Task InitializeAsync()
{
    await _initLock.WaitAsync();

    try
    {
        if (!_initialized)
        {
            await LoadDataAsync();
            _initialized = true;
        }
    }
    finally
    {
        _initLock.Release();
    }
}
```

Only one initialization runs.

## Deadlock example with `lock`

```csharp
lock(a)
{
    lock(b)
    {
        ...
    }
}
```

Thread 1:

```
lock(a)
waiting for b
```

Thread 2:

```
lock(b)
waiting for a
```

Result: `Deadlock`

## SemaphoreSlim pitfalls

### Forgetting Release()

❌ Bad:

```csharp
await semaphore.WaitAsync();

DoWork();
```

If `DoWork()` throws, the semaphore is never released.

✅ Correct:

```csharp
await semaphore.WaitAsync();

try
{
    DoWork();
}
finally
{
    semaphore.Release();
}
```

## In ASP.NET Core applications

Usually:

### Avoid `lock` around:

- database calls
- HTTP calls
- file I/O
- async operations

Because it blocks threads.

Prefer:

```
SemaphoreSlim + async/await
```

Example:

```
Controller
 |
Service
 |
SemaphoreSlim
 |
External API
```

# Interview Tips

> "`lock` is a mutual exclusion mechanism built on Monitor and is synchronous. `SemaphoreSlim` is a counting semaphore that supports async waiting and is commonly used for throttling or coordinating asynchronous operations."

- Mention that `lock` cannot be used with `await`.
- Know that `SemaphoreSlim(1)` is often used as an **async lock**.
- Be ready to explain why holding a lock during database/API calls is a bad idea.
- For distributed systems, mention that neither `lock` nor `SemaphoreSlim` works across multiple application instances; you need distributed locks (for example Redis-based locks or database locking).

**Use `lock` when:**

> "I need one thread at a time accessing a small piece of in-memory state."

**Use `SemaphoreSlim` when:**

> "I need async waiting or I want to allow N operations concurrently."
