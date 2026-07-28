## How do services communicate asynchronously?

- dead letter queue
- poison messages
- idempotency

# 3. Azure Service Bus / Kafka

They mention Service Bus.

Expect questions.

Difference between Queue and Topic?

Queue

One consumer.

Topic

Multiple subscribers.

Why not use HTTP instead?

Expected

HTTP couples services.

Messaging allows

- retries
- buffering
- asynchronous processing

How do you avoid duplicate message processing?

Expected

Idempotency

Store processed MessageId.

What is Dead Letter Queue?

Messages that couldn't be processed.

# 4. SQL Server

Very likely.

Explain clustered index.

Difference between clustered and nonclustered index.

How do indexes improve performance?

What causes slow queries?

Expected

Missing indexes

SELECT \*

Functions in WHERE

Poor joins

Statistics

Blocking

How do you investigate slow SQL?

Expected

Execution Plan

Query Store

SET STATISTICS IO

Profiler

DMVs

Difference between

INNER JOIN

LEFT JOIN

CROSS APPLY

OUTER APPLY

Normalization

When would you denormalize?

Transactions

Isolation Levels

Read Committed

Repeatable Read

Snapshot

Serializable

# 5. Cosmos DB

Even if you haven't worked with it.

They'll ask basics.

Difference between SQL Server and Cosmos DB.

Partition Key

Very common.

Why is choosing Partition Key important?

Expected

Data distribution

Performance

Cost

Cross-partition queries

Consistency Levels

Strong

Session

Eventual

RU (Request Units)

What affects RU consumption?

- document size
- indexing
- queries
- cross partition

# 8. API

How do you secure APIs?

JWT,OAuth,Azure AD,Roles,Claims

Swagger,What is OpenAPI?

# 9. Unit Testing

Mock vs Stub

When do you mock?

What shouldn't be mocked?

Difference

Unit test

Integration test

How do you test Minimal APIs?

# 10. Architecture

Very senior questions.

How do you structure a new backend project?

What design patterns do you commonly use?

Examples

Repository

Factory

Strategy

Mediator

Decorator

CQRS

Outbox

Saga

How do you log application errors?

Expected

ILogger

Structured logging

Correlation IDs

Application Insights

How do you handle configuration?

Expected

appsettings

Environment Variables

Azure Key Vault

Options Pattern

# 11. Performance

How do you improve API performance?

Expected

Caching

Indexes

Async

Compression

Pagination

Connection pooling

Minimal allocations

What causes memory leaks in .NET?

Expected

Events

Static collections

Long-lived references

Improper IDisposable handling

# 12. Practical Scenario Questions

These are increasingly common.

> An API suddenly becomes very slow. How do you investigate?

Expected approach

1. Check monitoring (Application Insights/logs)
2. Review recent deployments
3. Identify whether CPU, memory, database, or external dependencies are the bottleneck
4. Analyze slow SQL queries and execution plans
5. Check for blocking, deadlocks, or high latency to downstream services
6. Verify message queues and retries if asynchronous processing is involved
7. Reproduce locally or in a test environment if needed

> A message is processed twice. What do you do?

Discuss:

- Idempotent consumers
- Message IDs
- Deduplication
- Transactional outbox/inbox patterns
- At-least-once delivery semantics

> One microservice is unavailable. Should the whole application stop?

Talk about:

- Circuit breakers
- Retries with backoff
- Timeouts
- Graceful degradation
- Queueing work for later processing

# 13. Behavioral Questions

Expect some questions like:

- Tell me about a production incident you resolved.
- Describe a difficult technical decision you made.
- Have you ever disagreed with an architectural decision? How did you handle it?
- How do you review pull requests?
- How do you mentor junior developers?
- What's the most complex bug you've debugged?
- Describe a situation where you improved application performance.

## Topics I'd prioritize for this specific role

Given the description, I'd rank the interview focus roughly as:

1. **Microservices architecture** (communication, resilience, distributed systems)
2. **.NET Core/C# fundamentals** (DI, async/await, lifetimes, performance)
3. **SQL Server** (query optimization, indexing, execution plans)
4. **Messaging** (Azure Service Bus, Kafka, queues, topics, retries, DLQs)
5. **Azure services** (App Services, Functions, Application Insights, Storage)
6. **Cosmos DB and NoSQL concepts** (partitioning, consistency, RU consumption)
7. **Search technologies** (Azure AI Search or Elasticsearch fundamentals)
8. **Testing and clean architecture** (unit testing, code quality, design patterns)

This aligns well with the areas we've already been covering. The remaining topics I'd recommend studying in depth are **Azure services (especially App Services, Functions, and Application Insights), Cosmos DB, Azure AI Search/Elasticsearch basics, and distributed systems patterns such as Saga, Outbox, and idempotency**, as these are the most likely knowledge gaps based on the job description.
