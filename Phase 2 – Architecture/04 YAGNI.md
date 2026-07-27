# YAGNI

## Table of Content

1. [What is YAGNI?](#what-is-yagni)
   - [Why YAGNI matters](#why-yagni-matters)
2. [Example 1 – Premature abstraction](#example-1--premature-abstraction)
   - [YAGNI violation](#yagni-violation)
   - [YAGNI](#yagni)
3. [Example 2 – Database design](#example-2--database-design)
4. [Example 3 – API](#example-3--api)
5. [Example 4 – Microservices](#example-4--microservices)
   - [YAGNI violation](#yagni-violation-1)
   - [YAGNI](#yagni-1)
6. [YAGNI vs KISS vs DRY](#yagni-vs-kiss-vs-dry)
7. [Common mistake](#common-mistake)
8. [Interview Tips](#interview-tips)
9. [Cheat-sheet summary](#cheat-sheet-summary)

## What is YAGNI?

> **YAGNI** stands for **You Aren't Gonna Need It**.

It is an Extreme Programming (XP) principle that says:

> **Don't implement functionality until there is a real, current requirement for it.**

The idea is to avoid spending time building features or abstractions based on assumptions about the future.

### Why YAGNI matters

Building unnecessary features results in:

- ❌ Wasted development time
- ❌ More code to maintain
- ❌ Increased complexity
- ❌ More bugs
- ❌ Harder testing

Many "future-proof" features are never actually used.

## Example 1 – Premature abstraction

Suppose your application sends emails only.

### YAGNI violation

```csharp
public interface IMessageSender
{
    Task SendAsync(string message);
}

public class EmailSender : IMessageSender
{
    public Task SendAsync(string message)
    {
        // Send email
    }
}

public class SmsSender : IMessageSender
{
    public Task SendAsync(string message)
    {
        // Not used
    }
}

public class PushNotificationSender : IMessageSender
{
    public Task SendAsync(string message)
    {
        // Not used
    }
}
```

Someone says:

> "We might need SMS later."

But there is no requirement for SMS.

### YAGNI

```csharp
public class EmailSender
{
    public Task SendAsync(string message)
    {
        // Send email
    }
}
```

If SMS becomes a real requirement later, then introduce the interface and additional implementations.

## Example 2 – Database design

You have `Customer`

Someone creates:

```
Customer
CustomerType
CustomerCategory
CustomerSegment
CustomerGroup
```

because:

> "Marketing might need them one day."

If they're unused, they're unnecessary complexity.

## Example 3 – API

Your application has:

```
GET /products
POST /products
```

Someone also creates:

```
PUT /products
PATCH /products
DELETE /products
```

even though updates and deletes are not part of the current requirements.

That's extra code to write, test, document, and secure.

## Example 4 – Microservices

Current application:

- 3 developers
- 2,000 users
- One database
- Simple CRUD

### YAGNI violation

Starting with:

- Kubernetes
- Kafka
- Event Sourcing
- CQRS
- Saga
- Service Mesh

because:

> "We'll need them when we scale."

Most startups never reach that scale.

### YAGNI

Start with:

- ASP.NET Core
- SQL Server
- EF Core

Introduce more advanced architecture only when concrete problems justify it.

## YAGNI vs KISS vs DRY

| Principle | Focus                       | Question to ask                             |
| --------- | --------------------------- | ------------------------------------------- |
| **YAGNI** | Don't build unused features | "Do we need this now?"                      |
| **KISS**  | Keep solutions simple       | "Is there a simpler way?"                   |
| **DRY**   | Avoid duplicate knowledge   | "Can this business rule live in one place?" |

## Common mistake

Developers often think they're "future-proofing" the application.

For example:

```csharp
public interface IRepository<T>
```

created before there is any need for multiple repository implementations.

Unless you have a real use case (e.g., multiple data sources or testability concerns that aren't already addressed by EF Core), this abstraction may just add complexity.

# Interview Tips

> "YAGNI stands for _You Aren't Gonna Need It_. It means you should implement functionality only when there is a real requirement, rather than anticipating future needs. This helps keep the codebase smaller, easier to maintain, and less error-prone. In .NET, a common example is avoiding unnecessary abstractions, microservices, or generic frameworks until they solve an actual problem."

## Cheat-sheet summary

| Principle | Meaning                  | Goal                                        |
| --------- | ------------------------ | ------------------------------------------- |
| **KISS**  | Keep It Simple           | Avoid unnecessary complexity.               |
| **DRY**   | Don't Repeat Yourself    | Keep each business rule in one place.       |
| **YAGNI** | You Aren't Gonna Need It | Don't build features before they're needed. |

A useful way to remember them is:

- **KISS:** _Keep the implementation as simple as possible._
- **DRY:** _Don't duplicate business logic or knowledge._
- **YAGNI:** _Don't solve tomorrow's problems today._
