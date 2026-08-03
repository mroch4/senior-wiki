# Event-Driven Architecture (EDA)

## Table of content

## What is Event-Driven Architecture (EDA)?

> Event-Driven Architecture (EDA) is an architectural style where **components communicate by publishing and reacting to events**, rather than directly calling each other.

Instead of saying:

> "Do this now."

A service says:

> "This happened."

Other services decide whether they care about that event.

## Best Practices

- Use **past-tense** names (`OrderPlaced`, `PaymentSucceeded`).
- Keep events immutable.
- Include a unique `EventId` for idempotency.
- Include a timestamp and correlation ID for tracing.
- Make consumers idempotent.
- Use the **Outbox Pattern** for reliable publishing.
- Expect eventual consistency.
- Monitor message queues and dead-letter queues (DLQs).
- Version events in a backward-compatible way.
- Avoid exposing internal domain models directly in integration events.

# Interview Tips

- Clearly distinguish **Domain Events** (inside a bounded context) from **Integration Events** (shared between services). Domain events model internal business occurrences, while integration events communicate across service boundaries.
- Mention that EDA is **not limited to microservices**. Modular monoliths can also benefit from events to decouple modules.
- Explain that EDA typically favors **asynchronous communication**, but synchronous APIs are still appropriate for immediate user interactions or queries.
- Be ready to discuss trade-offs such as eventual consistency, observability, retry policies, dead-letter queues, idempotency, and distributed tracing. These operational concerns often matter as much as the architecture itself in production systems.

What is Event-Driven Architecture?

> An architectural style where components communicate by publishing and subscribing to events rather than calling each other directly, resulting in loose coupling and better scalability.

What is the difference between a command and an event?

> A command requests an action and targets a specific handler. An event represents something that has already happened and can be consumed by multiple subscribers.

Why are consumers required to be idempotent?

> Because brokers often provide **at-least-once delivery**, meaning the same event may be delivered multiple times. Idempotent consumers ensure processing the same event repeatedly has no unintended side effects.

Why is the Outbox Pattern commonly used with EDA?

> To guarantee that database changes and event publication remain consistent. The event is first stored in the same database transaction as the business data, then published asynchronously, preventing lost or phantom events.

When is Event-Driven Architecture a good fit?

> For distributed systems, microservices, asynchronous workflows, systems requiring high scalability, integrations with external services, and applications where eventual consistency is acceptable.

❌
✅

---

# Core Idea

Imagine an online shop.

Traditional request-response:

```
Order API
    │
    ├── Call Payment Service
    ├── Call Inventory Service
    ├── Call Email Service
    └── Call Analytics Service
```

Every service knows about the others.

In Event-Driven Architecture:

```
Customer places order
        │
        ▼
Order Service
        │
Publishes:
OrderPlaced
        │
        ▼
 Event Bus
 ├───────────────┬─────────────┬─────────────┐
 ▼               ▼             ▼             ▼
Payment      Inventory      Email      Analytics
```

The Order Service doesn't know who is listening.

# What is an Event?

An event is simply a fact that already happened.

Examples:

- UserRegistered
- OrderPlaced
- PaymentSucceeded
- ProductCreated
- InvoiceGenerated
- ShipmentDelivered

Notice the naming.

Events are always in the past tense.

Good:

```
OrderPlaced
```

Bad:

```
PlaceOrder
```

Because commands tell someone what to do.

Events describe what already happened.

# Main Components

## Event Producer

Produces events.

```
Customer clicks Buy

↓

Order Service

↓

Publishes OrderPlaced
```

## Event Broker

Receives events and distributes them.

Examples:

- Apache Kafka
- RabbitMQ
- Azure Service Bus
- AWS SNS/SQS
- Google Pub/Sub

The broker is not the business logic.

It simply moves messages around.

## Event Consumer

Subscribes to events.

```
OrderPlaced

↓

Inventory Service
```

or

```
OrderPlaced

↓

Email Service
```

Consumers only process events they care about.

# Event Flow Example

```
Customer

↓

POST /orders

↓

Order Service

↓

Save Order

↓

Publish:

OrderPlaced
```

Broker receives:

```
OrderPlaced
```

Then:

```
Payment Service
    ↓
Charge card

Inventory Service
    ↓
Reserve stock

Email Service
    ↓
Send confirmation

Analytics Service
    ↓
Update dashboard
```

None of these services know about each other.

# Benefits

