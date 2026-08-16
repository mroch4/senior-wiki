# Saga Pattern

## Table of content

1. [What is Saga Pattern?](#what-is-saga-pattern)
2. [Example](#example)
   - [Step 1](#step-1)
   - [Step 2](#step-2)
   - [Step 3](#step-3)
   - [Step 4](#step-4)
   - [Step 5](#step-5)
3. [Can we use one SQL transaction?](#can-we-use-one-sql-transaction)
4. [Visual Flow](#visual-flow)
5. [Compensation](#compensation)
6. [Types of Saga](#types-of-saga)
   - [Choreography](#choreography)
     - [Pros](#pros)
     - [Cons](#cons)
   - [Orchestration](#orchestration)
     - [Pros](#pros-1)
     - [Cons](#cons-1)
7. [.NET Example (Conceptual)](#net-example-conceptual)
8. [Saga vs Outbox](#saga-vs-outbox)
9. [Outbox + Saga Together](#outbox--saga-together)
10. [Cheat Sheet](#cheat-sheet)
11. [Interview Tips](#interview-tips)

## What is Saga Pattern?

> A **Saga** is a sequence of **local transactions**, where each successful step triggers the next step. If one step fails, previously completed steps execute **compensating transactions** (undo actions).

Instead of:

```
BEGIN TRANSACTION

Step 1
Step 2
Step 3

COMMIT
```

You have:

```
Step 1
 |
Step 2
 |
Step 3
 |
Done
```

If something fails:

```
Undo Step 2
 |
Undo Step 1
```

## Example

Customer orders a laptop.

### Step 1

- **Order Service:** `Create Order`
- Status: `Pending`
- Publish: `OrderCreated`

### Step 2

**Inventory Service:**

- Receives: `OrderCreated`
- Reserves stock.
- Publishes:`InventoryReserved`

### Step 3

**Payment Service:**

- Receives: `InventoryReserved`
- Charges customer.
- ❌ Suppose payment fails.
- Publishes: `PaymentFailed`

### Step 4

**Inventory Service:**

- Receives: `PaymentFailed`
- Undo reservation.
- Release inventory.
- Publishes:`InventoryReleased`

### Step 5

**Order Service:**

- Receives: `InventoryReleased`
- Changes order status: `Cancelled`

## Can we use one SQL transaction?

No. Each service owns its own database:

```
Order DB
Inventory DB
Payment DB
```

Microservices **must not** share a transaction. This is exactly where Saga comes in.

## Visual Flow

```
Create Order
 |
Reserve Inventory
 |
Charge Payment
 |
❌ FAIL
 |
Inventory
 |
Cancel Order
```

Notice no global rollback. Instead, every service performs its own compensation.

## Compensation

A compensating transaction is **not always the exact reverse** of the original action.

Examples:

- Reserve inventory -> Release inventory
- Charge credit card -> Issue refund
- Book hotel -> Cancel reservation
- Send email -> Send another email: `Previous confirmation has been cancelled.`

You can't "unsend" an email. This is why compensation is business-specific rather than database-specific.

## Types of Saga

### Choreography

No central coordinator - each service reacts to events. Every service only knows what to do next.

```
OrderCreated
 ↓
Inventory
 ↓
InventoryReserved
 ↓
Payment
 ↓
PaymentCompleted
 ↓
Shipping
 ↓
ShipmentCreated
```

#### Pros

- ✅ Simple
- ✅ No central point of failure
- ✅ Loosely coupled

#### Cons

- ❌ Event flows become difficult to understand as systems grow
- ❌ Debugging is harder because control is distributed

### Orchestration

A central **Saga Orchestrator** coordinates the workflow:

```
     Orchestrator
    /     |      \
Order Inventory Payment
```

Flow:

```
Orchestrator
 ↓
Create Order
 ↓
Reserve Inventory
 ↓
Charge Payment
 ↓
Ship Order
```

If payment fails:

```
Orchestrator
 ↓
Release Inventory
 ↓
Cancel Order
```

#### Pros

- ✅ Easier to visualize and monitor
- ✅ Centralized business workflow

#### Cons

- ❌ More infrastructure
- ❌ Orchestrator becomes a critical component

## .NET Example (Conceptual)

Order service publishes:

```csharp
await publisher.Publish(new OrderCreated(orderId));
```

Inventory consumer:

```csharp
public async Task Consume(OrderCreated message)
{
    await ReserveInventory();

    await publisher.Publish(new InventoryReserved(message.OrderId));
}
```

Payment consumer:

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

Order consumer:

```csharp
public async Task Consume(PaymentFailed message)
{
    order.Status = OrderStatus.Cancelled;
}
```

## Saga vs Outbox

| Feature             | Outbox                    | Saga                                        |
| ------------------- | ------------------------- | ------------------------------------------- |
| Purpose             | Reliable event publishing | Coordinate a multi-service business process |
| Scope               | One service               | Multiple services                           |
| Solves              | Lost messages             | Distributed transactions                    |
| Uses DB transaction | Yes (local only)          | No global transaction                       |
| Rollback            | Retry publishing          | Compensating actions                        |
| Consistency         | Eventual                  | Eventual                                    |

Think of it this way:

Outbox:

```
Database
 ↓
Reliable event
```

Saga:

```
Event
 ↓
Business workflow
 ↓
More events
 ↓
More services

```

The Outbox ensures each service publishes events reliably; the Saga uses those events to drive a distributed business process.

## Outbox + Saga Together

A typical production architecture looks like this:

```
Client
   │
   ▼
Order Service
   │
   ├── Save Order
   ├── Save Outbox Event
   └── Commit
        │
        ▼
Outbox Worker
        │
        ▼
Message Broker
        │
        ▼
Inventory Service
        │
        ├── Save Reservation
        ├── Save Outbox Event
        └── Commit
        │
        ▼
Payment Service
        │
        ▼
Shipping Service
```

Notice that **each service uses the Outbox Pattern locally**, while the **Saga spans across all services**.

## Cheat Sheet

| Pattern           | Think of it as                         | Solves                                                                         |
| ----------------- | -------------------------------------- | ------------------------------------------------------------------------------ |
| **Outbox**        | A reliable "mailroom" for events       | Prevents lost messages after database commits                                  |
| **Saga**          | A workflow manager with undo steps     | Coordinates long-running transactions across services                          |
| **Compensation**  | Business undo                          | Restores consistency when a step fails                                         |
| **Choreography**  | Services "dance" by reacting to events | Decentralized coordination                                                     |
| **Orchestration** | A conductor directs the workflow       | Centralized coordination                                                       |
| **Consistency**   | Eventual                               | Temporary inconsistencies are expected until the Saga completes or compensates |

# Interview Tips

Why do we need a Saga?

> Because a single database transaction cannot span multiple microservices. A Saga coordinates local transactions and maintains consistency through compensating actions.

What is a compensating transaction?

> A business action that semantically undoes a previously completed step (e.g., releasing inventory, refunding a payment, cancelling an order).

What is the difference between choreography and orchestration?

- **Choreography:** Services react to events without a central controller.
- **Orchestration:** A dedicated orchestrator decides which step runs next and what to do on failure.

Does a Saga guarantee ACID consistency?

> No. It provides **eventual consistency**. During execution, different services may temporarily have different views of the process.

Can you use Outbox without Saga?

> Yes. If you simply need reliable event publication from a single service, the Outbox Pattern is sufficient.

Can you use Saga without Outbox?

> You can, but it's risky. If an event is lost due to a crash or broker outage, the Saga can become stuck because the next service never receives the event. This is why production systems commonly pair the Saga Pattern with the Outbox Pattern.
