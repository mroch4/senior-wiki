# YAGNI/KISS/DRY

## Table of Contents

1. [KISS — Keep It Simple, Stupid (Avoid Accidental Complexity)](#1-kiss-—-keep-it-simple-stupid-avoid-accidental-complexity)
   - [Example: Clean Architecture abuse](#example-clean-architecture-abuse)
   - [KISS and design patterns](#kiss-and-design-patterns)
2. [DRY — Don't Repeat Yourself (Avoid Duplicate Knowledge)](#2-dry-—-dont-repeat-yourself-avoid-duplicate-knowledge)
   - [Example: Duplicate business knowledge](#example-duplicate-business-knowledge)
   - [But DRY can go wrong](#but-dry-can-go-wrong)
   - [Rule of thumb](#rule-of-thumb)
3. [YAGNI — You Aren't Gonna Need It (Avoid Speculative Design)](#3-yagni-—-you-arent-gonna-need-it-avoid-speculative-design)
   - [Example: Generic repository](#example-generic-repository)
4. [The relationship between the three](#the-relationship-between-the-three)
   - [YAGNI says](#yagni-says)
   - [KISS says](#kiss-says)
   - [DRY says](#dry-says)
5. [How senior developers apply them together](#how-senior-developers-apply-them-together)
   - [Question 1 — Is this required now?](#question-1-—-is-this-required-now)
   - [Question 2 — Is there a simpler solution?](#question-2-—-is-there-a-simpler-solution)
   - [Question 3 — Is the same business knowledge duplicated?](#question-3-—-is-the-same-business-knowledge-duplicated)
   - [Example: Microservices](#example-microservices)
   - [Senior interview summary](#senior-interview-summary)

## 1. KISS — Keep It Simple, Stupid (Avoid Accidental Complexity)

The advanced idea:

> **Prefer essential complexity over accidental complexity.**

Every system has unavoidable complexity (**essential complexity**) because the business itself is complex.

Example:

A banking system must handle:

- interest calculation
- compliance rules
- transactions
- auditing

You cannot remove that complexity.

But you can avoid accidental complexity:

- unnecessary abstractions
- too many layers
- excessive patterns
- premature distributed architecture

### Example: Clean Architecture abuse

A simple CRUD API:

```
Controller
 |
Service
 |
Manager
 |
Handler
 |
Repository
 |
EF Core
 |
Database
```

A developer may say:

> "This is clean architecture."

But if every layer only forwards the call:

```csharp
public Task<User> GetUser(int id)
{
    return _repository.GetUser(id);
}
```

you created **ceremony**, not design.

A simpler design:

```
Controller
 |
EF Core DbContext
 |
Database
```

may be better.

### KISS and design patterns

Patterns are not automatically good.

A junior developer thinks:

> "I know Strategy Pattern, I should use Strategy Pattern."

A senior developer thinks:

> "Do I have a problem that Strategy Pattern solves?"

Example:

Today:

```csharp
decimal CalculateShipping(Order order)
{
    return 20;
}
```

Tomorrow:

```csharp
decimal CalculateShipping(Order order)
{
    if(order.Country == "PL")
        return 20;

    if(order.Country == "DE")
        return 30;
}
```

Only when the variation becomes real:

```csharp
IShippingCalculator
{
    Calculate();
}
```

is justified.

## 2. DRY — Don't Repeat Yourself (Avoid Duplicate Knowledge)

The advanced idea:

> DRY is not about duplicate code. It is about duplicate decisions.

Two pieces of code can look different but violate DRY if they represent the same business rule.

### Example: Duplicate business knowledge

You have:

```csharp
public decimal CalculateEmployeeBonus(Employee e)
{
    return e.Salary * 0.10m;
}
```

and:

```csharp
public decimal CalculateManagerBonus(Employee e)
{
    return e.Salary * 0.10m;
}
```

The duplication is not the formula.

The duplication is the business rule:

> "Bonus is 10% of salary."

If the company changes bonus rules:

```
10% → 15%
```

you must find every location.

### But DRY can go wrong

A common senior mistake:

> "I see duplication. I must eliminate it."

Example:

```csharp
CreateCustomer()
CreateOrder()
CreateInvoice()
```

All have:

```csharp
ValidateName()
ValidateAddress()
```

Someone creates:

```csharp
UniversalValidator<T>
```

Now:

```csharp
validator.Validate(customer);
validator.Validate(order);
validator.Validate(invoice);
```

The abstraction becomes unclear.

Why?

Because these things may evolve differently.

Today they share code.

Tomorrow:

```
Customer validation:
- Name required
- Email required

Order validation:
- Amount > 0
- Currency required
```

The abstraction starts fighting the domain.

### Rule of thumb

Duplicate code is cheaper than a wrong abstraction.

A famous phrase:

> "The first rule of abstractions: they should be discovered, not invented."

## 3. YAGNI — You Aren't Gonna Need It (Avoid Speculative Design)

The advanced idea:

> Every line of code has a maintenance cost.

Developers often optimize for imaginary future requirements.

### Example: Generic repository

Someone writes:

```csharp
public interface IRepository<TEntity>
{
    Task<TEntity> GetAsync(Guid id);
    Task AddAsync(TEntity entity);
    Task DeleteAsync(TEntity entity);
}
```

Why?

> "Maybe we will change databases."

But:

- EF Core already abstracts database access.
- There is no second database.
- No requirement exists.

You introduced:

```
Controller
 |
Repository
 |
DbContext
 |
Database
```

Instead of:

```
Controller
 |
DbContext
 |
Database
```

Extra code with no current value.

## The relationship between the three

They work together.

Imagine a requirement:

> "Customers can receive notifications."

A developer creates:

```
NotificationService
       |
       +-- EmailProvider
       +-- SmsProvider
       +-- PushProvider
       +-- TeamsProvider
       +-- SlackProvider
       +-- WebhookProvider
```

### YAGNI says

"Only implement what we need."

Start:

```
EmailNotificationService
```

### KISS says

"Don't create unnecessary layers."

Maybe:

```
OrderService
 |
EmailSender
```

is enough.

### DRY says

"Keep notification rules in one place."

Don't do:

```csharp
if(customer.IsPremium)
{
   send email;
}
```

in five different services.

Create:

```csharp
NotificationPolicy
```

# How senior developers apply them together

A good decision process:

## Question 1 — Is this required now?

(YAGNI)

No → don't build it.

## Question 2 — Is there a simpler solution?

(KISS)

Yes → use it.

## Question 3 — Is the same business knowledge duplicated?

(DRY)

Yes → centralize it.

## Example: Microservices

Requirement:

> "Process orders."

Bad approach:

```
Order Service
Payment Service
Inventory Service
Notification Service
Shipping Service
API Gateway
Message Broker
Service Discovery
Kubernetes
```

because:

> "We might scale."

YAGNI violation.

Start:

```
ASP.NET Core API
 |
Application Layer
 |
EF Core
 |
Database
```

Later, when real problems appear:

- independent scaling
- deployment bottlenecks
- team ownership boundaries

then extract services.

## Senior interview summary

> **KISS, DRY, and YAGNI are complementary design principles. KISS prevents unnecessary complexity, DRY prevents duplicated business knowledge, and YAGNI prevents building functionality without a current need. Applying them requires judgment because over-applying any principle can create problems: excessive simplicity can become rigidity, excessive DRY can create bad abstractions, and excessive YAGNI can prevent preparing for obvious requirements.**

A useful senior-level phrase:

> **"Design for today's requirements, but structure your code so tomorrow's requirements can be added without rewriting everything."**

This is the balance between **YAGNI** and **good architecture**.
