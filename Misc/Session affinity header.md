# Session affinity header

## Table of content

1. [What is Session affinity header?](#what-is-session-affinity-header)
   - [Why is it needed?](#why-is-it-needed)
2. [How does it work?](#how-does-it-work)
   - [Cookie-based affinity (most common)](#cookie-based-affinity-most-common)
   - [Header-based affinity](#header-based-affinity)
3. [Example flow](#example-flow)
4. [Why is session affinity often avoided?](#why-is-session-affinity-often-avoided)
5. [.NET example](#net-example)
6. [Interview Tips](#interview-tips)

## What is Session affinity header?

> A **session affinity header** (also called a **sticky session header**) is an HTTP header used by a **load balancer or reverse proxy** to ensure that subsequent requests from the same client are routed to the **same backend server**.

This is important when the application stores session state **in memory** instead of in a shared store.

### Why is it needed?

Imagine you have three instances of your API:

```
     Load Balancer
    /      |      \
API1     API2     API3
```

A user logs in:

```
Request 1 -> API2
```

API2 stores the user's session in memory.

The next request arrives:

```
Request 2 -> API1
```

API1 has no knowledge of that session, so the user appears logged out.

Session affinity prevents this by routing both requests to API2.

## How does it work?

The load balancer usually adds or relies on one of the following:

### Cookie-based affinity (most common)

After the first request:

```
Set-Cookie: ARRAffinity=abc123
```

Every subsequent request includes:

```
Cookie: ARRAffinity=abc123
```

The load balancer reads the cookie and routes the request back to the same server.

Examples:

- Azure App Service → `ARRAffinity`
- AWS Application Load Balancer → `AWSALB`
- NGINX sticky cookie module
- HAProxy stick tables

### Header-based affinity

Instead of cookies, some infrastructures use a custom header:

```
X-Session-Affinity: server-2
```

or

```
X-Forwarded-Session: abc123
```

The exact header is platform-specific—there is **no HTTP standard** for a session affinity header.

---

## Example flow

```
Client
 |
POST /login
 |
Load Balancer
 |
API2
```

Response:

```
Set-Cookie: ARRAffinity=xyz
```

Next request:

```
GET /orders

Cookie: ARRAffinity=xyz
```

The load balancer detects the cookie and sends the request to API2 again.

## Why is session affinity often avoided?

In modern cloud applications, sticky sessions are generally considered a workaround rather than a best practice because they:

- Reduce load balancing effectiveness.
- Make scaling less flexible.
- Cause issues if the chosen server fails.
- Complicate deployments and rolling updates.

Instead, applications typically store shared state in:

- Redis
- SQL Server
- Distributed caches
- JWT access tokens (stateless authentication)

This allows any instance to handle any request.

## .NET example

Suppose you store shopping cart data in memory:

```csharp
builder.Services.AddSingleton<CartService>();
```

Without session affinity:

```
Request 1 -> API1 (cart exists)

Request 2 -> API3 (empty cart)
```

With session affinity:

```
Request 1 -> API1

Request 2 -> API1
```

The cart remains available.

A more scalable approach is to store the cart in Redis:

```
API1 ----\
          \
           Redis
          /
API2 ----/
```

Now any API instance can serve the request, eliminating the need for sticky sessions.

# Interview Tips

> A session affinity header or cookie is used by a load balancer to route requests from the same client to the same backend instance. It's mainly needed when applications keep session state in memory. In modern distributed systems, it's generally preferable to avoid sticky sessions by storing session state in a shared store like Redis or by using stateless authentication with JWTs, allowing any instance to process any request.

If asked, "When would you enable session affinity?", a strong answer is:

- **Enable it temporarily** for legacy applications that rely on in-memory session state.
- **Avoid it for new cloud-native applications**, where services should be stateless and share state through external stores (e.g., Redis or a database). This improves scalability, resilience, and load distribution.
