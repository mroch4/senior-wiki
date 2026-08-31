# Async & Await: The 12 Questions That Reject More .NET Candidates Than Anything Else

[source](https://medium.com/@thecurlybrace/async-await-the-12-questions-that-reject-more-net-candidates-than-anything-else-9e21c2d12858)

Here’s the pattern I keep seeing.

A developer with six years of experience walks into an interview. Fundamentals - solid. LINQ - fluent. Design patterns - can talk for twenty minutes. Then:

> Walk me through what actually happens when you `await` something.

And the answer comes back as some version of _'it waits for the task to finish without blocking.'_

Which isn’t wrong. It’s just not an answer. It’s a restatement of the keyword. And the interviewer now knows that everything after this is going to be surface-level too.

This round is different from the others because `async` **is the one topic where the wrong mental model still produces working code - right up until production**. Your .Result call works fine on your machine. It works fine in QA. Then it meets real concurrency and the whole app hangs.

Twelve questions. Same format:

- 🎯 What they’re **actually** testing
- 🌍 An analogy that sticks
- 💻 Small, clean code
- 🗣️ **The exact words to say**
- ⚠️ The trap or follow-up

## 1. Is this work CPU-bound or I/O-bound? (Ask this first, always)

🎯 **What they’re actually testing**

This isn’t usually asked directly — it’s the question _behind_ half the others. Almost every wrong async answer comes from skipping it.

🌍 **Think of it like this**

**CPU-bound** work is a chef **actively chopping vegetables** — someone has to be there, doing it, the whole time.

**I/O-bound** work is a chef **waiting for water to boil**. Standing over the pot doesn’t make it boil faster. Walking away and coming back when it whistles costs nothing.

[cpu-vs-io](https://miro.medium.com/v2/resize:fit:720/format:webp/1*xI8Fqi6yXnP0OET4ck2BzQ.png)

🗣️ **Say this in the interview**

> The first thing I ask is whether the work is CPU-bound or I/O-bound, because it determines everything else.

> CPU-bound work genuinely needs a thread - resizing an image, hashing, a heavy in-memory sort - and for that you’d use `Task.Run` or `Parallel.ForEach` to move it off the current thread.

> I/O-bound work — a database query, an HTTP call, a file read - doesn't need a thread at all while it's waiting, because the actual waiting happens at the OS and hardware level. That's what `async/await` is designed for: freeing the thread during a wait that no thread needs to supervise.

⚠️ **The trap:** “Does `async` make code faster?"

**No.** For a single operation, async is marginally slower — there’s state machine overhead. What it buys you is **scalability**: the same number of threads serves far more concurrent requests, because none of them sit blocked. A server that could handle 100 concurrent requests handles 10,000. That’s the payoff, and it’s not the same thing as speed.

📌 **Memory hook:** _CPU-bound needs a thread. I/O-bound doesn’t. Async is about scalability, not speed._

---

## 2. What actually happens when you await something?

🎯 **What they’re actually testing**

Whether you know there’s a compiler transformation happening, or whether `await` is magic to you.

🌍 **Think of it like this**

A waiter who takes your order and then **stands at the kitchen door until your food is ready** is a blocked thread. A waiter who takes your order, serves three other tables, and comes back when the food is up is `await`.

Same kitchen. Same cooking time. Completely different throughput.

[sync-vs-async-waiter](https://miro.medium.com/v2/resize:fit:720/format:webp/1*RnLQk3RdyF9E8xvxCxbZYg.gif)

```csharp
public async Task<string> GetUserNameAsync(int id)
{
    Console.WriteLine("A");                       // runs on the caller's thread
    var user = await _repo.GetUserAsync(id);      // method RETURNS here
                                                  // thread is released
    Console.WriteLine("B");                       // resumes when the task completes
    return user.Name;
}
```

🗣️ **Say this in the interview**

> When the compiler sees await in an async method, it rewrites the method into a state machine. The code runs synchronously up to the first await. At that point, if the awaited operation hasn't already completed, the method registers a continuation and returns to its caller - the thread is released back to the pool and can serve other work. When the operation completes, the continuation is scheduled and the method resumes at that exact point with its local variables restored. So await isn't waiting - it's yielding, and being resumed later.

⚠️ **The follow-up:** “Does `await` create a new thread?"

**No, and this is the answer they’re listening for.** For I/O, there’s no thread at all during the wait - the operation is handled by the OS and completion ports. When the work finishes, a thread from the pool picks up the continuation, but that’s not a new thread and it may not be the same one that started.

📌 **Memory hook:** _`await` doesn't wait — it returns. The compiler builds a state machine; the thread goes back to the pool._

---

## 3. Why is async void dangerous?

🎯 **What they’re actually testing**

Whether you’ve ever debugged an unexplained process crash. There’s a single correct answer here and it’s easy to state.

```csharp
// ⚠️ NEVER do this outside an event handler
public async void ProcessOrder(int id)
{
    await _service.ProcessAsync(id);
    throw new Exception("boom"); // nobody can catch this
}

// The caller cannot help:
try
{
    ProcessOrder(1); // returns instantly - nothing to await
}
catch (Exception ex)
{
    // never reached. The process dies instead.
}

// ✅ Do this
public async Task ProcessOrderAsync(int id)
{
    await _service.ProcessAsync(id);
}
```

|                               | `async Task`         | `async void`                    |
| ----------------------------- | -------------------- | ------------------------------- |
| Can be awaited                | ✅ Yes               | ❌ No - fire and forget         |
| Exceptions land               | On the returned Task | On the `SynchronizationContext` |
| `try/catch` at the call site  | ✅ Catches it        | ❌ Cannot catch it              |
| An unhandled throw            | Faults the task      | Crashes the process             |
| Caller knows when it finished | ✅ Yes               | ❌ Never                        |
| When is it acceptable         | Always the default   | Only event handlers             |

[async-void](https://miro.medium.com/v2/resize:fit:720/format:webp/1*LcTPo1x879ZuFWDhENVfbA.png)

🗣️ **Say this in the interview**

> Three reasons.

> First, you can’t await it — the caller has no way to know when it finished, so there’s no way to sequence anything after it.

> Second, exceptions can’t be caught by the caller; with `async Task`, an exception is captured on the returned Task, but with `async void` it's raised on the synchronization context and typically brings the process down.

> Third, it's untestable, because a test can't await it to know when to assert. The one legitimate use is event handlers, where the signature is forced on you by the delegate — and there I'd wrap the whole body in a try/catch.

⚠️ **The follow-up:** “What about fire-and-forget work?”

Use `async Task` and handle the task explicitly — a background service, a queue, or at minimum `_ = DoWorkAsync().ContinueWith(LogFailure, TaskContinuationOptions.OnlyOnFaulted)`. The problem with `async void` isn't that the work is unobserved; it's that the _failure_ is unobservable.

📌 **Memory hook**: Can’t await it. Can’t catch it. Can’t test it. Event handlers only.

---

## 4. How does `.Result` cause a deadlock?

🎯 **What they’re actually testing**

The single most valuable async question there is, because it explains a real production failure mode with a precise mechanism.

🌍 **Think of it like this**

Two people in a narrow corridor. Each says _'I’ll move when you move.'_ Neither is being unreasonable. Nobody moves.

[deadlock](https://miro.medium.com/v2/resize:fit:720/format:webp/1*8_if5plXA9h_9iVZvKmBrw.png)

```csharp
// In a classic ASP.NET or WinForms/WPF context:
public string GetData()
{
    return GetDataAsync().Result; // ⚠️ hangs forever
}

private async Task<string> GetDataAsync()
{
    var response = await _http.GetStringAsync(url);
    return response; // never reached
}
```

🗣️ **Say this in the interview**

> In a context with a single-threaded synchronization context — classic ASP.NET, WinForms, WPF — calling `.Result` blocks the current thread while it holds that context. Meanwhile the awaited operation completes and its continuation needs to resume _on that same context_, because by default `await` captures it. But the context is occupied by the thread that's blocked waiting. The continuation can never run, the task can never complete, and the block never releases. Classic circular wait.

⚠️ **The two follow-ups**

_“Why doesn’t this deadlock in ASP.NET Core?”_

ASP.NET Core removed the synchronization context, so the continuation can resume on any pool thread. **But blocking is still wrong there** — it wastes a pool thread, and under load it causes thread pool starvation instead of deadlock. Different failure, same root cause.

_“So how do you fix it?”_

Async all the way up. If a method awaits, its caller awaits, all the way to the entry point. Every .`Result` or `.Wait()` in a codebase is a place where someone broke the chain and pushed the problem downstream.

📌 **Memory hook:** _The caller blocks holding the context; the continuation needs that context. Await all the way up._

---

## 5. What is thread pool starvation?

🎯 **What they’re actually testing**

Whether you can diagnose a production incident. This is a senior-level signal.

🌍 **Think of it like this**

A call centre with eight agents. Every agent is on hold with a supplier — not talking, just holding the line. New customers ring and ring. Nobody’s busy, and nobody can be helped.

[thread-pool-starvation](https://miro.medium.com/v2/resize:fit:720/format:webp/1*2Cwwg_6sDbdfq5FiTZjAag.gif)

🗣️ **Say this in the interview**

> The thread pool has a limited number of threads, and it only injects new ones slowly — roughly one or two per second once it’s past its minimum. If code blocks pool threads on I/O, using `.Result` or `.Wait()`, those threads sit idle but unavailable. Under load, incoming requests queue waiting for a thread that never frees up. The signature symptom is latency climbing into seconds while CPU usage stays near zero — that combination is almost always starvation rather than a genuine performance problem. The fix is to stop blocking: await the I/O so the thread returns to the pool during the wait.

⚠️ **The bonus:** _“How would you confirm it?”_

Watch the thread count climbing steadily while CPU stays flat, check `ThreadPool.ThreadCount` versus queue length, or take a dump and look for many threads parked in `Monitor.Wait` or `Task.Result`. In .NET, the `ThreadPool` counters in `dotnet-counters` show queue length directly.

📌 **Memory hook:** _High latency, low CPU, rising thread count. That combination means blocked threads._

---

## 6. `Thread` vs `Task` vs `Parallel` — when do you use each?

🎯 **What they’re actually testing**

Whether you’d reach for `new Thread()` in 2026. That choice alone is the signal.

Certainly! Here is the pivoted version of your data in Markdown table format, where the concepts are columns and the attributes are rows:

|                 | `Thread`                    | `Task`                   | `Parallel` / PLINQ         |
| --------------- | --------------------------- | ------------------------ | -------------------------- |
| What it is      | An OS thread you own        | A promise of future work | A loop split across cores  |
| Costs           | ~1 MB stack, slow to create | Pooled and reused        | Uses the thread pool       |
| Returns a value | ❌ No                       | ✅ Task<T>               | ❌ No                      |
| Cancellation    | Crude at best               | ✅ CancellationToken     | ✅ ParallelOptions         |
| Exceptions      | Can kill the process        | Captured in the Task     | AggregateException         |
| Use it for      | Almost never now            | Everything async         | CPU-bound data parallelism |

[thread-task-parallel](https://miro.medium.com/v2/resize:fit:720/format:webp/1*y3rlh7xmqmtfLIhVdLAtUg.png)

```csharp
// Thread — you own it, you manage it. Almost never needed now.
var t = new Thread(DoWork);
t.Start();
t.Join();

// Task - a promise of work. Pooled, composable, cancellable, returns a value.
Task<int> task = Task.Run(() => Compute());
int result = await task;

// Parallel - split CPU-bound work across cores
Parallel.ForEach(images, img => Resize(img));
```

🗣️ **Say this in the interview**

> Thread is a raw OS thread — roughly a megabyte of stack, expensive to create, and you manage its lifetime yourself. I'd only use it for something genuinely long-running and dedicated that shouldn't occupy a pool thread.

> Task is an abstraction over work that may complete in the future — it uses the thread pool, composes with await, supports cancellation, and can return a value.

> Parallel and PLINQ are for data parallelism: the same CPU-bound operation over a collection, split across cores.

> My default is `Task` for asynchrony and `Parallel` for CPU-bound loops. `new Thread()` almost never.

⚠️ **The follow-up:** _“`Task.Run` vs `Task.Factory.StartNew?`"_

`Task.Run` is the safe modern default. `StartNew `has surprising defaults — it doesn't unwrap nested tasks, so passing an async delegate gives you a `Task<Task>` that completes as soon as the inner task starts, not when it finishes. You need `.Unwrap()` to fix it. It also uses `TaskScheduler.Current` rather than `Default`, which can behave unexpectedly inside another task. Unless you need custom `TaskCreationOptions`, use `Task.Run`.

📌 **Memory hook:** _Thread = you own it. Task = a promise, pooled and composable. Parallel = CPU-bound loops._

---

## 7. `WhenAll` vs `WhenAny` — and the loop mistake everyone makes

🎯 **What they’re actually testing**

Whether you can spot the most common async performance bug in real code.

[whenall](https://miro.medium.com/v2/resize:fit:720/format:webp/1*IfZDYKjEYplOqU_Tv0wezg.png)

```csharp
// ⚠️ Sequential — 3 calls × 3 seconds = 9 seconds
foreach (var id in ids)
    results.Add(await GetAsync(id));

// ✅ Concurrent - 3 seconds total
var tasks = ids.Select(id => GetAsync(id));
var results = await Task.WhenAll(tasks);

// WhenAny - first one to finish wins
var winner = await Task.WhenAny(primary, fallback);
```

🗣️ **Say this in the interview**

> `Task.WhenAll` takes a set of tasks and returns a task that completes when all of them do — the key is that you start them all first and await once, so they overlap.

> `WhenAny` completes as soon as the first one does, which suits timeouts and racing a primary against a fallback.

> The mistake I look for in code review is `await` inside a `foreach`: it forces each call to finish before the next starts, which turns concurrent I/O into sequential I/O for no reason.

⚠️ **Two follow-ups worth pre-empting**

_“What happens to exceptions in `WhenAll`?"_

All tasks run to completion. await rethrows only the **first** exception, but the returned task's `Exception` property holds an `AggregateException` with all of them. If you need every failure, inspect the task rather than relying on the `await`.

_“Would you use `WhenAll` for 10,000 calls?"_

No, and this is a great thing to raise unprompted. That would hammer the downstream service and exhaust connections. You'd throttle with `SemaphoreSlim` or use `Parallel.ForEachAsync` with `MaxDegreeOfParallelism`.

📌 **Memory hook:** _`WhenAll` waits for all. `WhenAny` returns the first. Never await inside a loop._

---

## 8. What does `ConfigureAwait(false)` actually do?

🎯 **What they’re actually testing**

Whether you understand the synchronization context, and — importantly — whether you know when this advice is _outdated_.

```csharp
// In library code:
public async Task<string> GetAsync()
{
    var data = await _http.GetStringAsync(url).ConfigureAwait(false);
    return Parse(data); // resumes on any pool thread — doesn't care
}
```

🗣️ **Say this in the interview**

> By default, `await` captures the current synchronization context and resumes the continuation on it - which matters in UI apps, where you must be back on the UI thread to touch controls. `ConfigureAwait(false)` says 'I don't need the original context, resume me anywhere.' That has two benefits: it avoids the deadlock scenario if a caller blocks, and it skips the cost of marshalling back. I use it consistently in library code, because a library can't know what context its caller has. In an ASP.NET Core application it's unnecessary - there's no synchronization context to capture — and in UI code you deliberately _want_ the capture for any continuation that touches the UI.

⚠️ **The nuance that shows you’re current**

A lot of advice online still says “always use `ConfigureAwait(false)`." That was written for classic ASP.NET. **In ASP.NET Core it's noise.** Saying that out loud — and explaining _why_ the advice existed — demonstrates that you understand the reason rather than following a rule.

📌 **Memory hook:** _Don’t resume on the captured context. Essential in libraries, pointless in ASP.NET Core, harmful in UI continuations._

---

## 9. How does cancellation work?

🎯 **What they’re actually testing**

Whether you know cancellation is **cooperative** — nothing gets forcibly stopped.

```csharp
public async Task<List<Order>> GetAsync(CancellationToken ct)
{
    // Pass it down — that's most of the job
    var orders = await _db.Orders.ToListAsync(ct);
    foreach (var o in orders)
    {
        ct.ThrowIfCancellationRequested(); // check in long loops
        Process(o);
    }
    return orders;
}

// Timeouts are just cancellation with a clock
using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
await GetAsync(cts.Token);
```

🗣️ **Say this in the interview**

> Cancellation in .NET is cooperative — a token is a request, not a command. Nothing is killed; the running code has to observe the token and stop. In practice that means two things: pass the `CancellationToken` down into every async call that accepts one, and call `ThrowIfCancellationRequested` inside long-running loops. In ASP.NET Core the framework gives you a token tied to the request, so if the client disconnects, you can stop doing work nobody is waiting for.

⚠️ **The follow-up:** _“What exception does it throw, and should you catch it?”_

`OperationCanceledException` — or `TaskCanceledException`, which derives from it. You generally **shouldn't** treat it as an error: it means the system worked. Logging it as an exception creates noise that hides real failures.

📌 **Memory hook:** _Cooperative. Pass the token down, check it in loops. Nothing stops unless your code stops it._

---

## 10. Why can’t you use lock with await?

🎯 **What they’re actually testing**

Whether you understand that async code can resume on a _different thread_ — and what that breaks.

```csharp
// ❌ Doesn't even compile
lock (_gate)
{
    await DoWorkAsync();     // CS1996
}
```

```csharp
// ✅ The async-aware equivalent
private readonly SemaphoreSlim _gate = new(1, 1);

await _gate.WaitAsync();
try
{
    await DoWorkAsync();
}
finally
{
    _gate.Release(); // always release, even on exception
}
```

🗣️ **Say this in the interview**

> `lock` uses `Monitor`, which has thread affinity — the thread that takes the lock must be the one that releases it. With `await`, the continuation can resume on a different pool thread, so the release would happen from a thread that doesn't own the lock. The compiler blocks it outright. For mutual exclusion around async work I use `SemaphoreSlim` with `WaitAsync`, and always release in a `finally`.

⚠️ **The follow-up:** _“When do you still use `lock`?"_

For short, purely synchronous critical sections — incrementing a counter, mutating a small shared structure. `lock` is cheaper than `SemaphoreSlim` for that. The rule is simply: no `await` inside, use `lock`; `await` inside, use `SemaphoreSlim`.

📌 **Memory hook:** _`lock` has thread affinity, `await` can change threads. Use `SemaphoreSlim.WaitAsync` for async mutual exclusion._

---

## 11. What is `IAsyncEnumerable<T>`?

🎯 **What they’re actually testing**

Whether you know modern C#. This is Part 3’s `yield return`, made asynchronous.

```csharp
public async IAsyncEnumerable<Order> StreamAsync([EnumeratorCancellation] CancellationToken ct = default)
{
    await foreach (var row in _db.Orders.AsAsyncEnumerable().WithCancellation(ct))
        yield return Map(row);
}

// Consume as items arrive - nothing is buffered
await foreach (var order in StreamAsync(ct))
    Process(order);
```

🗣️ **Say this in the interview**

> `IAsyncEnumerable<T>` is the async counterpart to `IEnumerable<T>` — it lets you stream results asynchronously with `await foreach`. Before it, you had a bad choice: `Task<List<T>>`, which buffers everything before the caller sees anything, or `IEnumerable<Task<T>>`, which is awkward. With `IAsyncEnumerable` the caller processes each item as it arrives. It's genuinely useful for large result sets, streaming APIs, and reading files line by line, because memory stays flat regardless of size.

⚠️ **The detail worth knowing:** the `[EnumeratorCancellation]` attribute. Without it, the token passed via `WithCancellation` doesn't reach your method body. It's a small thing, and knowing it signals you've actually written one.

📌 **Memory hook:** _`await foreach`. Stream results as they arrive instead of buffering the whole set._

---

## 12. Scenario: you need to call an API 1,000 times. How?

🎯 **What they’re actually testing**

This is the closing scenario question, and it’s where they find out whether you’d take down a downstream service.

**The wrong answers, in order of how bad they are:**

```csharp
// ❌ Sequential — correct, but 1000 × 200ms = 3+ minutes
foreach (var id in ids)
    results.Add(await CallApiAsync(id));

// ❌❌ All at once - 1000 concurrent calls. You just DDoS'd your own supplier.
var tasks = ids.Select(id => CallApiAsync(id));
await Task.WhenAll(tasks);
```

**The right answer — bounded concurrency:**

```csharp
// .NET 6+ — the cleanest option
await Parallel.ForEachAsync(
    ids,
    new ParallelOptions { MaxDegreeOfParallelism = 10, CancellationToken = ct },
    async (id, token) =>
    {
        var result = await CallApiAsync(id, token);
        results.Add(result); // use a concurrent collection here
    });
```

🗣️ **Say this in the interview**

> I wouldn’t do it sequentially, because the calls are independent and that wastes the concurrency entirely. But I also wouldn’t fire all thousand at once — that exhausts the connection pool and can overwhelm the downstream service, and you’ll usually get rate-limited or throttled anyway. I’d use bounded concurrency, with `Parallel.ForEachAsync` and a `MaxDegreeOfParallelism` of maybe ten to twenty, tuned against what the API actually tolerates. On top of that I'd add a retry policy with exponential backoff and jitter for transient failures, a cancellation token so the whole batch can be abandoned, and I'd collect results into a concurrent collection since multiple threads are writing.

⚠️ **The follow-up:** _“How do you pick the degree of parallelism?”_

You measure. Start conservative, watch latency and error rates, and increase until either the downstream service degrades or your own throughput plateaus. The honest answer: _'I’d tune it against the target’s documented rate limits'_ - is far better than confidently naming a number.

📌 **Memory hook:** _Not sequential. Not all at once. Bounded concurrency, with retries and a cancellation token._

---

# Quick self-check

1. Does `await` create a new thread?

2. Your API has 4-second latency but CPU sits at 3%. What’s the most likely cause?

3. Why can’t the caller catch an exception thrown inside an `async void` method?

4. Is `ConfigureAwait(false)` needed in an ASP.NET Core controller?

5. You need to call an API 1,000 times. Why is `Task.WhenAll` on all 1,000 a bad answer?

Answers

1. No. For I/O there’s no thread during the wait at all. A pool thread picks up the continuation afterwards, but that’s not a new thread and may not be the original one.

2. Thread pool starvation from blocking calls. High latency with low CPU is the signature — threads are parked, not busy.

3. `async void` returns nothing to await, so there's no Task to carry the exception. It's raised on the synchronization context instead and typically terminates the process.

4. No. ASP.NET Core has no synchronization context, so there’s nothing to capture. The advice comes from classic ASP.NET.

5. It fires 1,000 concurrent requests — exhausting the connection pool and likely overwhelming or getting throttled by the downstream service. Use bounded concurrency instead.
