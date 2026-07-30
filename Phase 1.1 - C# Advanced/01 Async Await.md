# Async/Await

## Table of content

## What is Async/Await?

>

# Interview Tips

# Question 1

> **What does the `async` keyword actually do?**

A common misconception is that `async` makes a method run on another thread.

**It does not.**

The `async` keyword tells the compiler:

> "This method contains one or more `await` expressions. Transform it into a state machine so it can pause and later resume execution."

For example:

```csharp
public async Task<string> GetDataAsync()
{
    var result = await httpClient.GetStringAsync(url);
    return result;
}
```

The compiler transforms this into something conceptually like:

```csharp
public Task<string> GetDataAsync()
{
    var stateMachine = new GetDataStateMachine();
    stateMachine.MoveNext();
    return stateMachine.Task;
}
```

You don't see this generated code, but that's effectively what happens.

## What does `await` do?

When execution reaches:

```csharp
await SomeOperationAsync();
```

the runtime checks:

### Is the task already completed?

If yes:

```csharp
Continue immediately.
```

No suspension happens.

If not:

1. Save all local variables.
2. Save the current execution position (the "state").
3. Return control to the caller.
4. Register a continuation to resume the method when the task completes.

The thread is **not blocked** while waiting.

## Example timeline

```csharp
public async Task Foo()
{
    Console.WriteLine("A");

    await Task.Delay(5000);

    Console.WriteLine("B");
}
```

Timeline:

```
Thread #12

A
↓

Task.Delay starts timer

↓

Method exits temporarily

Thread #12 becomes free
(can process other work)

↓

5 seconds later

↓

Continuation is scheduled

↓

B
```

Notice:

**The thread does not sit idle for five seconds.**

## Does async create a new thread?

This is a favorite interview question.

Answer:

> No.

For example:

```csharp
await File.ReadAllTextAsync(file);
```

No thread is waiting for the disk.

The operating system performs the I/O asynchronously. When the operation completes, .NET schedules the continuation.

Likewise:

```csharp
await HttpClient.GetAsync(url);
```

No thread waits for the network response.

## Then what creates a new thread?

Code like:

```csharp
await Task.Run(() =>
{
    DoCpuWork();
});
```

`Task.Run()` queues work to the thread pool.

So:

```
Current Thread
      │
      │
Task.Run
      │
      ▼
ThreadPool Thread
```

Notice it's `Task.Run()`, not `async`, that introduces another thread.

## Why is async scalable?

Imagine a web server handling 1,000 requests.

Without async:

```
1000 requests

↓

1000 threads blocked

↓

High memory usage

↓

Context switching

↓

Poor scalability
```

With async:

```
1000 requests

↓

I/O operations started

↓

Threads returned to the pool

↓

Only active requests occupy threads

↓

Far better scalability
```

This is why ASP.NET Core strongly encourages asynchronous I/O.

## Why does `async` return `Task`?

A `Task` represents work that may finish in the future.

```csharp
Task<int>
```

means:

> "I don't have the `int` yet, but I will."

`await` unwraps the result:

```csharp
int x = await GetNumberAsync();
```

Without `await`:

```csharp
Task<int> task = GetNumberAsync();
```

## Common interview traps

### Trap 1

> Does `async` make code faster?

**No.**

It improves **scalability** and **responsiveness**, not the speed of the underlying operation.

### Trap 2

> Does `await` block the thread?

**No.**

It suspends the method and frees the thread until the awaited task completes.

### Trap 3

> Does every `async` method run on another thread?

**No.**

Only if you explicitly schedule work to another thread (e.g., with `Task.Run`) or the underlying API uses background threads internally.

## Senior-level question

Imagine this code:

```csharp
public async Task<int> CalculateAsync()
{
    Console.WriteLine(Thread.CurrentThread.ManagedThreadId);

    await Task.Delay(1000);

    Console.WriteLine(Thread.CurrentThread.ManagedThreadId);

    return 42;
}
```

**Question:** Will the two thread IDs always be the same?

**Answer:** **Not necessarily.**

- In an ASP.NET Core application, there is no `SynchronizationContext`, so the continuation may run on a different thread pool thread.
- In a WPF or WinForms application, the default behavior is to resume on the UI thread, so the IDs are typically the same.

