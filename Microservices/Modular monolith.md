# Modular monolith

Designing a **new monolithic application that can later evolve into microservices** is mostly about creating the right **boundaries and dependencies** from day one. The goal is not to build "microservices in disguise" — that usually creates unnecessary complexity. The goal is to build a **modular monolith**.

A good evolution path is:

```
Monolithic App
 |
Modular Monolith
 |
Extract selected modules
 |
Microservices
```

## 1. Start with a Modular Monolith (most important)

Avoid:

```
Controllers
 |
Services
 |
Repositories
 |
Database
```

where everything can call everything.

Instead:

```
Application
|
+-- Orders Module
|     +-- API
|     +-- Application
|     +-- Domain
|     +-- Infrastructure
|
+-- Payments Module
|     +-- API
|     +-- Application
|     +-- Domain
|     +-- Infrastructure
|
+-- Identity Module
      +-- API
      +-- Application
      +-- Domain
```

Each module behaves like a future service.

Example:

```
/src

  /Modules

      /Orders
          Orders.Api
          Orders.Application
          Orders.Domain
          Orders.Infrastructure

      /Payments
          Payments.Api
          Payments.Application
          Payments.Domain
          Payments.Infrastructure
```

Later:

```
Orders module
 |
 +----> Orders Microservice

Payments module
 |
 +----> Payments Microservice
```

## 2. Define business boundaries, not technical boundaries

❌ Bad:

```
UserService
DatabaseService
EmailService
LoggingService
```

These are technical.

✅ Better:

```
Customer Management
Order Processing
Billing
Inventory
Shipping
```

These represent business capabilities.

This is from Domain Driven Design:

**Bounded Contexts**

Example:

```
E-commerce System:


Customer Context
    Customer
    Address
    Preferences


Order Context
    Order
    OrderLine
    Shipment


Payment Context
    Invoice
    Transaction
```

Each context could become a microservice.

## 3. Avoid shared databases between future services

A common mistake:

```
Order Service
 |
+---------+
| Same DB |
+---------+
 |
Payment Service
```

Later extraction becomes painful.

Instead:

```
Order Module

Orders
OrderItems
```

and

```
Payment Module

Payments
Transactions
```

Even inside one database, keep logical ownership:

```
Database

dbo.Orders
dbo.OrderItems
```

and

```
Payment schema

payment.Payments
payment.Transactions
```

The rule:

> A module owns its data.

## 4. Do not share domain models

Very common mistake:

```
SharedProject

Customer.cs
Order.cs
Payment.cs
```

Then:

```
Orders
 |
 +-- references Shared

Payments
 |
 +-- references Shared
```

Now everything is coupled.

Later:

```
Extract Payment Service

Problem:
Payment needs Shared.Customer
```

Instead:

```
Orders.Domain

Order
CustomerId
```

```
Payments.Domain

Payment
CustomerReference
```

Share IDs, not objects.

✅ Good:

```csharp
public class Order
{
    public Guid CustomerId {get;set;}
}
```

❌ Bad:

```csharp
public class Order
{
    public Customer Customer {get;set;}
}
```

## 5. Use internal APIs between modules

Do not allow:

```
Orders
 |
 | directly calls
 |
Payments database
```

Instead:

```
Orders
 |
 | Payment interface
 |
Payments Module
```

Example:

```csharp
public interface IPaymentService
{
    Task<bool> ChargeAsync(Guid orderId, decimal amount);
}
```

Implementation:

Current:

```
IPaymentService
 |
Payments Module
```

Future:

```
IPaymentService
 |
HTTP/gRPC call
 |
Payment Microservice

```

The caller does not change.

## 6. Introduce events early

Microservices communicate mostly through events.

Start your monolith with domain events:

Example:

Order created:

```csharp
public record OrderCreatedEvent( Guid OrderId, DateTime CreatedAt);
```

Flow:

