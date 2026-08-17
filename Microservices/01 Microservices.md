# Microservices

## Table of content

1. [What are Microservices?](#what-are-microservices)
2. [Core Characteristics](#core-characteristics)
3. [Example Structure](#example-structure)
4. [Monolith vs Microservices](#monolith-vs-microservices)
   - [Monolith](#monolith)
   - [Microservices](#microservices)
5. [When should you use Microservices?](#when-should-you-use-microservices)
6. [Core Principles](#core-principles)
   - [Single Responsibility](#single-responsibility)
   - [Database per Service](#database-per-service)
7. [Communication](#communication)
   - [Synchronous](#synchronous)
     - [REST APIs](#rest-apis)
     - [gRPC](#grpc)
   - [Asynchronous Messaging](#asynchronous-messaging)
8. [Common Design Patterns](#common-design-patterns)
   - [API Gateway](#api-gateway)
   - [Service Discovery](#service-discovery)
   - [Event-Driven Architecture](#event-driven-architecture)
   - [Distributed Transactions](#distributed-transactions)
   - [Saga Pattern](#saga-pattern)
9. [Eventual Consistency](#eventual-consistency)
10. [Resilience Patterns](#resilience-patterns)
11. [Observability](#observability)
    - [Logging](#logging)
    - [Metrics](#metrics)
    - [Distributed Tracing](#distributed-tracing)
12. [Containers](#containers)
13. [Kubernetes](#kubernetes)
14. [Technologies Used](#technologies-used)
15. [Common .NET Stack](#common-net-stack)
16. [Interview Tips](#interview-tips)
17. [Interview Questions](#interview-questions)

## What are Microservices?

A **microservice** is a small, independently deployable application responsible for a **single** business capability. Each service owns its own code, business logic, and usually its own database. Services communicate over APIs or messaging rather than direct database access.

A distributed system is a collection of independent services running on different machines that communicate over a network to achieve a common goal.

```
Client
 |
API Gateway (YARP/Ocelot)
 ├── Product API -> Product DB
 ├── Order API -> Order DB
 ├── Payment API -> Payment DB
```

Each service can:

- Be deployed independently
- Scale independently
- Be developed by a separate team
- Have its own database

### Core Characteristics

- **Independent deployment** – Each service can be deployed without affecting others.
- **Single responsibility** – One service handles one business function.
- **Own database** – Each microservice manages its own data.
- **Loose coupling** – Services communicate via HTTP/gRPC or message brokers.
- **Scalability** – Individual services can be scaled independently.

### Example Structure

```
ECommerce
│
├── ProductService
│   ├── Controllers
│   ├── Services
│   ├── Models
│   └── ProductDb
│
├── OrderService
│   ├── Controllers
│   ├── Services
│   ├── Models
│   └── OrderDb
│
├── PaymentService
│
├── API Gateway
│
└── Docker Compose
```

## Monolith vs Microservices

### Monolith

```
+------------------+
| Web API          |
| Controllers      |
| Business Logic   |
| Data Access      |
| One SQL Database |
+------------------+
```

✅ Advantages:

- Simple
- Easy debugging
- Easy transactions
- Fast local development

❌ Disadvantages:

- Entire application must be redeployed
- Difficult to scale one feature
- Large codebase becomes difficult to maintain

### Microservices

```
Product Service -> Product DB
```

```
Order Service -> Order DB
```

```
Payment Service -> Payment DB
```

✅ Advantages/benefits:

- Independent deployment
- Independent scaling
- Better fault isolation
- Easier maintenance
- Faster development by multiple teams working independently
- Technology flexibility

❌ Disadvantages/challenges:

- Increased operational complexity
- Distributed transactions
- Network latency (average HTTP call 20-300 ms as compared to 1 microsecond for a method call)
- Network failures (packets are lost, connections fail, DNS fails, TLS expires, firewalls block traffic)
- Service discovery
- Eventual consistency
- Data consistency across services
- Monitoring and logging becomes harder
- More DevOps work

## When should you use Microservices?

✅ Good candidates:

- Large systems
- Complex business domains
- Different scaling requirements
- Multiple development teams
- Frequent deployments

❌ Bad candidates:

- Small CRUD applications
- Startup MVP
- Small development team
- Simple internal tools

Interview answer:

> "Don't start with microservices. Start with a modular monolith and extract services when business or scaling requirements justify it."

## Core Principles

### Single Responsibility

Each service should own one business capability.

✅ Good:

- Product Service
- Order Service
- Payment Service

❌ Bad:

```
CustomerManagementService
```

that handles:

- orders
- invoices
- products
- shipping
- authentication

### Database per Service

One of the biggest interview questions.

✅ Good:

```
Order Service
 |
SQL Database
```

```
Payment Service
 |
PostgreSQL
```

❌ Bad:

```
Order Service
 |
Shared Database
 |
Payment Service
```

If services share the same database, they become tightly coupled.

## Communication

### Synchronous

When to use:

- immediate response needed
- request/response
- validation
- querying data

```
Order Service
 |
GET HTTP/gRPC
 |
Product Service
```

#### REST APIs

✅ Pros:

- Simple and easy
- Immediate response

❌ Cons:

- Tight runtime dependency
- Cascading failures

#### gRPC

High-performance communication (faster than REST):

- ideal for internal service communication
- binary protocol
- high performance

Uses:

- HTTP/2
- Protobuf
- Binary serialization

### Asynchronous Messaging

When to use:

- work takes time
- failures should be retried
- loose coupling desired
- event-driven architecture

Message Broker:

- RabbitMQ
- Kafka
- Azure Service Bus

```
Order Created
 |
RabbitMQ
 ├── Inventory Service
 ├── Shipping Service
 ├── Email Service
```

✅ Benefits:

- Loose coupling
- Better resilience
- Retry support

## Common Design Patterns

- [API Gateway](#api-gateway)
- ← [CQRS (Command Query Responsibility Segregation)](/Phase%202%20–%20Architecture/09%20CQRS.md)
- [Event-Driven Architecture](#event-driven-architecture)
- [Saga Pattern (distributed transactions)](#saga-pattern)
- ← Circuit Breaker (using Polly)
- ← [Repository Pattern](/Phase%202%20–%20Architecture/12%20Repository%20Pattern.md)
- ← [Outbox Pattern]()
- Database per Service

### API Gateway

Clients should not call every service directly.

```
Client
 |
API Gateway (YARP/Ocelot)
 ├── Products
 ├── Orders
 ├── Users
```

Common responsibilities:

- Authentication
- Authorization
- Rate limiting
- Routing
- Aggregation
- Logging

In .NET:

- YARP
- Ocelot

#### Load balancing

Requests are distributed among healthy instances.

Common algorithms

- Round Robin
- Least Connections
- Weighted Round Robin
- Least Response Time
- Consistent Hashing (useful when affinity or partitioning matters)

### Service Discovery

How does Service A know where Service B lives?

Instead of hardcoding:

```
http://10.0.0.23
```

Services register themselves.

Solutions:

- Kubernetes DNS
- Consul
- Eureka
- Azure Service Discovery

### Event-Driven Architecture

This is one of the most common interview misconceptions:

> **Event-Driven Architecture (EDA) and Microservices are not competing architectures.**
>
> They solve different problems and are often used **together**.

Think of it like this:

- **Microservices** answer **"How do we split the application?"**
- **Event-Driven Architecture** answers **"How do those parts communicate?"**

Instead of:

```
Order Service
 |
Email Service
```

Publish an event/fact:

```
OrderCreated
 |
RabbitMQ
 ├── Email Service
 ├── Analytics
 ├── Inventory
 ├── Loyalty Points
```

One service publishes events. The other subscribe.
Publisher knows nothing about subscribers.
Producer knows nothing about consumers.

Loose coupling.

### Distributed Transactions

Traditional SQL transaction:

```
Begin

Insert Order

Insert Payment

Commit
```

Impossible across multiple services.

Instead use:

### Saga Pattern

Assume published event/fact:

```
Order Created
 ├── Reserve Inventory
 ├── Charge Payment
 ├── Create Shipment
```

If any step fails, cancel `Order`, refund `Payment` and release `Inventory`. **Compensating actions undo previous work and replace transaction rollback.**

## Eventual Consistency

In a monolith application everything updates immediately. Distributed systems usually cannot provide immediate consistency. Average HTTP call takes miliseconds (20-300 ms) or seconds. Hence, **temporary inconsistency is acceptable**.

## Resilience Patterns

> What happens if one service is unavailable?

- Retry (for transient failures)
- Timeout (avoid hanging requests)
- Circuit Breaker (stop calling unhealthy services)
- Bulkhead (isolate resources)
- Fallback (return cached/default response)
- Rate Limiting (protect services)
- Health Checks (detect unhealthy instances)

In .NET, these are commonly implemented with **Polly** (or the newer resilience APIs built into recent .NET versions) together with `HttpClientFactory`.

## Observability

A production microservice system needs:

### Logging

Structured logs

Example:

```
OrderId
UserId
CorrelationId
```

### Metrics

- CPU
- Memory
- Requests/sec
- Response time

### Distributed Tracing

Request flow:

```
Gateway
 |
Orders
 |
Payments
 |
Inventory
```

Tools:

- OpenTelemetry
- Jaeger
- Zipkin
- Azure Application Insights

## Containers

Most microservices run in Docker.

Each service:

```
FROM mcr.microsoft.com/dotnet/aspnet:9.0

COPY .

ENTRYPOINT ["dotnet","Orders.dll"]
```

## Kubernetes

Responsibilities:

- Scheduling
- Scaling
- Restart failed containers
- Rolling deployments
- Load balancing
- Service discovery

## Technologies Used

| Component         | .NET Technology                           |
| ----------------- | ----------------------------------------- |
| Framework         | ASP.NET Core                              |
| Runtime           | .NET 8 / .NET 9                           |
| API               | REST, Minimal APIs, gRPC                  |
| Database          | SQL Server, PostgreSQL, MongoDB           |
| ORM               | Entity Framework Core, Dapper             |
| Messaging         | RabbitMQ, Azure Service Bus, Apache Kafka |
| Authentication    | JWT, OAuth2, OpenID Connect               |
| Service Discovery | Consul, Kubernetes DNS                    |
| API Gateway       | YARP, Ocelot                              |
| Containerization  | Docker                                    |
| Orchestration     | Kubernetes                                |
| Monitoring        | OpenTelemetry, Prometheus, Grafana        |
| Logging           | Serilog, Elasticsearch                    |

## Common .NET Stack

A typical production stack:

- ASP.NET Core Minimal APIs
- EF Core
- MediatR
- RabbitMQ / Azure Service Bus / Kafka
- Polly
- Docker
- Kubernetes
- YARP or Ocelot
- OpenTelemetry
- Prometheus
- Grafana
- Azure App Insights

## Interview Tips

If you're preparing for interviews, a practical stack to learn is:

- ASP.NET Core (.NET 8/9)
- Entity Framework Core
- Docker
- RabbitMQ
- YARP API Gateway
- JWT Authentication
- OpenTelemetry + Serilog
- Kubernetes (basics)
- Redis for caching

Building a small e-commerce application (Products, Orders, Payments, and Notifications as separate services) is an excellent way to gain hands-on experience with .NET microservices.

## Interview Questions

What a Senior .NET Developer Should Be Comfortable Explaining

- What are microservices, and how do they differ from monolithic architecture?
- Microservices vs. monolith trade-offs.

- How do microservices communicate in .NET?
- Synchronous vs. asynchronous communication.
- When would you choose gRPC over REST?

- Event-driven architecture.
- RabbitMQ, Azure Service Bus, and Kafka use cases.
- Idempotency and duplicate message handling.

- What is the Saga pattern?
- What is the Outbox pattern, and why is it useful?
- What is eventual consistency and why it is necessary?

- How do you secure .NET microservices using JWT?
- What is the role of an API Gateway?

- Resilience patterns (retry, circuit breaker, timeout, bulkhead, fallback).
- How do you implement resilience with Polly?

- How do you deploy .NET microservices using Docker and Kubernetes?
- Correlation IDs and distributed tracing.
- Health checks, service discovery, and load balancing.
- CAP theorem and practical consistency trade-offs.

- Explain that microservices solve **organizational and scaling problems**, not just technical ones.
- Stress **database per service** and **event-driven communication** as core principles.
- Differentiate clearly between **REST/gRPC (request-response)** and **messaging (asynchronous events)**.
- Mention **DDD bounded contexts** as the basis for identifying service boundaries.
- Show awareness of operational concerns: monitoring, tracing, logging, CI/CD, containers, and Kubernetes.
- If asked what architecture you'd choose for a new project, a strong answer is: start with a modular monolith and evolve to microservices when justified by business and operational needs.

**Why separate databases?**

> To avoid tight coupling and allow independent evolution of each service.

**How do services communicate?**

> REST/gRPC for synchronous calls; RabbitMQ, Azure Service Bus, or Kafka for asynchronous messaging.

**Why avoid distributed transactions?**

> They are difficult to coordinate across services and reduce availability. Sagas with compensating actions are preferred.

**What happens if a service is down?**

> Apply resilience patterns such as retries, circuit breakers, fallbacks, and timeouts.

**How do you version APIs?**

> Use URI or header versioning (e.g., `/api/v1/orders`) and support multiple versions during migrations.

**How do you deploy without downtime?**

> Use rolling updates or blue-green/canary deployments with Kubernetes and CI/CD pipelines.

**How do you ensure reliable event publishing?**

> Use the Outbox Pattern so database updates and event persistence occur in the same transaction, then publish asynchronously.

**How do you avoid duplicate message processing?**

> Design consumers to be idempotent.
> Track processed message IDs or use idempotency keys.

**Why is `HttpClientFactory` preferred over creating a new `HttpClient` for every request?**

- It reuses underlying handlers to avoid socket exhaustion.
- It manages DNS updates more effectively.
- It integrates with resilience policies, logging, and dependency injection.

**What's the difference between RabbitMQ, Azure Service Bus, and Kafka?**

- **RabbitMQ:** traditional message broker, flexible routing, ideal for work queues and request distribution.
- **Azure Service Bus:** managed enterprise messaging with queues, topics, sessions, dead-letter queues, and Azure integration.
- **Kafka:** distributed event streaming platform optimized for very high throughput, durable logs, and replaying events.

**Can you use database transactions across services?**

Usually no. Distributed transactions (Two-Phase Commit) are rarely used because they:

- reduce availability
- are slow
- are difficult to scale
- create tight coupling

Instead use:

- Saga pattern
- Outbox pattern
- retries
- compensating transactions