The important guarantee is **logical continuation**, not thread affinity (unless a synchronization context enforces it).

# Question 2

> **Why is using `.Result` or `.Wait()` on an asynchronous method generally considered a bad practice?**

> They turn async into sync code and may cause deadlocks

That's a good start. I'd rate that answer around **7/10** for a senior interview. It identifies the key issue (deadlocks), but I'd expect a bit more depth.

---

Here's how I'd challenge you as the interviewer:

> **Why can they cause deadlocks?**
> And **why is this less of an issue in ASP.NET Core than in classic ASP.NET or WPF?**

A strong senior answer would be:

> Calling `.Result` or `.Wait()` blocks the current thread until the `Task` completes. If the awaited method captures the current `SynchronizationContext` (such as the UI thread in WPF/WinForms or the request context in classic ASP.NET), its continuation tries to resume on that same context. But the thread owning that context is blocked waiting for the result, so the continuation can't run, creating a deadlock.

For example:

```csharp
public async Task<string> GetDataAsync()
{
    await Task.Delay(1000);
    return "Hello";
}

public void Button_Click(...)
{
    var result = GetDataAsync().Result;
}
```

Execution flow:

1. `Button_Click` runs on the UI thread.
2. `GetDataAsync` starts and reaches `await Task.Delay`.
3. `await` captures the UI `SynchronizationContext`.
4. `.Result` blocks the UI thread.
5. The delay completes.
6. The continuation wants to resume on the UI thread.
7. The UI thread is blocked by `.Result`.
8. **Deadlock.**

## Why doesn't this usually happen in ASP.NET Core?

ASP.NET Core **does not install a `SynchronizationContext`** for request processing. After an `await`, the continuation is scheduled on a thread pool thread instead of trying to get back to a specific request thread. That removes the classic deadlock scenario.

However, `.Result` and `.Wait()` are **still bad practice** because they:

- Block a thread that could be serving other work.
- Reduce throughput and scalability.
- Increase the risk of thread pool starvation under load.
- Can still deadlock in code that introduces its own synchronization context or locking.

# Question 3

Suppose you have this code:

```csharp
public async Task<IActionResult> Get()
{
    var customer = await customerService.GetCustomerAsync();

    var orders = await orderService.GetOrdersAsync(customer.Id);

    return Ok(new { customer, orders });
}
```

**Question:** If `GetCustomerAsync()` and `GetOrdersAsync()` are independent (i.e., `GetOrdersAsync` does **not** need the customer result), how would you optimize this method using `Task.WhenAll`, and why is it more efficient?

> Because both can be excited in parallel without waiting till first one is finished

Exactly. That's the core idea. I'd score this **8.5/10**. For a senior interview, I'd want you to be a bit more precise and mention **when** it's safe to parallelize work.

---

A polished answer would be:

> "If the operations are independent, I would start both asynchronous operations before awaiting either of them. That allows both I/O operations to be in progress at the same time instead of waiting for one to finish before starting the next. Then I'd use `Task.WhenAll` to asynchronously wait until both complete, reducing the overall response time."

For example:

```csharp
public async Task<IActionResult> Get()
{
    var customerTask = customerService.GetCustomerAsync();
    var ordersTask = orderService.GetOrdersAsync();

    await Task.WhenAll(customerTask, ordersTask);

    return Ok(new
    {
        customer = await customerTask,
        orders = await ordersTask
    });
}
```

Or even:

```csharp
var customerTask = customerService.GetCustomerAsync();
var ordersTask = orderService.GetOrdersAsync();

await Task.WhenAll(customerTask, ordersTask);

var customer = customerTask.Result;
var orders = ordersTask.Result;
```

# Question 4

> **Why is using `.Result` acceptable here after `await Task.WhenAll`, even though we just said to avoid `.Result`?**

> Becuase in that cast it does not block the thread but is used to access the operation result

Excellent. That's essentially correct. I'd give this **9.5/10** in a senior interview.

---

Here's how I'd refine it to make it sound more authoritative:

