# Async Printer

```csharp
public class AsyncPrinter : IAsyncDisposable
{
    private readonly ConcurrentQueue<string> _queue = new();
    private readonly SemaphoreSlim _signal = new(0);
    private readonly Task[] _workers;
    private readonly CancellationTokenSource _cts = new();

    public AsyncPrinter(int maxConcurrentPrints)
    {
        _workers = Enumerable.Range(0, maxConcurrentPrints)
            .Select(_ => WorkerAsync())
            .ToArray();
    }

    public void Enqueue(string document)
    {
        _queue.Enqueue(document);

        // Tell one waiting worker that work is available
        _signal.Release();
    }

    private async Task WorkerAsync()
    {
        try
        {
            while (true)
            {
                // Wait until something is put in the queue
                await _signal.WaitAsync(_cts.Token);

                // Take the work
                if (_queue.TryDequeue(out var document))
                {
                    await PrintAsync(document);
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Shutdown
        }
    }

    private async Task PrintAsync(string document)
    {
        Console.WriteLine($"Printing {document}");

        await Task.Delay(1000);

        Console.WriteLine($"Finished {document}");
    }

    public async ValueTask DisposeAsync()
    {
        _cts.Cancel();

        // Wake workers so they can observe cancellation
        for (int i = 0; i < _workers.Length; i++)
        {
            _signal.Release();
        }

        await Task.WhenAll(_workers);

        _signal.Dispose();
        _cts.Dispose();
    }
}
```

### The important idea

You have **two separate responsibilities**:

`ConcurrentQueue` handles the data:

```
Producer
 ▼
ConcurrentQueue
 ▼
Worker
```

`SemaphoreSlim` handles notification:

```
Queue empty
 ▼
Worker waits ──────┐
                   │
Producer enqueues  │
                   │
                   ▼
             semaphore.Release()
                   │
                   ▼
             Worker wakes up
```

So when you do:

```csharp
_queue.Enqueue(document);
_signal.Release();
```

you're saying:

> "I've added one item, so wake one worker."

### Why not just lock the queue?

You could do:

```csharp
lock (_queue)
{
    _queue.Enqueue(document);
}
```

but the `lock` only protects the operation. It doesn't solve the **waiting problem**.

For example, if there is no work:

```
Worker
 ↓
queue empty
 ↓
what now?
```

You could have the worker repeatedly check:

```csharp
while (_queue.IsEmpty)
{
    // wait
}
```

but that's polling and wastes CPU.

`SemaphoreSlim` lets the worker efficiently sleep:

```csharp
await _signal.WaitAsync();
```

and wake only when work is available.

### One subtle interview point

You might ask:

> "Why use both `ConcurrentQueue` and `SemaphoreSlim`? Isn't `ConcurrentQueue` already thread-safe?"

Yes, but **thread safety and coordination are different things**.

`ConcurrentQueue` answers:

> "Can multiple threads safely add/remove items?"

`SemaphoreSlim` answers:

> "How do workers know when there is work?"

That's the key distinction.

### What if there are 2 workers and 10 documents?

Initially:

```
Queue: [1,2,3,4,5,6,7,8,9,10]

Worker 1 → 1
Worker 2 → 2
```

The remaining items stay queued:

```
[3,4,5,6,7,8,9,10]
```

When Worker 1 finishes:

```
Worker 1 → 3
```

When Worker 2 finishes:

```
Worker 2 → 4
```

So **at most 2 documents are being printed concurrently**.

One thing I'd change for a production implementation is making `Enqueue` asynchronous and deciding whether the queue should be **bounded**. A bounded queue gives you backpressure so a producer can't enqueue millions of documents indefinitely.
