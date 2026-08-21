# Outbox Pattern (Architectural)

## Entity

```csharp
public class OutboxMessage
{
    public Guid Id { get; set; }

    public string Type { get; set; }

    public string Payload { get; set; }

    public DateTime OccurredOn { get; set; }

    public bool Processed { get; set; }
}
```

## Service

```csharp
using var transaction = await db.Database.BeginTransactionAsync();

db.Orders.Add(order);

db.OutboxMessages.Add(new OutboxMessage
{
    Type = "OrderCreated",
    Payload = JsonSerializer.Serialize(order)
});

await db.SaveChangesAsync();

await transaction.CommitAsync();
```

### Background worker

This worker can be implemented using `BackgroundService`, a hosted service, or a dedicated worker process.

```csharp
public class OutboxWorker : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var events = await db.OutboxMessages.Where(x => !x.Processed).ToListAsync();

            foreach (var message in events)
            {
                await publisher.Publish(message);

                message.Processed = true;
            }

            await db.SaveChangesAsync();

            await Task.Delay(5000);
        }
    }
}
```