> "After `await Task.WhenAll(...)` completes, both tasks are guaranteed to be in a terminal state (typically `RanToCompletion`, or they have faulted/canceled). Therefore, accessing `.Result` does not block because the result is already available. The issue with `.Result` is blocking on an _incomplete_ task, not reading the result of a completed one."

For example:

```csharp
var customerTask = customerService.GetCustomerAsync();
var ordersTask = orderService.GetOrdersAsync();

await Task.WhenAll(customerTask, ordersTask);

// Safe: both tasks have already completed
var customer = customerTask.Result;
var orders = ordersTask.Result;
```

Many developers think ".Result is always bad." That's not true.

- ❌ Bad:

  ```csharp
  var customer = customerService.GetCustomerAsync().Result;
  ```

  because it may block.

- ✅ Fine:

  ```csharp
  await Task.WhenAll(customerTask, ordersTask);

  var customer = customerTask.Result;
  ```

  because there is nothing left to wait for.

# Question 5

What is the difference between these two pieces of code?

```csharp
await Task1();
await Task2();
```

and

```csharp
var t1 = Task1();
var t2 = Task2();

await Task.WhenAll(t1, t2);
```

Besides "one is faster," explain:

- What actually happens internally?
- When would you **not** use `Task.WhenAll`?
- Can `Task.WhenAll` make an application slower or even fail under some circumstances?

> First execution happens one by one; starting task2 as soon as task1 is finished execution is done by one thread at the time not necessarily the same one ; case 2 these tasks are executed on their own threads concurrently. When all should not be used then tasks are denedent of each other - like 2nd consumes 1st response

---

Very good answer. I'd score it **8.5–9/10**. You understand the concept, but there's one important misconception that interviewers often probe.

## The misconception

You said:

> "these tasks are executed on their own threads concurrently."

This is **not necessarily true**.

This is one of the most common interview traps.

### For I/O-bound operations

Imagine:

```csharp
var t1 = httpClient.GetAsync(url1);
var t2 = httpClient.GetAsync(url2);

await Task.WhenAll(t1, t2);
```

There are **not two threads** sitting there doing work.

Instead:

1. Request 1 is sent.
2. Request 2 is sent.
3. Both are waiting on the operating system/network.
4. No managed thread is blocked during the wait.
5. When each response arrives, a thread pool thread executes the continuation.

So `Task.WhenAll` enables **concurrency**, but not necessarily **multiple active threads**.

### For CPU-bound operations

If you write:

```csharp
var t1 = Task.Run(() => Calculate1());
var t2 = Task.Run(() => Calculate2());

await Task.WhenAll(t1, t2);
```

Now it's likely that two thread pool threads execute the calculations in parallel (assuming enough CPU cores).

So:

- **I/O tasks** → concurrent without occupying threads while waiting.
- **CPU tasks with `Task.Run`** → may execute in parallel on multiple threads.

Senior interviewers love this distinction.

## When should you NOT use `Task.WhenAll`?

You correctly identified the first case:

✅ When one task depends on the result of another.

Example:

```csharp
var customer = await GetCustomerAsync();
var orders = await GetOrdersAsync(customer.Id);
```

You cannot start `GetOrdersAsync` until you know the customer ID.

Other cases include:

### 1. Resource limits

Imagine:

```csharp
var tasks = users.Select(u => SendEmailAsync(u));

await Task.WhenAll(tasks);
```

If there are **100,000 users**, you've just started 100,000 operations at once.

This can:

- exhaust database connections,
- overwhelm an external API,
- consume excessive memory,
- trigger rate limiting.

In these situations, you'd throttle concurrency (e.g., with `SemaphoreSlim` or `Parallel.ForEachAsync` with a `MaxDegreeOfParallelism`).

### 2. Failure behavior

If one task fails:

```csharp
await Task.WhenAll(task1, task2, task3);
```

`Task.WhenAll` waits for **all tasks** to finish, then completes in a faulted state if any task failed. You need to consider whether that's the behavior you want.

## Interview score so far

If this were a real interview:

- ✅ Understands `async`/`await`
- ✅ Knows why `.Result` is dangerous
- ✅ Understands `Task.WhenAll`
- ⚠️ Small misconception about "each task gets its own thread"

Overall: **9/10**. You're answering at a solid senior level.