## Loose Coupling

Without EDA:

```
Order Service

↓

Payment Service
Inventory Service
Shipping Service
Email Service
```

Many dependencies.

With EDA:

```
Order Service

↓

Event Bus

↓

Everyone else
```

Order Service only knows the broker.

## Scalability

Consumers scale independently.

Example:

```
Email Service

1 instance

↓

10 instances
```

Nothing changes for the producer.

## Extensibility

Want SMS notifications?

Just create:

```
SMS Service

↓

Subscribe to:

OrderPlaced
```

No changes to Order Service.

## Better Separation

Each service owns one responsibility.

Order Service:

- create orders

Email Service:

- send emails

Inventory Service:

- manage stock

# Drawbacks

## Eventual Consistency

Data is not updated everywhere immediately.

Example:

```
Order Created

↓

Inventory updates 2 seconds later

↓

Email updates 3 seconds later
```

For a brief moment, services have different views of the system.

## Harder Debugging

Instead of:

```
API

↓

Database
```

You now have:

```
API

↓

Kafka

↓

Payment

↓

Inventory

↓

Shipping

↓

Email
```

Tracing a request requires distributed tracing (e.g., OpenTelemetry).

## Duplicate Messages

A broker may deliver the same event more than once.

Consumers must be **idempotent**.

Bad:

```
Receive event

↓

Charge customer
```

Duplicate event:

```
Charge again
```

Good:

```
Check EventId

Already processed?

↓

Ignore duplicate
```

## Ordering

Events may arrive out of order.

Example:

```
PaymentCompleted

arrives before

OrderCreated
```

Consumers must handle such scenarios gracefully.

# Commands vs Events

| Command             | Event                |
| ------------------- | -------------------- |
| Do something        | Something happened   |
| Imperative          | Past tense           |
| One receiver        | Many receivers       |
| Usually synchronous | Usually asynchronous |
| Expects action      | Describes history    |

Example:

Command:

```
CreateOrder
```

Event:

```
OrderCreated
```

# Event Notification vs Event-Carried State

## Event Notification

Very little information.

```
OrderPlaced

OrderId = 123
```

Consumer fetches details itself.

Advantages:

- small messages
- fresh data

Disadvantages:

- additional database/API calls

## Event-Carried State Transfer

Entire object included.

```
OrderPlaced

OrderId
CustomerId
Address
Items
Price
Discount
Currency
```

Advantages:

- no additional queries
- better decoupling

Disadvantages:

- larger events
- duplicated data
- versioning becomes more challenging

# Delivery Guarantees

## At Most Once

```
Delivered

or

Lost
```

Never duplicated.

May disappear.

## At Least Once

```
Delivered

Maybe twice
```

Most common.

Consumers must be idempotent.

## Exactly Once

```
Delivered once
```

Very difficult in distributed systems. Technologies like Kafka provide support under specific conditions, but application logic still needs careful design.

# Event Versioning

Events evolve over time.

Version 1

```json
{
  "OrderId": 10,
  "CustomerId": 5
}
```

Version 2

```json
{
  "OrderId": 10,
  "CustomerId": 5,
  "Currency": "EUR"
}
```

Best practices:

- Add new fields instead of removing existing ones.
- Make new fields optional when possible.
- Avoid breaking changes to existing consumers.

# Event Sourcing vs Event-Driven Architecture

These are often confused.

## Event-Driven Architecture

Events are used for communication.

```
OrderPlaced

↓

Email Service
```

The database stores the current state.

```
Order

Status = Paid
```

## Event Sourcing

Events are the source of truth.

```
OrderCreated

↓

PaymentReceived

↓

ItemAdded

↓

ItemRemoved

↓

OrderCancelled
```

Current state is reconstructed by replaying the event history.

You can use:

- Event-Driven Architecture without Event Sourcing (most common)
- Event Sourcing without a distributed event bus
- Both together

# EDA in .NET

Common stack:

```
ASP.NET Core API

↓

MediatR Domain Events

↓

Outbox Pattern

↓

Background Worker

↓

Kafka / RabbitMQ / Azure Service Bus

↓

Consumers
```

Typical flow:

```
HTTP Request

↓

Application Layer

↓

Domain Entity raises Domain Event

↓

Save changes

↓

Outbox Pattern stores Integration Event

↓

Background Service publishes event

↓

Consumers process it
```

This avoids publishing events if the database transaction fails.
