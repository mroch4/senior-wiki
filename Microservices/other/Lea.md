# 1. How would you identify and define bounded contexts in a large financial services system?

A **bounded context** is a boundary within which a particular domain model and its terminology have a well-defined meaning.

In a financial system, I wouldn't start by splitting services according to technical layers such as `CustomerService`, `DatabaseService`, etc. I would start with the **business capabilities and domain language**.

For example:

```text
Financial Platform
│
├── Customer Management
├── Account Management
├── Payments
├── Loans
├── Risk & Fraud
├── Trading
├── Settlement
├── Reporting
└── Notifications
```

Each area can potentially become a bounded context.

For example, the concept of `Customer` may mean something different in different contexts:

```text
Customer Context
    Customer
    Address
    ContactDetails

Risk Context
    CustomerRiskProfile
    RiskScore
    RiskCategory

Payments Context
    Payer
    Payee
```

The same real-world person doesn't necessarily need to be represented by the same domain object everywhere.

### How I'd identify them

I'd look at:

1. **Business capabilities**
2. **Business processes**
3. **Ubiquitous language**
4. **Different business rules**
5. **Ownership of data**
6. **Team boundaries**
7. **Transaction boundaries**
8. **Frequency and nature of communication between areas**

A strong indication that two areas should be separate bounded contexts is when they have **different models or business rules around the same concept**.

> A bounded context is primarily a **domain boundary**, not automatically a microservice boundary.

A bounded context can eventually be implemented as one or more microservices.

---

# 2. What is Domain-Driven Design and why is it useful for complex backend systems?

**Domain-Driven Design (DDD)** is an approach to designing software around the **business domain and its rules**, rather than starting from technical infrastructure.

The key idea is that developers and business experts collaborate around a common **ubiquitous language**.

DDD has two major parts:

### Strategic DDD

Deals with the big picture:

- Domains
- Subdomains
- Bounded contexts
- Context maps
- Relationships between contexts

### Tactical DDD

Deals with the implementation:

- Entities
- Value Objects
- Aggregates
- Aggregate Roots
- Repositories
- Domain Services
- Domain Events

For example:

```text
Order
 ├── OrderId
 ├── CustomerId
 ├── OrderLines
 ├── ShippingAddress
 └── Status
```

`ShippingAddress` might be a value object rather than an entity because its identity isn't important.

DDD is particularly useful when the system contains **complex business rules**, such as banking, insurance, trading, payments or lending.

It helps prevent business logic from becoming scattered across controllers, repositories and database procedures.

---

# 3. What is CQRS and when would you apply it in a .NET microservices architecture?

**CQRS = Command Query Responsibility Segregation.**

It separates operations that **change state** from operations that **read state**.

```text
Command
   │
   ▼
Write Model
   │
 Database
```

versus:

```text
Query
   │
   ▼
Read Model
   │
 Database / Cache
```

A command might be:

```text
CreatePayment
ApproveLoan
PlaceOrder
```

A query might be:

```text
GetPaymentHistory
GetAccountSummary
GetCustomerPortfolio
```

CQRS doesn't necessarily mean two databases.

You can start with:

```text
         ┌── Commands ──> same DB
API ─────┤
         └── Queries ───> same DB
```

and evolve toward:

```text
Commands → Write DB → Events → Read DB
                         │
                         ▼
                    Read Models
```

### When I'd use CQRS

I'd consider it when:

- Read and write workloads are very different.
- Queries are significantly more complex than writes.
- Read models need to be optimized independently.
- The domain has complex commands/business rules.
- Event-driven architecture is already being used.
- Different scaling requirements exist for reads and writes.

I **wouldn't introduce CQRS automatically** into every microservice. For a simple CRUD service it can add unnecessary complexity.

---

# 4. What is eventual consistency?

In a distributed system, **eventual consistency** means that different parts of the system may temporarily have different versions of the data, but they should converge to a consistent state.

For example:

```text
Order Service
     │
     │ OrderCreated
     ▼
Message Broker
     │
     ├── Inventory Service
     │
     └── Notification Service
```

Immediately after the order is created:

```text
Order DB       = Order exists
Inventory DB   = May not have processed event yet
Notification   = May not have processed event yet
```

This is different from a single database transaction where everything commits atomically.

### Design implications

You need to think about:

