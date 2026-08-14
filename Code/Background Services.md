# Background Services

```csharp
protected override async Task ExecuteAsync(CancellationToken token)
{
    while(!token.IsCancellationRequested)
    {
        var messages = await db.Outbox.Where(x => !x.Processed).ToListAsync();

        foreach(var message in messages)
        {
            await publisher.Publish(message);

            message.Processed = true;
        }


        await db.SaveChangesAsync();

        await Task.Delay(5000, token);
    }
}
```
