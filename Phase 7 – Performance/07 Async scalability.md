## Table of content

# `async` scalability

`async` scalability in .NET means using asynchronous I/O so that a server can handle **more concurrent requests without needing one dedicated thread per waiting operation**.

## The core idea

Imagine an API endpoint:

```csharp
public async Task<User> GetUserAsync(int id)
{
    return await database.GetUserAsync(id);
}
```

While the database is working, your application **doesn't need to keep a thread blocked** waiting for the response.

Conceptually:

```
Request
   ▼
Thread starts DB call
   ├─► DB is processing
   │   Thread is returned to ThreadPool
   ▼
Other requests can use that thread
   │
   ...
   │
DB finishes
   ▼
Continuation resumes
   ▼
Response
```

That's where the scalability comes from.

## `async` vs synchronous

### With synchronous code

```csharp
var user = database.GetUser();
```

If the DB takes 500 ms, the request's thread can be blocked for those 500 ms.

### With `async`

```csharp
var user = await database.GetUserAsync();
```

the thread can be released while the DB operation is in progress.

So if you have:

```
1,000 concurrent requests
 ▼
DB/API calls
```

you don't necessarily need **1,000 threads**.

## Important: `async` doesn't make the operation faster

`async/await` primarily improves **resource utilization and scalability**, not the raw execution time of the operation.

If the database takes 500 ms:

```
Sync:   thread waits 500 ms
Async:  thread doesn't have to wait 500 ms
```

The DB still takes roughly 500 ms.

The benefit is that the server can use its limited ThreadPool threads to process other work during that wait.

## Where async helps most

Async scalability is especially valuable for **I/O-bound operations**:

- Database queries
- HTTP requests
- Reading/writing files
- Network communication
- Message queues
- Cloud storage

For example:

```csharp
public async Task<IActionResult> GetOrder(int id)
{
    var order = await _httpClient.GetFromJsonAsync<Order>($"orders/{id}");

    return Ok(order);
}
```

While `HttpClient` waits for the remote server, the application doesn't need to keep a worker thread blocked.

## What async does NOT solve

Async doesn't magically make **CPU-bound work** scalable.

For example:

```csharp
public async Task<int> Calculate()
{
    return VeryExpensiveCalculation();
}
```

Making the method `async` doesn't help if the expensive calculation is CPU work.

You'd need a different strategy, potentially parallelism/background processing, depending on the problem.

## Interview Tips

> Async scalability means that asynchronous I/O allows a server to avoid blocking ThreadPool threads while waiting for external operations such as databases or HTTP calls. Those threads can process other requests, allowing the application to handle more concurrent requests with fewer threads. It improves resource utilization and throughput rather than making the underlying I/O operation itself faster.

Yes — **async and multithreading are related, but they are not the same thing.**

# Multithreading

Multithreading means having **multiple threads executing work concurrently**.

For example:

```
Thread 1 → Request A → CPU work
Thread 2 → Request B → CPU work
Thread 3 → Request C → CPU work
```

Threads are actual execution paths managed by the OS/.NET runtime.

## Async

Async is mainly about **not blocking a thread while waiting for I/O**.

```
Thread 1
   ├── Start DB request
   ├── Thread is free
   ├── handles another request
   └── resumes when DB completes
```

So you can have **async code without creating another thread**.

## The important distinction

|                            | Async                        | Multithreading            |
| -------------------------- | ---------------------------- | ------------------------- |
| Main purpose               | Avoid blocking while waiting | Execute work concurrently |
| Requires multiple threads? | ❌ Not necessarily           | ✅ Yes                    |
| Great for                  | I/O-bound work               | CPU-bound/concurrent work |
| Example                    | DB/HTTP/file operations      | Parallel calculations     |
| Improves scalability?      | ✅ Especially for I/O        | Sometimes                 |

## Example

This:

```csharp
await httpClient.GetAsync(url);
```

does **not** mean:

> Start another thread to make the HTTP request.

Instead, roughly:

```
Thread
  ├─ starts HTTP I/O
  ├─ returns to ThreadPool
  │  HTTP operation happens
  └─ continuation runs when completed
```

This is why **async can improve scalability without increasing the number of threads proportionally**.

## And `Task.Run`?

A common misconception is:

```csharp
await Task.Run(() => DoSomething());
```

= async. Not really.

`Task.Run` typically means:

**Run this synchronous CPU work on a ThreadPool thread.**

So:

```csharp
await database.GetAsync(); // async I/O
```

and

```csharp
await Task.Run(() => Calculate()); // ThreadPool thread
```

are fundamentally different mechanisms.

## Interview Tips

Multithreading is about threads executing work; async is about avoiding blocking while waiting, especially for I/O. Async may use multiple threads over the lifetime of an operation, but it doesn't require a dedicated thread while the I/O is pending.

# Easy mental model

| Concept        | Meaning                                        |
| -------------- | ---------------------------------------------- |
| Concurrency    | Multiple tasks are in progress                 |
| Multithreading | Multiple threads execute work                  |
| Parallelism    | Multiple pieces of work execute simultaneously |
| Async          | Don't block while waiting, especially for I/O  |

So:

**Concurrency does not require multithreading.**
**Multithreading can provide concurrency.**
**Parallelism generally requires multiple execution resources (e.g. CPU cores/threads).**

For your senior .NET interview, this distinction is **very important**, especially when discussing `async/await`, `Task.WhenAll`, `Task.Run`, and ThreadPool behavior.
