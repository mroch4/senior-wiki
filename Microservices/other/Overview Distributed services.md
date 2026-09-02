### Versioning

Service A

expects

```
Customer.Name
```

Service B returns

```
Customer.FullName
```

Breaking change.

Need API versioning.

### Monitoring

One request may travel through

Gateway
|
Order
|
Payment
|
Inventory
|
Notification

Finding the failure is difficult.

Need

- distributed tracing
- centralized logging
- correlation IDs

## Interview Question 8

# Orchestration vs Choreography

### Orchestration

One service coordinates everything.

```
Saga Orchestrator
 |
Inventory
 |
Payment
 |
Shipping
```

Easy to understand.

Single coordinator.

### Choreography

No central coordinator.

Events trigger events.

```
Order Created
 |
Inventory Reserved
 |
Payment Charged
 |
Shipment Created
```

Highly decoupled.

Harder to debug.

## Interview Question 10

**Why are retries dangerous?**

Imagine

```
Charge credit card
 |
Timeout
 |
Retry
 |
Charged twice
```

Need idempotency.

# Idempotency

Executing the same request multiple times gives the same result.

Example

```
POST /payments

Idempotency-Key:
12345
```

Second request
|
Ignored.

Critical for

- payments
- orders
- reservations

## Interview Question 12

**How do you trace requests across services?**

Use

Correlation ID

```
X-Correlation-ID

12345
```

Every service logs

```
12345
```

Then use

- OpenTelemetry
- Jaeger
- Zipkin
- Azure Monitor
- Application Insights

to reconstruct the request path.

# CAP Theorem

Another classic interview question.

A distributed system can guarantee at most two of:

- **Consistency (C):** all nodes see the same data at the same time.
- **Availability (A):** every request receives a response, even if some nodes have failed.
- **Partition Tolerance (P):** the system continues operating despite network partitions.

Because network partitions are unavoidable in distributed systems, the real trade-off is typically between **Consistency** and **Availability** during a partition.

Examples:

- Banking systems often favor **CP** (correctness over availability).
- DNS and many large-scale web systems often favor **AP** (availability with eventual consistency).