- Retries
- Idempotency
- Duplicate messages
- Out-of-order events
- Dead-letter queues
- Transaction boundaries
- Failure recovery
- User-facing status

For example, instead of:

```text
POST /orders

200 OK
"Everything is complete"
```

you might return:

```text
202 Accepted
{
    "orderId": "...",
    "status": "Processing"
}
```

The system can then expose the current state through a query.

---

# 5. What is Event-Driven Architecture and how does it differ from request-response?

In traditional request-response:

```text
Service A
   │
   │ HTTP
   ▼
Service B
   │
   ▼
Response
```

Service A directly invokes Service B and waits for a response.

In Event-Driven Architecture:

```text
Service A
   │
   │ publishes event
   ▼
Message Broker
   │
   ├────> Service B
   ├────> Service C
   └────> Service D
```

The producer doesn't necessarily know which consumers exist.

For example:

```text
PaymentCompleted
```

could be consumed by:

```text
Accounting
Notifications
Fraud
Analytics
Loyalty
```

This provides **loose coupling** and makes it easier to add consumers without changing the producer.

The trade-off is increased complexity around:

- Eventual consistency
- Debugging
- Message delivery
- Ordering
- Idempotency
- Schema evolution

---

# 6. What is the role of RabbitMQ or Kafka in a distributed .NET application?

They provide **asynchronous communication between services**.

Instead of:

```text
Order → HTTP → Inventory
```

you can have:

```text
Order
  │
  ▼
RabbitMQ/Kafka
  │
  ▼
Inventory
```

This provides:

- Decoupling
- Asynchronous processing
- Buffering
- Retry mechanisms
- Scalability
- Fault isolation

### RabbitMQ

Often a good choice for:

- Commands
- Work queues
- Task processing
- Routing messages
- Request/response messaging

### Kafka

Often useful for:

- High-throughput event streaming
- Event history
- Analytics
- Multiple independent consumers
- Stream processing

They're not interchangeable in every scenario.

A useful interview answer is:

> "I choose the messaging technology based on the communication semantics and operational requirements rather than simply because the application uses microservices."

---

# 7. What is the purpose of OpenAPI in microservices?

OpenAPI defines the **HTTP API contract**.

For example:

```yaml
GET /customers/{id}
```

can define:

```text
Request
Response
Status codes
Parameters
Authentication
Schemas
```

This gives teams a shared understanding of the API before implementation.

In .NET, **ASP.NET Core + Swagger/OpenAPI** can automatically expose API documentation.

It can also support:

```text
OpenAPI
   │
   ├── Client generation
   ├── Server stubs
   ├── Documentation
   ├── Contract testing
   └── Validation
```

The important concept is:

> OpenAPI can become a machine-readable API contract between teams.

---

# 8. Unit vs integration vs contract tests

### Unit tests

Test a small piece of code in isolation.

```text
OrderCalculator
       │
       ▼
Unit Test
```

Dependencies are normally mocked.

They're:

- Fast
- Numerous
- Deterministic

---

### Integration tests

Test multiple components working together.

For example:

```text
API
 ↓
Application
 ↓
Repository
 ↓
PostgreSQL
```

You might use:

- Testcontainers
- Docker
- Real database instances
- Real message brokers

---

### Contract tests

Verify that two independently developed services agree on their communication contract.

For example:

```text
Order Service
      │
      │ API contract
      ▼
Payment Service
```

They are particularly valuable when teams deploy independently.

A simple distinction:

```text
Unit       → Does my code work?
Integration → Do my components work together?
Contract    → Do independently deployed services agree?
```

---

# 9. What is observability?

Observability is the ability to understand **what is happening inside a system from its external outputs**.

The three main pillars are:

```text
           Observability
          /      |       \
       Logs    Metrics   Traces
```

### Logs

Tell you **what happened**.

```text
Payment failed for paymentId=123
```

### Metrics

Tell you **how much/how often**.

```text
HTTP request rate
Error rate
CPU
Memory
Latency
Queue depth
```

### Distributed traces

Tell you **where a request travelled**.

```text
API
 │
 ├── Order Service
 │      │
 │      └── Payment Service
 │
 └── Notification Service
```

A trace ID can connect logs from multiple services to the same request.

In modern .NET systems, **OpenTelemetry** is commonly used to standardize collection of traces, metrics and logs.

---

# 10. How would you design a .NET microservice for long-running asynchronous operations?

