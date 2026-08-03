# Onion Architecture (Clean, Testable Enterprise Architecture)

## Table of Content

1. [What is Onion Architecture?](#what-is-onion-architecture)
   - [Advantages](#advantages)
   - [Disadvantages](#disadvantages)
2. [Layers](#layers)
3. [Domain Layer](#domain-layer)
   - [Entities](#entities)
   - [Value Objects](#value-objects)
   - [Domain Services](#domain-services)
   - [Repository Interfaces](#repository-interfaces)
4. [Application Layer](#application-layer)
5. [Infrastructure Layer](#infrastructure-layer)
6. [Presentation Layer](#presentation-layer)
7. [Dependency Rule](#dependency-rule)
8. [Why Repository Interface Lives in Domain](#why-repository-interface-lives-in-domain)
9. [Dependency Injection](#dependency-injection)
10. [Example Flow](#example-flow)
11. [Where EF Core Lives](#where-ef-core-lives)
12. [Why DbContext Isn't in Domain](#why-dbcontext-isnt-in-domain)
13. [Domain Events](#domain-events)
14. [Where the Outbox Pattern Fits?](#where-the-outbox-pattern-fits)
15. [Onion vs Clean Architecture](#onion-vs-clean-architecture)
16. [Typical Project Structure](#typical-project-structure)
17. [Onion + CQRS + MediatR + EF Core (Common .NET Setup)](#onion--cqrs--mediatr--ef-core-common-net-setup)
18. [Interview Tips](#interview-tips)

## What is Onion Architecture?

> Onion Architecture is an architectural pattern that emphasizes **separation of concerns** and **dependency inversion**. The core business logic sits at the center, while infrastructure concerns (database, APIs, UI) are pushed to the outer layers.

The key rule is:

> **Dependencies always point inward.**

The domain knows nothing about databases, web frameworks, or external services.

### Advantages

- Very testable
- Business logic independent of frameworks
- Easy to swap databases
- Easy to mock dependencies
- Promotes SOLID principles
- Clear separation of concerns
- Works well with CQRS and DDD
- Easy to evolve infrastructure without touching business logic

### Disadvantages

- More projects and abstractions
- Can be overkill for small CRUD applications
- More dependency injection and interface definitions
- Requires discipline to avoid leaking infrastructure concerns inward

## Layers

```
+--------------------------------------+
| Presentation                         |
| ASP.NET Core API / Blazor / MVC      |
+--------------------------------------+
| Application                          |
| Use cases, services, DTOs            |
+--------------------------------------+
| Domain                               |
| Entities, Value Objects              |
| Domain Services                      |
| Interfaces (Repositories)            |
+--------------------------------------+
| Infrastructure                       |
| EF Core, SQL, Redis, RabbitMQ, etc.  |
+--------------------------------------+
```

Or viewed as an onion:

```
+----------------+
| Presentation   |
+----------------+
| Infrastructure |
+----------------+
| Application    |
+----------------+
| Domain         |
+----------------+
```

The **Domain** is the center.

## 1. Domain Layer

Contains pure business logic.

No: `EF Core`, `ASP.NET.`, `HTTP`, `SQL`, `Azure`.

Just business rules.

Example:

```csharp
public class Order
{
    public Guid Id { get; }

    private readonly List<OrderItem> _items = new();

    public IReadOnlyCollection<OrderItem> Items => _items;

    public void AddItem(Product product, int quantity)
    {
        if (quantity <= 0) throw new ArgumentException();

        _items.Add(new OrderItem(product, quantity));
    }

    public decimal Total => _items.Sum(i => i.TotalPrice);
}
```

Notice: no `DbContext`, no `repository`, no `logging`.

### Domain also contains

#### Entities

```
Customer
Order
Invoice
Product
```

#### Value Objects

```
Money
Email
Address
PhoneNumber
```

Immutable.

Equality based on value.

#### Domain Services

When logic doesn't belong to one entity.

Example:

```text
CurrencyConversionService

TaxCalculationService

ShippingCalculator
```

#### Repository Interfaces

Only interfaces. No EF implementation.

```csharp
public interface IOrderRepository
{
    Task<Order?> Get(Guid id);

    Task Save(Order order);
}
```

## 2. Application Layer

Application layer orchestrates - coordinates use cases.

Example:

```
Place Order
Cancel Order
Create Invoice
Register Customer
```

```csharp
public class PlaceOrderHandler
{
    private readonly IOrderRepository _orders;

    public async Task Handle(CreateOrder command)
    {
        var order = new Order();

        order.AddItem(...);

        await _orders.Save(order);
    }
}
```

Notice:

Application knows repository interface.

Not implementation.

Application layer contains:

- CQRS Commands
- Queries
- DTOs
- Validators
- Use Cases
- MediatR handlers (if using MediatR)

## 3. Infrastructure Layer

Implements interfaces.

Example:

```csharp
public class OrderRepository : IOrderRepository
{
    private readonly AppDbContext _db;

    public async Task Save(Order order)
    {
        _db.Orders.Add(order);

        await _db.SaveChangesAsync();
    }
}
```

Infrastructure includes:

- EF Core
- SQL Server
- Azure Blob Storage
- RabbitMQ
- Kafka
- Redis
- Email
- File system
- External REST APIs

## 4. Presentation Layer

Usually:

- ASP.NET Core API
- MVC
- Blazor
- Minimal API

Example:

```csharp
app.MapPost("/orders", async (CreateOrderCommand command, IMediator mediator) =>
{
    await mediator.Send(command);

    return Results.Ok();
});
```

Presentation doesn't know EF Core either - it simply forwards requests.

## Dependency Rule

```
Presentation
 ↓
Application
 ↓
Domain
```

or:

```
Infrastructure
 ↓
Application
 ↓
Domain
```

Infrastructure depends on Domain.

Domain **NEVER** depends on Infrastructure.

❌ Bad:

```
Order
↓
DbContext
```

✅ Good:

```
Order
↓
nothing
```

## Why Repository Interface Lives in Domain

Suppose `Order` needs saving. Who should define _what_ "saving" means? Business. Not EF Core.

Therefore:

```
Domain

interface IRepository
↓
Infrastructure

EFRepository
```

## Dependency Injection

```csharp
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
```

Application asks for:

```csharp
IOrderRepository
```

Infrastructure provides:

```csharp
OrderRepository
```

## Example Flow

User creates an order:

```
HTTP Request
 ↓
Controller
 ↓
Application
 ↓
Domain
 ↓
Repository Interface
 ↓
Infrastructure
 ↓
SQL Server
```

Response returns back:

```
SQL
 ↓
Infrastructure
 ↓
Application
 ↓
Controller
 ↓
HTTP Response
```

## Where EF Core Lives

**Only Infrastructure.**

```
Infrastructure
AppDbContext
Configurations
Migrations
Repositories
```

Never in Domain.

## Why DbContext Isn't in Domain

Because then Domain would depend on EF Core. That means:

```
Business
 ↓
Database
```

Which violates Onion Architecture.

## Domain Events

The **Domain** can raise events:

```csharp
public class Order
{
    public void Place()
    {
        AddDomainEvent(new OrderPlaced(Id));
    }
}
```

Application layer dispatches them.

Handlers may:

- send email
- publish RabbitMQ message
- update read model
- create audit log

The entity never directly sends emails or messages.

## Where the Outbox Pattern Fits?

```
Application
 ↓
Save Aggregate
 ↓
Save Outbox Message
 ↓
Commit Transaction
 ↓
Background Service
 ↓
RabbitMQ
```

The Outbox implementation lives in **Infrastructure**, while the Application coordinates the use case. This ensures database changes and integration events are committed atomically.

## Onion vs Clean Architecture

| Onion                         | Clean                                   |
| ----------------------------- | --------------------------------------- |
| Domain at the center          | Enterprise business rules at the center |
| Strict inward dependencies    | Same principle                          |
| Application surrounds Domain  | Use Cases surround Entities             |
| Infrastructure on the outside | Infrastructure on the outside           |
| Presentation outermost        | Presentation outermost                  |

In practice, **Onion Architecture and Clean Architecture are very similar**. Many .NET teams use the terms interchangeably because both enforce dependency inversion and isolate business logic from infrastructure.

## Typical Project Structure

```
Solution
├── Api
├── Application
├── Domain
└── Infrastructure
```

Example:

```
MyShop.Api
MyShop.Application
MyShop.Domain
MyShop.Infrastructure
```

## Onion + CQRS + MediatR + EF Core (Common .NET Setup)

```
API
├── Minimal API / Controllers
▼
Application
├── Commands
├── Queries
├── Handlers (MediatR)
├── DTOs
▼
Domain
├── Entities
├── Value Objects
├── Domain Events
├── Repository Interfaces
▲
Infrastructure
├── EF Core DbContext
├── Repository Implementations
├── Migrations
├── Outbox
├── RabbitMQ
├── Redis
└── External APIs
```

This combination is widely used for enterprise .NET applications because it keeps business rules independent while supporting scalable patterns like CQRS, event-driven architecture, and cloud integrations.

# Interview Tips

- Be ready to explain the **dependency rule** in one sentence: _"All dependencies point toward the Domain; the Domain depends on nothing."_
- If asked why repositories are interfaces, explain that the **Application/Domain depend on abstractions**, while Infrastructure provides the concrete EF Core implementation.
- A common interview question is: **"Can I inject `DbContext` into my domain service?"** The answer is **no**. `DbContext` belongs to Infrastructure. If business logic needs persistence, depend on a repository interface instead.
- Distinguish **Application Services** (orchestrate use cases) from **Domain Services** (contain business rules that don't naturally belong to a single entity).
- Mention that Onion Architecture is often combined with **CQRS, MediatR, EF Core, the Outbox pattern, and Background Services** in modern .NET microservices.
