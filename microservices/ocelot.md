# Ocelot

> Ocelot is an API Gateway for .NET microservices. Instead of clients calling each microservice directly, they call Ocelot, which routes the request to the correct service.

```text
Client
 |
Ocelot Gateway
 |
Order API
Product API
```

The gateway is configured almost entirely through an **`ocelot.json`** file.

## Basic configuration

Suppose you have:

- Order Service → `https://localhost:7001`
- Product Service → `https://localhost:7002`

```json
{
  "Routes": [
    {
      "UpstreamPathTemplate": "/orders/{everything}",
      "DownstreamPathTemplate": "/api/orders/{everything}",
      "DownstreamScheme": "https",
      "DownstreamHostAndPorts": [
        {
          "Host": "localhost",
          "Port": 7001
        }
      ]
    },

    {
      "UpstreamPathTemplate": "/products/{everything}",
      "DownstreamPathTemplate": "/api/products/{everything}",
      "DownstreamScheme": "https",
      "DownstreamHostAndPorts": [
        {
          "Host": "localhost",
          "Port": 7002
        }
      ]
    }
  ],

  "GlobalConfiguration": {
    "BaseUrl": "https://localhost:5000"
  }
}
```

---

## What does each property mean?

### Upstream

This is what the **client sees**.

```text
https://gateway/orders/15
```

```json
"UpstreamPathTemplate": "/orders/{everything}"
```

### Downstream

This is where Ocelot forwards the request.

```text
https://localhost:7001/api/orders/15
```

Configured by:

```json
"DownstreamPathTemplate": "/api/orders/{everything}"
```

### DownstreamHostAndPorts

Where the service is running.

```json
"DownstreamHostAndPorts": [
  {
    "Host": "localhost",
    "Port": 7001
  }
]
```

In Kubernetes this would usually be the service name instead of `localhost`.

### DownstreamScheme

```json
"DownstreamScheme": "https"
```

or

```json
"DownstreamScheme": "http"
```

### HTTP Methods

Limit which methods are forwarded.

```json
"UpstreamHttpMethod": [
    "GET",
    "POST"
]
```

## Load balancing

If multiple instances of the same service exist:

```json
"DownstreamHostAndPorts": [
  {
    "Host": "order1",
    "Port": 7001
  },
  {
    "Host": "order2",
    "Port": 7001
  }
]
```

Add:

```json
"LoadBalancerOptions": {
    "Type": "RoundRobin"
}
```

Ocelot distributes requests across the instances.

## Authentication

You can require JWT authentication before forwarding requests.

```json
"AuthenticationOptions": {
  "AuthenticationProviderKey": "Bearer",
  "AllowedScopes": []
}
```

The gateway validates the token before the downstream service receives the request.

## Rate limiting

Prevent abuse by limiting requests.

```json
"RateLimitOptions": {
    "EnableRateLimiting": true,
    "Period": "1m",
    "Limit": 100
}
```

Meaning:

```text
100 requests

per minute

per client
```

## Swagger aggregation

Instead of exposing Swagger for every service:

```text
Gateway
 |
/swagger
```

Ocelot (often with an additional package like SwaggerForOcelot) can aggregate documentation from multiple services into a single endpoint.

## Startup configuration

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("ocelot.json");

builder.Services.AddOcelot();

var app = builder.Build();

await app.UseOcelot();

app.Run();
```

## Request flow

```text
Client
 |
GET /orders/15
 |
Ocelot
 |
Matches Route
 |
https://localhost:7001/api/orders/15
 |
Order Service
 |
Response
 |
Ocelot
 |
Client
```

## Typical interview discussion

For a Senior .NET Microservices role, you should know that Ocelot can also provide:

- **Authentication and authorization** (JWT validation)
- **Load balancing** across multiple service instances
- **Request routing** and path rewriting
- **Request aggregation** (combining responses from multiple services)
- **Caching** for selected endpoints
- **Delegating handlers** for custom request/response processing
- **Integration with service discovery** (e.g., Consul) so routes don't need hard-coded hosts

## Ocelot vs YARP

You may also be asked why someone would choose **YARP** instead of Ocelot.

| Ocelot                                                                | YARP                                                            |
| --------------------------------------------------------------------- | --------------------------------------------------------------- |
| API Gateway focused                                                   | Reverse proxy toolkit                                           |
| JSON configuration (`ocelot.json`)                                    | Configurable via code or configuration                          |
| Built-in gateway features (routing, auth, rate limiting, aggregation) | High-performance proxy; gateway features are composed as needed |
| Popular in earlier .NET microservices                                 | Microsoft's recommended reverse proxy for modern ASP.NET Core   |

Today, many new .NET projects choose **YARP** because it's developed by Microsoft, highly performant, and integrates naturally with ASP.NET Core. Ocelot is still widely used and is common in existing enterprise systems, so it's worth being familiar with both.