I wouldn't keep an HTTP request open for several minutes.

Instead:

```text
POST /reports
       │
       ▼
Create Job
       │
       ▼
202 Accepted
{
   "jobId": "123"
}
```

Then:

```text
Message Broker
      │
      ▼
Background Worker
      │
      ▼
Generate Report
      │
      ▼
Store Result
```

The client can then query:

```text
GET /reports/123
```

and receive:

```json
{
  "status": "Completed",
  "downloadUrl": "..."
}
```

In .NET, the worker could be implemented using:

```text
BackgroundService
IHostedService
Worker Service
```

For production systems I'd also consider:

- Idempotency
- Retry policies
- Dead-letter queues
- Job status
- Cancellation
- Timeouts
- Progress tracking
- Persistent job state

---

# 11. Relational vs NoSQL — PostgreSQL/MSSQL vs MongoDB

### Relational

Examples:

- PostgreSQL
- SQL Server

Data is typically structured around:

```text
Tables
Relationships
Constraints
Transactions
Indexes
```

They're excellent for transactional systems.

For example:

```text
Account
Transaction
Customer
Payment
```

where relationships and transactional consistency are important.

### MongoDB

Document-oriented:

```json
{
  "customerId": "123",
  "name": "John",
  "addresses": [...]
}
```

It can be useful when:

- Data is naturally document-shaped.
- Schema flexibility is valuable.
- Aggregates can be stored together.
- Horizontal scaling requirements favor the document model.

For financial transactional workloads, I'd generally lean toward **PostgreSQL or SQL Server** when strong relational integrity, ACID transactions, complex joins and mature transactional tooling are central requirements.

The database choice should follow the **access patterns and consistency requirements**, not simply "microservices = NoSQL."

---

# 12. How have you applied DDD tactical patterns?

A good experience-based answer could be:

> "I use aggregates to define transactional consistency boundaries. The aggregate root controls changes to the aggregate rather than allowing external code to arbitrarily modify its internal entities."

For example:

```text
Order  ← Aggregate Root
│
├── OrderLine
├── OrderLine
└── ShippingAddress
```

The application interacts with:

```csharp
order.AddLine(product, quantity);
```

rather than directly manipulating internal collections.

### Value Objects

Used for concepts defined by their value rather than identity:

```csharp
Money
EmailAddress
Address
AccountNumber
```

For example:

```text
Money
 ├── Amount
 └── Currency
```

### Repository

Provides persistence-oriented access to an aggregate:

```csharp
IOrderRepository
{
    Task<Order?> GetByIdAsync(OrderId id);
    Task AddAsync(Order order);
}
```

The domain shouldn't need to know whether persistence uses EF Core, PostgreSQL or SQL Server.

---

# 13. How have you used publish-subscribe or event sourcing?

### Publish-subscribe

One service publishes an event:

```text
OrderCreated
```

and multiple consumers subscribe:

```text
          OrderCreated
               │
       ┌───────┼────────┐
       ▼       ▼        ▼
 Inventory  Billing  Notification
```

This is useful when multiple parts of the system need to react independently.

### Event Sourcing

Event Sourcing is different.

Instead of storing only the current state:

```text
Account
Balance = 500
```

you store the sequence of events:

```text
AccountOpened
MoneyDeposited 1000
MoneyWithdrawn 300
MoneyWithdrawn 200
```

Current state can be reconstructed by replaying events.

It's powerful but introduces significant complexity.

I wouldn't use Event Sourcing simply because an application uses Kafka or RabbitMQ.

---

# 14. How have you used OpenAPI or AsyncAPI across teams?

I'd establish the contract **before implementation** where practical.

For synchronous APIs:

```text
OpenAPI
   ↓
Consumer + Provider
```

For asynchronous messaging:

```text
AsyncAPI
   ↓
Event schema
   ↓
Producer + Consumers
```

The contract can define:

```text
Event name
Schema
Required fields
Optional fields
Version
Headers
Error semantics
```

CI/CD can validate that changes don't unintentionally break consumers.

For example:

```text
Pull Request
     │
     ▼
Contract validation
     │
     ├── Compatible → Build
     └── Breaking   → Fail
```

This is particularly valuable when multiple teams deploy independently.

---

# 15. How would you structure and automate testing?

I'd normally have several layers:

```text
              Tests
                │
       ┌────────┼────────┐
       ▼        ▼        ▼
      Unit  Integration Contract
```

