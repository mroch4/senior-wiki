# Outbox Pattern

## Table of content

1. [What is Outbox Pattern?](#what-is-outbox-pattern)
2. [The Problem](#the-problem)
   - [Scenario 1](#scenario-1)
   - [Scenario 2](#scenario-2)
3. [Why not use one transaction?](#why-not-use-one-transaction)
4. [Outbox Pattern Idea](#outbox-pattern-idea)
5. [Outbox Table](#outbox-table)
6. [Flow Step by Step](#flow-step-by-step)
   - [Step 1](#step-1)
   - [Step 2](#step-2)
   - [Step 3](#step-3)
   - [Step 4](#step-4)
   - [Step 5](#step-5)
   - [Step 6](#step-6)
   - [Step 7](#step-7)
7. [Visual Timeline](#visual-timeline)
   - [Without Outbox](#without-outbox)
   - [With Outbox](#with-outbox)
8. [Why It Works](#why-it-works)
9. [Typical .NET Implementation](#typical-net-implementation)
   - [Entity](#entity)
   - [Service](#service)
   - [Background worker](#background-worker)
10. [What if publishing fails?](#what-if-publishing-fails)
11. [Duplicate Messages](#duplicate-messages)
12. [Pros](#pros)
13. [Cons](#cons)
14. [Cheat Sheet](#cheat-sheet)
15. [Interview Tips](#interview-tips)

## What is Outbox Pattern?

The **Outbox Pattern** is one of the most important patterns in distributed systems and microservices. It solves a very common problem:

> **How can we guarantee that a database change and a message publication either both happen or neither happen?**

## The Problem

Imagine an Order Service. When a customer places an order `POST /orders`:

1. Save Order into SQL database
2. Publish `OrderCreated` event to RabbitMQ/Kafka/Azure Service Bus

But what if something fails?

### Scenario 1

Database succeeds:

✅ Order saved -> Database: `Order #100`

❌ RabbitMQ down -> Message Broker: `nothing`

Other services never know an order exists.

### Scenario 2

**Even worse:**

❌ Save fails -> database transaction rolls back

✅ Message published -> Inventory Service reserves stock -> Billing starts payment

But order doesn't exist.

## Why not use one transaction?

People often ask:

> Why don't we wrap database + RabbitMQ inside one transaction?

Because they are different systems.

```
SQL Server
RabbitMQ
Kafka
Azure Service Bus
```

Each has its own transaction mechanism.

Distributed transactions (2PC/MSDTC) are:

- slow
- difficult
- unsupported by many brokers
- avoided in cloud-native architectures

## Outbox Pattern Idea

Instead of immediately publishing the message, save the event **inside the database** first. Both are committed together:

```
Transaction

Save Order
Save Event

Commit
```

Later another process publishes events.

Architecture

```
Client
 |
Order Service
 |
┌──────────────────────┐
│ Database Transaction │
│                      │
│ Save Order           │
│ Save Outbox Event    │
└──────────────────────┘
 |
Background Worker
 |
Publish Event
 |
RabbitMQ
```

Now database transaction contains everything.

## Outbox Table

Orders:

```
Id
Customer
Amount
```

Outbox:

```
Id
Type
Payload
OccurredOn
Processed
```

Example:

```
Id = 101
Type = OrderCreated
Payload =
{
    "OrderId": 15,
    "Customer": "John"
}
Processed = false
```

## Flow Step by Step

Customer creates order.

### Step 1

Application starts SQL transaction.

### Step 2

Insert Order into Orders:

```
Id = 15
Type = Laptop
```

### Step 3

Insert Outbox row into Outbox:

```
Id = 101
Type = OrderCreated
Processed = false
```

### Step 4

Commit. Now BOTH exist, even if application crashes afterwards.

### Step 5

Background worker wakes up every few seconds.

```sql
SELECT *
FROM Outbox
WHERE Processed = false
```

### Step 6

Background worker publishes message. RabbitMQ: `OrderCreated`

### Step 7

Background worker marks row as processed: `Processed = true`

## Visual Timeline

### Without Outbox:

```
Save Order
 |
Database
 |
Publish Event
 |
RabbitMQ crashes
```

Result: Database updated, but no event sent

### With Outbox:

```
Transaction

Save Order
Save Event

Commit
```

Later:

```
Background Worker
 |
Publish Event
 |
Mark Processed
```

## Why It Works

The key idea is that the database is your **source of truth**.

If the order exists, the event also exists in the Outbox table because they were written in the same transaction.

Even if the application crashes immediately after the commit, the event remains in the Outbox and will be published when the background worker restarts.

## Typical .NET Implementation

### Entity

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

### Service

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

Notice that **only the database is involved in the transaction** — there is no call to RabbitMQ or Kafka here.

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

## What if publishing fails?

Suppose RabbitMQ is unavailable.

```
Processed = false
```

The worker tries again later:

```
Retry
Retry
Retry
```

Eventually it succeeds. This provides **eventual consistency**: all systems become consistent over time rather than instantly.

## Duplicate Messages

A subtle issue arises if the worker publishes successfully but crashes before marking the Outbox row as processed:

```
✅ Publish

❌ Application crashes

Processed = false
```

After restarting, the worker publishes the same event again.

Therefore, the Outbox Pattern guarantees **at-least-once delivery**, not exactly-once delivery.

Consumers should therefore be **idempotent**, meaning they can safely process the same event multiple times without causing incorrect results (for example, by tracking processed message IDs).

## Pros

- ✅ Prevents lost events.
- ✅ Keeps database updates and event creation atomic.
- ✅ No distributed transactions required.
- ✅ Works with RabbitMQ, Kafka, Azure Service Bus, and other brokers.
- ✅ Widely used in microservices and event-driven architectures.

## Cons

- ❌ Additional Outbox table to maintain.
- ❌ Background worker adds complexity.
- ❌ Messages are delivered asynchronously (not immediately).
- ❌ Requires idempotent consumers because duplicates are possible.

## Cheat Sheet

| Problem                     | Solution                                                            |
| --------------------------- | ------------------------------------------------------------------- |
| DB saved, message lost      | Store the message in an Outbox table within the same DB transaction |
| Message broker unavailable  | Background worker retries later                                     |
| App crashes after DB commit | Message remains in Outbox and is published after restart            |
| Duplicate message possible  | Yes; consumers must be idempotent                                   |
| Transaction scope           | Database only (business data + Outbox row)                          |
| Delivery guarantee          | At least once                                                       |
| Consistency model           | Eventual consistency                                                |

# Interview Tips

**Memory trick:** Think of the Outbox as a **"mailroom"** inside your database. Your business transaction drops a letter (the event) into the mailroom at the same time it saves the business data. A separate mail carrier (background worker) later delivers the letter to RabbitMQ or Kafka. Even if the mail carrier is delayed or crashes, the letter is safely waiting in the mailroom.

Why is the Outbox Pattern needed?

> To guarantee that a database update and an event are recorded atomically, avoiding situations where one succeeds and the other fails.

Why not publish directly to RabbitMQ after `SaveChanges()`?

> If publishing fails after the database commit, the event is lost and other services never learn about the change.

Why isn't a distributed transaction used?

> Distributed transactions (2PC/MSDTC) are complex, slower, and often unsupported or discouraged in modern cloud-native systems.

What consistency model does the Outbox Pattern provide?

> **Eventual consistency.** Other services may not see the change immediately, but they will once the Outbox message is successfully published.

Does the Outbox Pattern guarantee exactly-once delivery?

> No. It guarantees **at-least-once delivery**. Consumers must be idempotent to safely handle duplicate messages.
