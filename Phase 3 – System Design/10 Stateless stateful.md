# Stateless/stateful application

## Table of content

1. [What is difference between stateless/stateful application?](#what-is-difference-between-statelessstateful-application)
2. [Stateless Example](#stateless-example)
3. [Stateful Example](#stateful-example)
4. [Why Stateful Makes Scaling Hard](#why-stateful-makes-scaling-hard)
5. [Why Microservices Prefer Stateless](#why-microservices-prefer-stateless)
6. [Is JWT Stateless?](#is-jwt-stateless)
7. [What About Caching?](#what-about-caching)
8. [Can a Stateless Service Use a Database?](#can-a-stateless-service-use-a-database)
9. [Examples](#examples)
   - [Stateless](#stateless)
   - [Stateful](#stateful)
10. [Interview Tips](#interview-tips)

## What is difference between stateless/stateful application?

> A **stateless application service** does not keep client-specific data between requests. A **stateful application service** stores information about previous interactions and uses it in future requests.

For modern web APIs (especially ASP.NET Core), **stateless is the default and strongly preferred** because it scales much better.

| Stateless                                       | Stateful                                                      |
| ----------------------------------------------- | ------------------------------------------------------------- |
| No client state stored on server                | Client state stored on server                                 |
| Every request contains all required information | Requests rely on previous interactions                        |
| Easy to scale horizontally                      | Harder to scale                                               |
| Simple load balancing                           | Requires session affinity ("sticky sessions") or shared state |
| Better fault tolerance                          | Server failure may lose session                               |

## Stateless Example

A REST API is typically stateless.

```
GET /api/orders/123

Authorization: Bearer eyJ...
```

The server doesn't remember who you are from previous requests.

Instead it gets everything it needs:

- JWT token
- Order ID
- Request body
- Headers

If another server handles the next request, it still works.

```
Client
 |
Request 1
 |
Load Balancer
 |      |
API1   API2
```

Any server can process any request.

## Stateful Example

Imagine logging into an old website.

1. User logs in.
2. Server creates a session object.
3. Session stored in server memory.

```
SessionId -> User123
```

Next request:

```
Cookie: SessionId=ABC123
```

The server looks up:

```
ABC123 -> User123
```

The client only sends the session ID. The server remembers everything else.

## Why Stateful Makes Scaling Hard

Suppose you have two API servers.

```
Load Balancer
 |      |
API1   API2
```

User logs into API1. API1 stores:

```
Session ABC123
```

Next request goes to API2.

API2 has no session.

User appears logged out.

Solutions include:

- Sticky sessions (session affinity)
- Shared session storage (e.g., distributed cache)
- Database-backed sessions

These add **complexity** and can reduce **resilience** or **performance**.

## Why Microservices Prefer Stateless

Stateless services can:

- Scale out easily
- Be restarted without losing client state
- Handle failures gracefully
- Work well in container orchestration platforms like Kubernetes

This is why cloud-native architectures emphasize stateless services.

## Is JWT Stateless?

Yes. A JWT contains the user's identity and claims.

```
Client
 |
JWT
 |
API
```

The API validates the token. No server-side session is required. Each request is self-contained.

## What About Caching?

Caching doesn't necessarily make a service stateful.

```
GET /products/10
```

The service checks Redis:

```
Redis
  Product 10
```

This is application data shared across users, not per-user session state.

The service remains stateless because it isn't relying on stored conversation or session context for a specific client.

## Can a Stateless Service Use a Database?

Absolutely.

```
Request
 |
API
 |
Database
```

The database stores persistent business data, not request/session state.

The service itself does not remember previous requests.

## Examples

### Stateless

- ASP.NET Core Web API
- REST APIs
- gRPC services
- Public web APIs
- Authentication using JWT
- Most microservices

### Stateful

- Shopping cart stored in server session
- Multiplayer game servers tracking player state in memory
- WebSocket servers maintaining active connections
- Chat servers keeping connection-specific context
- In-memory workflow engines

# Interview Tips

> A stateless service doesn't retain client-specific state between requests. Each request is independent and includes all the information needed to process it, making stateless services easy to scale, load balance, and restart. A stateful service stores client session data between requests, which can simplify some workflows but introduces challenges such as session management, sticky sessions, or shared session storage. Modern REST APIs and microservices are typically designed to be stateless, while applications like multiplayer games or long-lived WebSocket connections are common examples of stateful services.

- If asked why REST is stateless, explain that every request contains all the context needed (such as a JWT, headers, route values, and request body), allowing any server instance to handle it.
- If the interviewer mentions **session affinity (sticky sessions)**, explain that it's a technique to route a user's requests to the same server to support stateful sessions, but it reduces the flexibility and scalability benefits of a stateless architecture.
- Clarify that **using a database, distributed cache, or message broker does not make a service stateful**. The distinction is whether the service instance depends on in-memory, client-specific state from previous requests.
