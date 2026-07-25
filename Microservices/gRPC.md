# gRPC

## Table of Content

1. [What is gRPC?](#what-is-grpc)
2. [Why use gRPC?](#why-use-grpc)
3. [How gRPC Works](#how-grpc-works)
4. [Serialization](#serialization)
5. [Service Definition](#service-definition)
6. [Generated Classes](#generated-classes)
7. [Server Implementation](#server-implementation)
8. [Registering gRPC](#registering-grpc)
9. [Client](#client)
10. [Streaming](#streaming)
    - [Unary (most common)](#unary-most-common)
    - [Server Streaming](#server-streaming)
    - [Client Streaming](#client-streaming)
    - [Bidirectional Streaming](#bidirectional-streaming)
11. [REST vs gRPC](#rest-vs-grpc)
12. [When to Use gRPC](#when-to-use-grpc)
13. [Interview Tips](#interview-tips)

## What is gRPC?

> gRPC is a high-performance Remote Procedure Call (RPC) framework developed by Google.

Instead of sending HTTP requests to URLs like REST, clients call methods on remote services almost as if they were local methods.

REST

```
GET /products/5
```

gRPC

```
GetProduct(5)
```

## Why use gRPC?

Compared to REST:

- Faster
- Smaller messages
- Strongly typed
- Contract-first development
- Better streaming support

It is widely used for communication **between microservices**.

## How gRPC Works

```
Client
 |
HTTP/2
 |
Product Service
 |
Database
```

Notice:

It still uses HTTP. But it uses **HTTP/2**, not traditional HTTP/1.1.

## Serialization

REST

```
JSON
```

gRPC

```
Protocol Buffers (protobuf)
```

JSON

```json
{
  "id": 1,
  "name": "Laptop"
}
```

Protocol Buffer

```
Binary data
```

Binary is

- smaller
- faster
- more efficient

## Service Definition

Everything starts with a `.proto` file.

Example

```proto
syntax = "proto3";

service ProductService
{
    rpc GetProduct(ProductRequest)
        returns(ProductResponse);
}

message ProductRequest
{
    int32 id = 1;
}

message ProductResponse
{
    int32 id = 1;

    string name = 2;
}
```

This file defines the **contract** between client and server.

## Generated Classes

The .NET SDK generates C# classes from the `.proto` file automatically, so you work with strongly typed request and response objects instead of parsing JSON manually.

## Server Implementation

```csharp
public class ProductGrpcService : ProductService.ProductServiceBase
{
    public override Task<ProductResponse> GetProduct(ProductRequest request, ServerCallContext context)
    {
        return Task.FromResult(
            new ProductResponse
            {
                Id = request.Id,
                Name = "Laptop"
            });
    }
}
```

## Registering gRPC

```csharp
builder.Services.AddGrpc();
```

Map service:

```csharp
app.MapGrpcService<ProductGrpcService>();
```

## Client

```csharp
var client = new ProductService.ProductServiceClient(channel);

var response = await client.GetProductAsync(
    new ProductRequest
    {
        Id = 5
    });
```

Looks almost like calling a local method.

## Streaming

One of gRPC's biggest strengths is built-in streaming.

### Unary (most common)

One request => One response

```
Client
 |
Server
```

Equivalent to REST.

### Server Streaming

One request => Many responses

```
Client
 |
Server
 |
Item 1
 |
Item 2
 |
Item 3
```

Examples:

- Stock prices
- Notifications
- Logs

### Client Streaming

Many requests
|
One response

Example:

Uploading a large file in chunks.

### Bidirectional Streaming

Many requests
|
Many responses

Both client and server send messages simultaneously.

Example:

- Chat applications
- Multiplayer games
- Live collaboration
- IoT telemetry

## REST vs gRPC

| Feature          | REST                 | gRPC                   |
| ---------------- | -------------------- | ---------------------- |
| Protocol         | HTTP/1.1 (typically) | HTTP/2                 |
| Data Format      | JSON                 | Protocol Buffers       |
| Speed            | Good                 | Excellent              |
| Payload Size     | Larger               | Smaller                |
| Browser Friendly | Yes                  | Limited (native gRPC)  |
| Streaming        | Limited              | Excellent              |
| Strong Typing    | Optional             | Yes                    |
| Best Use         | Public APIs          | Internal microservices |

## When to Use gRPC

Choose **gRPC** when:

- Services communicate frequently with each other.
- Low latency and high throughput are important.
- You need streaming.
- You control both client and server.

Choose **REST** when:

- Building public APIs.
- Supporting browsers and third-party developers.
- Human-readable payloads are helpful.
- Compatibility across many platforms is a priority.

# Interview Tips

- **gRPC** is an RPC framework using HTTP/2 and Protocol Buffers, making it faster and more efficient than REST for service-to-service communication.
- In a typical microservices architecture, **external clients** (web, mobile, third-party systems) communicate with your system through **REST APIs**, often via an API Gateway, while **internal microservices** communicate using **gRPC** for better performance.
- Minimal APIs and gRPC are not competitors—you can host both in the same ASP.NET Core application if your architecture benefits from exposing both REST and gRPC endpoints.
