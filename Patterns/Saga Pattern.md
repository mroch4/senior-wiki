# Saga Pattern

## Order service publishes:

```csharp
await publisher.Publish(new OrderCreated(orderId));
```

## Inventory consumer:

```csharp
public async Task Consume(OrderCreated message)
{
    await ReserveInventory();

    await publisher.Publish(new InventoryReserved(message.OrderId));
}
```

## Payment consumer:

```csharp
public async Task Consume(InventoryReserved message)
{
    try
    {
        await ChargeCard();

        await publisher.Publish(new PaymentCompleted(message.OrderId));
    }
    catch
    {
        await publisher.Publish(new PaymentFailed(message.OrderId));
    }
}
```

## Order consumer:

```csharp
public async Task Consume(PaymentFailed message)
{
    order.Status = OrderStatus.Cancelled;
}
```