```
Order Module

Create Order

     |
     v

OrderCreated Event

     |
     +------------+
                  |
                  v

Inventory Module

                  |
                  v

Notification Module
```

Today:

```
in-process event bus
```

Tomorrow:

```
RabbitMQ
Azure Service Bus
Kafka
```

The architecture survives.

# 7. Use CQRS selectively

Not everything needs CQRS.

But design commands separately:

Instead of:

```csharp
OrderService.CreateOrder()
```

think:

```
Command

CreateOrderCommand


Handler

CreateOrderHandler


Domain

Order.Create()
```

Example:

```csharp
public record CreateOrderCommand(
    Guid CustomerId,
    List<ItemDto> Items);
```

Handler:

```csharp
public class CreateOrderHandler
{
    public async Task Handle(
        CreateOrderCommand command)
    {
        var order =
            Order.Create(command.CustomerId);

        await repository.Save(order);
    }
}
```

Later the handler can move to a service.

# 8. Keep dependencies pointing inward

Use Clean Architecture:

```
        API

         |
         v

 Application

         |
         v

 Domain


Infrastructure
```

Domain knows nothing about:

- SQL
- HTTP
- Entity Framework
- RabbitMQ

Example:

Good:

```
Domain
   |
   |
Application
   |
   |
Infrastructure
```

Bad:

```
Domain
 |
 |
Entity Framework
 |
 |
SQL
```

# 9. Create anti-corruption layers

When modules interact:

Example:

```
Order
 |
 |
Customer Module
```

Do not expose your internal model:

Bad:

```csharp
CustomerDto GetCustomer()
```

Better:

```csharp
CustomerSummary GetCustomerForOrder()
```

The Order module receives only what it needs.

# 10. Avoid distributed transactions from day one

Do not design:

```
Create Order

BEGIN TRANSACTION

Create order
Charge card
Reserve stock

COMMIT
```

Microservices cannot do this.

Instead:

Use eventual consistency:

```
Order Created

        |
        v

Payment Requested

        |
        v

Payment Completed

        |
        v

Order Confirmed
```

Patterns:

- Outbox Pattern
- Saga Pattern
- Event-driven workflows

# 11. Use dependency injection boundaries

Example:

```
Orders Module


Program.cs

services.AddOrders();


Payments Module


Program.cs

services.AddPayments();
```

Each module registers itself.

Later:

```
Orders Service

Program.cs

services.AddOrders();
```

almost unchanged.

# 12. Have separate configuration

Avoid:

```
appsettings.json

ConnectionString
PaymentSettings
EmailSettings
InventorySettings
```

Better:

```
Orders

appsettings.orders.json


Payments

appsettings.payments.json
```

Later:

```
Orders API

environment variables


Payments API

environment variables
```

# 13. Avoid synchronous chains

Danger:

```
API

 |
Order

 |
Payment

 |
Inventory

 |
Shipping
```

One request does everything.

Later:

```
API
 |
Order Service

Event

Payment Service

Event

Inventory Service
```

Design long-running processes asynchronously.

# Recommended .NET solution structure

A very scalable structure:

```
src

Company.Application

Modules

 ├── Orders
 │     ├── Domain
 │     ├── Application
 │     ├── Infrastructure
 │     └── Api
 │
 ├── Payments
 │     ├── Domain
 │     ├── Application
 │     ├── Infrastructure
 │     └── Api


BuildingBlocks

 ├── DomainEvents
 ├── Messaging
 ├── Logging
 └── Observability
```

## The golden rules

If you remember only these:

1. **Build a modular monolith first**
2. **Each module owns its data**
3. **No shared domain models**
4. **Communicate through interfaces/events**
5. **Business boundaries become future services**
6. **Avoid distributed transactions**
7. **Design for replacing in-process calls with network calls**
8. **Prefer autonomy over code reuse**

A well-designed modular monolith can often be split into microservices with **months less effort** compared with a traditional tightly coupled monolith.
