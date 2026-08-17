# RESTful APIs

## Table of content

1. [What is RESTful APIs?](#what-is-restful-apis)
2. [REST Principles](#rest-principles)
   - [Client-Server](#1-client-server)
   - [Stateless](#2-stateless)
   - [Cacheable](#3-cacheable)
   - [Uniform Interface](#4-uniform-interface)
   - [Layered System](#5-layered-system)
   - [Code on Demand (optional)](#6-code-on-demand-optional)
3. [HTTP Methods](#http-methods)
4. [URI Versioning](#uri-versioning)
   - [Header Versioning](#header-versioning)
   - [Query Parameter Versioning](#query-parameter-versioning)
   - [Resource Expansion / No Versioning](#resource-expansion--no-versioning)
5. [Error handling](#error-handling)
6. [IDEMPOTENCY](#idempotency)
   - [Idempotent methods](#idempotent-methods)
   - [Non-idempotent](#non-idempotent)
   - [Depends on implementation](#depends-on-implementation)
7. [Caching](#caching)
   - [HTTP caching headers](#http-caching-headers)
   - [ETags](#etags)
   - [Full Layered Caching Architecture](#full-layered-caching-architecture)
8. [HATEOAS](#hateoas)
9. [Authentication and authorization](#authentication-and-authorization)
   - [Authentication](#authentication)
   - [Authorization](#authorization)
   - [Why the distinction matters](#why-the-distinction-matters)

## What is RESTful APIs?

> A RESTful API is an API that follows the architectural principles of **REST (Representational State Transfer)**. It exposes resources through a uniform interface using standard HTTP methods. It is **stateless**, **cacheable**, and follows architectural constraints like **client–server** separation and **layered systems**. Resources are represented with URLs, and operations are performed via GET/POST/PUT/PATCH/DELETE.

It is an **architectural style** for designing distributed systems.
REST is **not a protocol** like HTTP. REST typically uses HTTP, but REST =/= HTTP.

## REST Principles

A REST API should satisfy these 6 constraints:

### 1. Client-Server

Client and server are independent:

- client doesn't know how data is stored
- server doesn't know how the UI works

```
Angular/Blazor/Mobile App
 |
HTTP
 |
ASP.NET Core API
```

### 2. Stateless

> Stateless means the server does not store any client context between requests. Every request must contain all necessary information — authentication, parameters, and payload — so any server instance can process it. This simplifies scaling, improves reliability, and enables effective caching.

- horizontal scaling becomes trivial - **any** request can go to **any** server instance because no instance holds user state
- no server‑side session - server does **not** store client context between requests (no session objects, no “logged‑in user” stored in memory)
- fault tolerance/improves reliability - if one node dies, another can take over without losing context
- load balancing
- simpler caching - because responses depend only on the request, caches become predictable and safe

### 3. Cacheable

### 4. Uniform Interface

Resources are identified using URLs.

Example

Good

```
/customers
/customers/15
/orders
/orders/45
```

Bad

```
/getCustomer
/createCustomer
/deleteCustomer
```

Instead, the HTTP method determines the operation.

### 5. Layered System

Clients don't know whether they communicate with:

- API Gateway
- Reverse Proxy
- Load Balancer
- Microservice
- Cache

They only know the endpoint.

```
Client
 |
API Gateway
 |
Authentication
 |
Order Service
 |
Database
```

### 6. Code on Demand (optional)

A server may send executable code. Almost never used in modern APIs.

## HTTP Methods

| Method | Purpose        | Safe | Idempotent | Example                  | Endpoint scope             | Returns        |
| ------ | -------------- | ---- | ---------- | ------------------------ | -------------------------- | -------------- |
| GET    | Read           | ✅   | ✅         | products/, products/{id} | Collection/Single‑resource | 200 OK         |
| POST   | Create         | ❌   | ❌         | products/                | Collection                 | 201 Created    |
| PUT    | Entire replace | ❌   | ✅         | products/{id}            | Single‑resource            |
| PATCH  | Partial update | ❌   | Usually    | products/{id}            | Single‑resource            |
| DELETE | Delete         | ❌   | ✅         | products/{id}            | Single‑resource            | 204 No Content |

**Safe** = server state is not modified (verbs: GET, HEAD, OPTIONS)

```
202 Accepted
```

Means processing will happen later (common in asynchronous or event-driven systems).

## URI Versioning

`/api/v1/orders`  
`/api/v2/orders`

✅ **Pros**

- clear for clients
- Easy to route and maintain
- Simple documentation

❌ **Cons**

- Breaks REST purity (version is not a resource)
- Forces clients to update URLs
- Harder to evolve gradually

### Header Versioning

`Accept: application/vnd.myapp.v2+json`

✅ **Pros**

- Clean URLs
- More REST‑aligned (content negotiation)
- Allows fine‑grained versioning per representation

❌ **Cons**

- Harder to test manually
- Less visible to developers
- Requires more tooling support

### Query Parameter Versioning

`GET /orders?version=2`

✅ **Pros**

- Easy to experiment
- Backwards compatible
- Simple for browser-based clients

❌ **Cons**

- Not ideal for long-term API design
- Can be ignored by caches
- Considered less formal

### Resource Expansion / No Versioning

Evolve the API without breaking old clients.

✅ **Pros**

- Zero breaking changes
- Very flexible
- Used by giants like Stripe

❌ **Cons**

- Requires extremely careful design
- Harder for teams without strong API governance

## Error handling

A strong REST API error format usually includes **five** elements:

### 1. HTTP status code

Clear mapping:

- 400 → client mistake
- 404 → resource missing
- 500 → server failure
- 401/403 → auth issues

### 2. Machine‑readable error code

Clients rely on this for logic:
`ORDER_NOT_FOUND`, `INVALID_EMAIL`, `PAYMENT_DECLINED`

### 3. Human‑readable message

Something developers can understand instantly.

### 4. Correlation ID

A unique request identifier generated by the system. Critical for debugging distributed systems.

### 5. Optional details

Field‑level validation errors, hints, or documentation links.

```json
{
  "status": 404,
  "error": "ORDER_NOT_FOUND",
  "message": "Order with id 12345 does not exist.",
  "correlationId": "b7f2c9e1-4a3d-4c9a-9a12-ff8e2d9c1a01",
  "details": {
    "suggestion": "Verify the order ID or contact support."
  }
}
```

## IDEMPOTENCY

> Repeating the same request produces the same final state.

### Idempotent methods

- GET — reading a resource doesn’t change anything
- HEAD — metadata only (same as GET but returns only headers)
- OPTIONS — capability discovery (returns allowed methods)
- PUT — replacing a resource with the same payload always results in the same state
- DELETE — deleting twice still results in “resource gone”

### Non‑idempotent

- POST — each call may create a new resource or trigger a non‑repeatable action

### Depends on implementation

- PATCH — can be idempotent _if_ the patch describes a deterministic update  
  Example:
  - `PATCH /users/1 { "status": "active" }` → idempotent
  - `PATCH /users/1 { "balance": "+10" }` → NOT idempotent, can result 120$ or 150$ based on current balance value

Why idempotency matters:

- **Retry safety** — clients, proxies, and load balancers can safely retry requests
- **Network resilience** — if a request times out, repeating it won’t corrupt data
- **Distributed systems correctness** — essential for microservices and queues
- **Predictable behavior** — avoids accidental duplication or inconsistent state

---

## CACHING

> Combine **HTTP caching headers**, **ETags**, **CDN/proxy caching**, and **server‑side caching** into a layered system. Each layer reduces server load, network traffic and latency before the request reaches your application.

#### **HTTP caching headers**

Tell browsers, CDNs, and proxies how long they can reuse a response:

- Cache-Control — defines TTL, public/private, must‑revalidate
- Expires — older, but still used
- ETag — lets clients check if the resource changed
- Last-Modified — alternative to ETag

```
Cache-Control: public, max-age=60
ETag: "order-12345-v7"
```

This means:  
Clients can cache for 60 seconds, and after that they can revalidate using the ETag.

#### **ETags**

ETags allow **smart revalidation** instead of full downloads.

Client sends:

```
If-None-Match: "order-12345-v7"
```

Server responds:

- **304 Not Modified** → no body, super cheap
- **200 OK** → new version + new ETag

This reduces bandwidth and CPU load.

#### ⭐ Interview‑ready summary

> I would implement caching using layered architecture: HTTP caching headers, ETags for revalidation, CDN edge caching for massive offload, Redis for server‑side caching, and a clear invalidation strategy using TTL or event‑driven updates. This ensures low latency, high throughput, and minimal load on the database.

**Full Layered Caching Architecture**

1. Browser cache
2. CDN edge cache
3. Redis cache
4. Database

Most requests never reach the database.

## HATEOAS

> HATEOAS (Hypermedia As The Engine Of Application State) means the server includes links in responses to guide the client through possible actions. It’s part of the original REST constraints but rarely used in modern APIs because it adds complexity and most clients already know the endpoint structure. Useful in hypermedia‑driven systems, but not common in everyday CRUD APIs.

```json
{
  "id": 123,
  "status": "processing",
  "links": {
    "self": "/orders/123",
    "cancel": "/orders/123/cancel",
    "items": "/orders/123/items"
  }
}
```

- self → where you are
- cancel → what you can do next
- items → related resources

❌ Why almost nobody uses it:

- More JSON, more bandwidth
- Mobile apps, SPAs, and backend services already know the API structure
- Swagger/OpenAPI already solves discoverability
- It adds complexity without real benefit

✅ When HATEOAS _is_ useful:

- Hypermedia APIs (HAL, JSON:API)
- Dynamic workflows
- Systems where the server must dictate allowed actions
- APIs that change often and must avoid breaking clients

## Authentication and authorization

> Authentication verifies the identity of the requester, while authorization determines what actions or resources that identity is allowed to access. You authenticate first, then authorize based on roles, scopes, or permissions.

### Authentication

Verifies **identity**. It answers: _“Who are you?”_

- Password login
- OAuth2 token exchange
- API keys
- JWT validation

### Authorization

Determines **permissions**. It answers: _“What can you access or perform?”_

- Can the user read this order?
- Can they update it?
- Are they allowed to delete it?

### Why the distinction matters

- Auth happens **before** authorization.
- Auth is about **identity**, authorization is about **rights**.
- You can be authenticated but **not authorized** (classic 403 case).
