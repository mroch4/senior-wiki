# CQRS

**CQRS (Command Query Responsibility Segregation)** is an architectural pattern that separates **operations that change data** from **operations that read data**.

- **Commands** = write/change state
- **Queries** = read state

> The core idea is that **reading and writing have different responsibilities and often different requirements**, so keeping them separate can make the application easier to maintain and scale.

## Without CQRS

A typical service does both reads and writes.

```text
OrderService

├── GetOrder()
├── GetOrders()
├── CreateOrder()
├── UpdateOrder()
└── DeleteOrder()
```

The same model is used for everything.

## With CQRS

Commands and queries are split into separate objects and handlers.

```text
Commands:

CreateOrderCommand
UpdateOrderCommand
DeleteOrderCommand

Queries:

GetOrderQuery
GetOrdersQuery
SearchOrdersQuery
```

Each has its own handler.

```text
Controller
 |
IMediator
 |
Command Handler
Query Handler
```

## Commands

A command **changes the state** of the system. Notice: modifies data, returns the new ID, no UI logic. Examples:

- Create customer
- Update address
- Cancel order
- Approve invoice

```csharp
public record CreateOrderCommand(string Customer, decimal Total) : IRequest<int>;
```

Handler:

```csharp
public class CreateOrderHandler : IRequestHandler<CreateOrderCommand, int>
{
    private readonly AppDbContext _db;

    public async Task<int> Handle(CreateOrderCommand request, CancellationToken ct)
    {
        var order = new Order
        {
            Customer = request.Customer,
            Total = request.Total
        };

        _db.Orders.Add(order);

        await _db.SaveChangesAsync(ct);

        return order.Id;
    }
}
```

## Queries

Queries **never change state**. Notice: only reads, returns a DTO, doesn't call `SaveChanges()`.

```csharp
public record GetOrderQuery(int Id) : IRequest<OrderDto>;
```

Handler:

```csharp
public class GetOrderHandler : IRequestHandler<GetOrderQuery, OrderDto>
{
    private readonly AppDbContext _db;

    public async Task<OrderDto> Handle(GetOrderQuery request, CancellationToken ct)
    {
        return await _db.Orders
            .Where(x => x.Id == request.Id)
            .Select(x => new OrderDto
            {
                Id = x.Id,
                Customer = x.Customer,
                Total = x.Total
            })
            .FirstAsync(ct);
    }
}
```

## Why separate them?

Those concerns are different.

### Reading needs

- fast
- caching
- pagination
- projections
- denormalized data

### Writing needs

- validation
- transactions
- business rules
- consistency

---

## Read models vs Write models

A key CQRS concept is using **different** models for reading and writing.

Write model:

```text
Order.cs

Id
CustomerId
Status
OrderLines
Payments
```

Read model (optimized for display and doesn't need the full domain object):

```text
OrderSummaryDto.cs

Id
CustomerName
Status
Total
```

## Benefits

### 1. Single Responsibility

Each handler does one thing.

```text
GetOrderHandler

only reads
```

```text
UpdateOrderHandler

only writes
```

---

### 2. Easier testing

You test one use case at a time.

```text
Test CreateOrderHandler

↓

Verify order exists
```

No unrelated methods to mock.

---

### 3. Better performance

Read handlers can:

- use raw SQL or Dapper
- use caching
- query replicas
- project directly to DTOs

Write handlers focus on enforcing business rules.

---

### 4. Better organization

Instead of one large `OrderService` with many methods, the codebase is organized by feature or use case.

```text
Orders

Commands
    CreateOrder
    CancelOrder
    UpdateOrder

Queries
    GetOrder
    SearchOrders
```

This scales well as applications grow.

---

## CQRS and MediatR

These patterns are often used together.

Controller:

```csharp
await _mediator.Send(new CreateOrderCommand(...));
```

Mediator routes to `CreateOrderHandler.cs`

```csharp
var order = await _mediator.Send(new GetOrderQuery(id));
```

Mediator routes to `GetOrderHandler.cs`

## Does CQRS require two databases?

No. There are two common approaches:

### Simple CQRS (most common)

One database. Only the code is separated.

### Advanced CQRS

Separate read and write stores.
The read database is updated asynchronously from the write side, often using events.

## CQRS in microservices

A common pattern is:

```text
HTTP Request
 |
Controller
 |
Mediator
 |
Command Handler
 |
SQL Database
 |
Publish Integration Event
 |
RabbitMQ / Azure Service Bus / Kafka
```

Other microservices consume the event and update their own data, supporting eventual consistency.

---

## Senior interview perspective

A common misconception is that **CQRS means "two databases."** In reality, CQRS is primarily about **separating reads from writes in your code**. Using separate databases or read models is an optimization for systems with high scale or specialized read requirements.

For most enterprise .NET applications, a practical implementation is:

- Commands and queries represented as separate request types.
- One handler per command or query (often via MediatR or another mediator library).
- A single relational database (e.g., SQL Server) with Entity Framework Core.
- DTO projections for queries and domain/business logic in command handlers.

This gives you the organizational benefits of CQRS without the complexity of distributed read/write stores.