### Unit

Run on every build:

```text
dotnet test
```

Fast feedback.

### Integration

Use real infrastructure where appropriate.

For example:

```text
.NET
 │
 ├── PostgreSQL container
 ├── RabbitMQ container
 └── API
```

**Testcontainers for .NET** is useful here.

### Contract

Validate API/message contracts between services.

Then CI could look like:

```text
Commit
  ↓
Build
  ↓
Unit tests
  ↓
Integration tests
  ↓
Contract tests
  ↓
Security checks
  ↓
Deploy
```

---

# 16. How have you implemented logging, metrics and distributed tracing?

A strong modern .NET answer would mention **structured logging + OpenTelemetry**.

For example:

```text
Incoming HTTP request
        │
        ▼
Trace ID
        │
        ├── Order Service
        │       │
        │       ▼
        │   PostgreSQL
        │
        └── Payment Service
                │
                ▼
             RabbitMQ
```

Structured logs might contain:

```text
timestamp
traceId
spanId
service
operation
customerId
orderId
duration
status
```

Metrics could include:

```text
http_requests_total
http_request_duration
payment_failures
queue_depth
database_latency
```

This allows you to answer questions such as:

> "Why did this customer's request take 8 seconds?"

rather than simply knowing that an error occurred.

---

# 17. How would you design PostgreSQL or MSSQL for high-volume transactional workloads?

I'd start from the **access patterns**, not the tables.

I'd consider:

### Proper indexing

```sql
CREATE INDEX IX_Transactions_AccountId_Date
ON Transactions(AccountId, TransactionDate);
```

### Appropriate normalization

Avoid unnecessary duplication for transactional data, while considering denormalization where it provides a measurable benefit.

### Transactions

Use database transactions for operations that must be atomic.

### Concurrency

Consider:

- Isolation levels
- Optimistic concurrency
- Row/version columns
- Locking

### Partitioning

For very large transaction tables, partitioning by something such as:

```text
TransactionDate
```

can help manage large datasets.

### Query optimization

Use:

```text
EXPLAIN
EXPLAIN ANALYZE
```

in PostgreSQL, or the equivalent SQL Server tooling, to understand query plans.

I'd also monitor:

```text
CPU
IO
locks
deadlocks
connection pool
query latency
index usage
```

---

# 18. How have you worked with nontechnical stakeholders to create DDD-aligned solutions?

This is really testing whether you can **discover the domain**, rather than just implement tickets.

I'd start with conversations around:

```text
What business problem are we solving?
Who performs the process?
What are the business rules?
What can go wrong?
What does each term mean?
What decisions are made?
```

I'd use techniques such as:

- Domain workshops
- Event Storming
- Process mapping
- Example scenarios
- Domain terminology/glossary

For example, a stakeholder might say:

> "A payment is completed."

I'd clarify:

```text
What does "completed" mean?
```

Does it mean:

```text
Payment requested?
Payment authorized?
Payment accepted?
Funds reserved?
Funds settled?
```

Those distinctions can lead to completely different domain events and states.

The goal is to create a shared language:

```text
Business
   ↕
Ubiquitous Language
   ↕
Domain Model
   ↕
.NET implementation
```

---

# The bigger picture

These questions are strongly connected. I'd learn them as one architecture rather than as 18 isolated definitions:

```text
                    DDD
                     │
             Bounded Contexts
                     │
          ┌──────────┴──────────┐
          ▼                     ▼
      Microservice          Microservice
          │                     │
       CQRS / DDD            CQRS / DDD
          │                     │
          └──────────┬──────────┘
                     │
              Messaging
           RabbitMQ / Kafka
                     │
             Event-driven flow
                     │
          ┌──────────┴──────────┐
          ▼                     ▼
       PostgreSQL            SQL Server
                     │
                     ▼
              Observability
        Logs / Metrics / Traces
                     │
                     ▼
                OpenTelemetry
```

And the **most important interview distinction** is this:

> **DDD tells you how to model the business. Bounded contexts tell you where models have boundaries. Microservices are one possible deployment architecture for those boundaries. CQRS separates reads from writes when useful. Messaging enables asynchronous communication. Event-driven architecture uses events to decouple participants. Eventual consistency is a consequence you must explicitly design for.**

That relationship is much more valuable to understand than memorizing individual definitions.
