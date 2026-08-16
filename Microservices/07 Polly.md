# Polly

## Table of content

1. [What is Polly?](#what-is-polly)
2. [Why Do We Need Polly?](#why-do-we-need-polly)
3. [What Is a Transient Fault?](#what-is-a-transient-fault)
4. [Polly Strategies](#polly-strategies)
   - [Retry](#1-retry)
     - [Retry Delay](#retry-delay)
     - [Exponential Backoff](#exponential-backoff)
   - [Circuit Breaker](#2-circuit-breaker)
     - [Without Circuit Breaker](#without-circuit-breaker)
     - [With Circuit Breaker](#with-circuit-breaker)
     - [States](#states)
     - [Why It Matters](#why-it-matters)
   - [Timeout](#3-timeout)
   - [Fallback](#4-fallback)
   - [Bulkhead Isolation](#5-bulkhead-isolation)
5. [Polly with HttpClient](#polly-with-httpclient)
6. [Polly in Microservices](#polly-in-microservices)
7. [When to Retry?](#when-to-retry)
8. [Polly vs Retry](#polly-vs-retry)
9. [Best Practices](#best-practices)
10. [Cheat Sheet](#cheat-sheet)
11. [Interview Tips](#interview-tips)

## What is Polly?

> Polly is a .NET resilience library that helps applications **handle transient failures** such as temporary network issues, service outages, and timeouts. It is widely used in **microservices** because remote service calls are inherently unreliable.

Think of Polly as a **safety net** around your HTTP, gRPC, or database calls.

## Why Do We Need Polly?

Suppose the **Order Service** calls the **Payment Service**:

```
Order Service
 │ HTTP Request
Payment Service
```

What if:

- The network is temporarily slow?
- The Payment Service is restarting?
- A timeout occurs?
- The service returns HTTP 503 (Service Unavailable)?

### Without resilience:

```
Order Service
 |
Payment Service
 |
❌ Failure
```

The order fails immediately.

### With Polly:

```
Order Service
 |
Retry
 |
Payment Service
 |
✅ SUCCESS
```

## What Is a Transient Fault?

A transient fault is a **temporary problem** that may succeed if you try again.

Examples:

- Temporary network interruption
- HTTP 503 (Service Unavailable)
- HTTP 502 (Bad Gateway)
- HTTP 504 (Gateway Timeout)
- Temporary database connection issue

Examples of **non-transient** failures:

- HTTP 404 (Not Found)
- Invalid credentials
- Validation errors
- Programming bugs

Retrying these usually doesn't help.

## Polly Strategies

### 1. Retry

If a request fails, Polly retries automatically.

Without retry:

```
Request
 ↓
Failure
```

With retry:

```
Request
 ↓
Failure
 ↓
Retry
 ↓
Failure
 ↓
Retry
 ↓
Success
```

#### Example

```csharp
builder.Services.AddHttpClient<IPaymentService, PaymentService>()
    .AddStandardResilienceHandler();
```

In modern .NET (8+), the recommended approach is using the built-in resilience support, which is powered by Polly.

A customized retry example:

```csharp
builder.Services.AddHttpClient("PaymentApi")
    .AddResilienceHandler("payment-pipeline", builder =>
    {
        builder.AddRetry(new()
        {
            MaxRetryAttempts = 3
        });
    });
```

#### Retry Delay

Retrying immediately isn't always a good idea.

Better approach:

```
Try 1
 ↓
Wait 1 second
 ↓
Try 2
 ↓
Wait 2 seconds
 ↓
Try 3
```

This is called **backoff**.

#### Exponential Backoff

Instead of: `1/1/1`
Use: `1/2/4/8/16`

This reduces pressure on an already struggling service.

### 2. Circuit Breaker

Imagine repeatedly calling a service that is down.

#### Without Circuit Breaker

```
Request
 ↓
Fail
 ↓
Request
 ↓
Fail
 ↓
Request
 ↓
Fail
```

This wastes resources.

#### With Circuit Breaker

After several failures:

```
Circuit
 ↓
OPEN
```

Requests fail immediately without contacting the remote service.

After a wait period: `HALF OPEN` One request is allowed through.

If it succeeds: `CLOSED` normal operation resumes.

If it fails: `OPEN` again.

#### States

```
Closed
 ↓
Failures
 ↓
Open
 ↓
Wait
 ↓
Half Open
 ↓
Success
 ↓
Closed
```

#### Why It Matters

❌ Without Circuit Breaker:

- Thousands of useless requests
- Cascading failures
- Resource exhaustion

✅ With Circuit Breaker:

- Protects your application
- Gives the failing service time to recover

### 3. Timeout

Sometimes a service doesn't fail — it just hangs.

Example:

```
Order Service
 ↓
Payment Service
 ↓
Waiting...
 ↓
Waiting...
 ↓
Waiting...
```

Timeout stops waiting after a configured period.

Example:

```
Wait 5 seconds
 ↓
Still no response
 ↓
Timeout
```

The application can then retry, return an error, or use a fallback.

### 4. Fallback

Suppose the Product Service is unavailable:

Instead of an error: `503 Service Unavailable` return cached data.

```
Request
 ↓
Failure
 ↓
Fallback
 ↓
Cached Products
```

Users still receive a response, although it may not be the latest data.

### 5. Bulkhead Isolation

Imagine a ship - if one compartment floods only one section fills with water. The ship stays afloat.
Software works similarly.

#### Without Bulkhead:

```
1000 requests
 ↓
Everything blocked
```

#### With Bulkhead:

```
Payment
Inventory
Shipping
Notifications
```

Each area has **its own resource limits**. A problem in one doesn't affect the others.

## Polly with HttpClient

The most common usage is protecting outbound HTTP calls:

```csharp
builder.Services
    .AddHttpClient("InventoryApi")
    .AddStandardResilienceHandler();
```

This enables a sensible set of resilience strategies for HTTP requests in .NET 8+.

## Polly in Microservices

Example:

```
Order Service
 ↓
Payment Service
 ↓
Inventory Service
 ↓
Notification Service
```

Every network call is a candidate for:

- Retry
- Timeout
- Circuit Breaker

These are where transient failures are most common.

## When to Retry?

Retry:

- HTTP 503
- HTTP 502
- HTTP 504
- Temporary network failures
- Connection reset

Don't retry:

- HTTP 400
- HTTP 401
- HTTP 403
- HTTP 404
- Validation failures

These errors are unlikely to succeed on another attempt.

## Polly vs Retry

> Isn't Polly just a retry library?

No. Retry is only **one** resilience strategy.

Polly also supports:

- Circuit Breaker
- Timeout
- Fallback
- Bulkhead Isolation
- Composition of multiple strategies into a resilience pipeline

## Best Practices

- Retry only transient failures.
- Use **exponential backoff** instead of immediate retries.
- Combine retries with **timeouts** so requests don't wait forever.
- Use **circuit breakers** to prevent cascading failures.
- Log retries and failures for monitoring and troubleshooting.
- Avoid infinite retries.

## Cheat Sheet

| Strategy        | Purpose                            | Example                                |
| --------------- | ---------------------------------- | -------------------------------------- |
| Retry           | Try again after transient failures | Temporary network glitch               |
| Timeout         | Stop waiting after a limit         | Service hangs for 30 seconds           |
| Circuit Breaker | Stop calling an unhealthy service  | Payment API is down                    |
| Fallback        | Provide an alternative response    | Return cached product data             |
| Bulkhead        | Isolate resource usage             | Payment failures don't block inventory |

# Interview Tips

What is Polly?

> Polly is a .NET resilience library used to make applications more fault-tolerant. It helps handle transient failures by applying strategies such as retry, timeout, circuit breaker, fallback, and bulkhead isolation. In modern .NET (8+), these capabilities are integrated into the built-in HTTP resilience pipeline and are commonly used to protect service-to-service communication in microservice architectures.

The Payment Service is temporarily unavailable. How would you make the Order Service more resilient?

- Configure a timeout so requests don't hang indefinitely.
- Retry a few times with exponential backoff for transient failures.
- Use a circuit breaker to stop sending requests if the service is consistently failing.
- Optionally use a fallback (such as queuing the order for later processing or returning cached information, depending on business requirements).
