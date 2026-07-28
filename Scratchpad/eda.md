# High-Level View

## Monolith

```text
+----------------------------------+
|           Application            |
|----------------------------------|
| Orders | Payments | Inventory    |
+----------------------------------+
```

Everything is in one process.

---

## Microservices

```text
+---------+   +-----------+   +------------+
| Orders  |   | Payments  |   | Inventory  |
+---------+   +-----------+   +------------+

Orders calls Payments
Orders calls Inventory
```

The application is split into independently deployable services.

Communication is often synchronous (HTTP/gRPC).

---

## Event-Driven Microservices

```text
Orders

↓

OrderPlaced

↓

Kafka / RabbitMQ

↓

Payments
Inventory
Email
Analytics
Shipping
```

Now the services communicate asynchronously using events.

---

# Main Difference

| Microservices                      | Event-Driven Architecture   |
| ---------------------------------- | --------------------------- |
| Defines system decomposition       | Defines communication style |
| Services own business capabilities | Components react to events  |
| Can use REST/gRPC                  | Uses events/messages        |
| Often synchronous                  | Mostly asynchronous         |
| Independent deployment             | Loose communication         |

Notice they address different concerns.

---

# Can You Have One Without the Other?

## Microservices WITHOUT EDA

Very common.

```text
Orders

↓

HTTP

↓

Payments

↓

HTTP

↓

Inventory
```

Every request waits for the next service.

Advantages:

- Simpler
- Easier debugging
- Immediate responses

Disadvantages:

- Tight runtime coupling
- Cascading failures
- Higher latency

---

## EDA WITHOUT Microservices

Also common.

Example:

```text
Modular Monolith

Modules:

Orders
Inventory
Payments
Notifications
```

Instead of modules calling each other:

```text
Orders

↓

OrderPlaced

↓

Notifications
Inventory
```

Everything is inside one application but still loosely coupled.

Many teams adopt this approach before splitting into microservices.

---

# Coupling Comparison

## REST

```text
Orders

↓

POST /payments
```

Orders must know:

- Payment URL
- API contract
- Authentication
- Timeout
- Retry policy

Strong coupling.

---

## Event

```text
Orders

↓

Publish OrderPlaced
```

Orders knows only:

- event schema
- broker

It doesn't know:

- who consumes it
- how many consumers exist
- whether any consumer exists

Much looser coupling.

---

# Failure Handling

## REST

```text
Orders

↓

Payment
```

Payment is down.

Result:

```text
Order fails.
```

---

## Event

```text
Orders

↓

Broker

↓

Payment (currently offline)
```

The event remains in the broker.

Payment processes it when it comes back online.

This increases resilience.

---

# Latency

## REST

```text
Customer

↓

Orders

↓

Payments

↓

Inventory

↓

Shipping

↓

Response
```

Every call adds latency.

---

## Event

```text
Customer

↓

Orders

↓

Response immediately

↓

Background processing

↓

Payments

Inventory

Shipping
```

The user doesn't wait for all downstream work.

---

# Data Ownership

Both architectures encourage **database-per-service**.

```text
Orders DB

Payments DB

Inventory DB
```

EDA complements this by synchronizing data through events instead of direct database access.

---

# Consistency

REST often enables stronger consistency:

```text
Create Order

↓

Charge Payment

↓

Reserve Inventory

↓

Commit
```

EDA usually embraces eventual consistency:

```text
Create Order

↓

Publish OrderPlaced

↓

Payment later

↓

Inventory later
```

Different services may temporarily have different views of the data.

---

# Scalability

REST:

```text
Orders

↓

Payments
```

Payments becomes a bottleneck if every request depends on it.

---

EDA:

```text
OrderPlaced

↓

Broker

↓

10 Payment Consumers
```

Consumers can scale independently.

---

# Real Example

### REST-Based Checkout

```text
Customer

↓

Orders

↓

Payments

↓

Inventory

↓

Shipping

↓

Email

↓

Response
```

If Email is slow:

Everything is slow.

---

### Event-Driven Checkout

```text
Customer

↓

Orders

↓

OrderPlaced

↓

Response
```

Background:

```text
Payment

↓

PaymentSucceeded

↓

Inventory

↓

InventoryReserved

↓

Shipping

↓

ShipmentCreated

↓

Email
```

Each step proceeds independently, improving responsiveness.

---

# Which One Should You Choose?

## REST/gRPC

Best for:

- Queries
- Immediate user feedback
- CRUD operations
- Simple request-response flows
- Operations requiring an immediate result

---

## Event-Driven

Best for:

- Notifications
- Background work
- Long-running workflows
- High scalability
- Integrations
- Loose coupling

---

## Modern Systems Use Both

A typical microservices architecture combines synchronous and asynchronous communication:

```text
Client

↓

Order API

↓

HTTP

↓

Payment Service
```

Once the payment succeeds:

```text
PaymentSucceeded

↓

Broker

↓

Shipping

↓

Analytics

↓

Email

↓

Fraud Detection
```

This hybrid approach keeps user-facing operations responsive while allowing downstream processing to scale independently.

---

# Interview Questions

### Is Event-Driven Architecture an alternative to Microservices?

No. Microservices define how an application is decomposed into independently deployable services, while Event-Driven Architecture defines how components communicate. They are complementary and are frequently used together.

---

### Can a microservices system be built without events?

Yes. Many systems use only synchronous communication such as REST or gRPC. However, this increases runtime coupling and can reduce resilience.

---

### Can a monolith use Event-Driven Architecture?

Yes. A modular monolith can use in-process events to decouple modules. This often makes a future migration to microservices easier because the interaction patterns are already loosely coupled.

---

### When should you prefer asynchronous communication?

When the caller does not need an immediate response, such as sending emails, updating analytics, processing notifications, or coordinating long-running business processes.

# Interview Tips

A strong interview answer is:

> "Microservices and Event-Driven Architecture solve different problems. Microservices define service boundaries, while EDA defines communication patterns. In practice, most production systems use both: synchronous APIs (REST/gRPC) for request-response interactions and asynchronous events for cross-service notifications, background processing, and improving resilience."
