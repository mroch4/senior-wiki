# Minimal APIs

## Table of Content

1. [What is Minimal API?](#what-is-minimal-api)
2. [Why Microsoft introduced Minimal APIs](#why-microsoft-introduced-minimal-apis)
3. [Creating a Minimal API](#creating-a-minimal-api)
4. [HTTP Methods](#http-methods)
5. [Route Parameters](#route-parameters)
6. [Query Parameters](#query-parameters)
7. [Dependency Injection](#dependency-injection)
8. [Validation](#validation)
9. [Returning Results](#returning-results)
10. [Endpoint Filters (.NET 7+)](#endpoint-filters-net-7)
11. [Route Groups](#route-groups)
12. [Swagger](#swagger)
13. [Authentication](#authentication)
14. [Advantages](#advantages)
15. [Disadvantages](#disadvantages)
16. [When should you use Minimal APIs?](#when-should-you-use-minimal-apis)
17. [Interview Tips](#interview-tips)

## What is Minimal API?

> Minimal APIs are a lightweight way of creating HTTP APIs in ASP.NET Core without using MVC Controllers.

Traditional **ASP.NET Core**:

```text
Request
 |
Controller
 |
Action Method
 |
Service
 |
Database
```

Minimal API:

```text
Request
 |
Endpoint
 |
Service
 |
Database
```

Instead of creating:

- Controllers
- Action methods
- Route attributes

everything is configured directly in **Program.cs** (or extension methods).

## Why Microsoft introduced Minimal APIs

The goals were:

- Less boilerplate code
- Faster startup
- Better performance
- Easier microservices
- Better support for cloud-native applications

Many microservices only expose 10–20 endpoints. Creating dozens of controllers adds unnecessary complexity.

## Creating a Minimal API

Traditional controller:

```csharp
[ApiController]
[Route("api/products")]
public class ProductController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok();
    }
}
```

Minimal API:

```csharp
var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.MapGet("/products", () =>
{
    return Results.Ok();
});

app.Run();
```

## HTTP Methods

GET

```csharp
app.MapGet("/products", () => productService.GetAll());
```

POST

```csharp
app.MapPost("/products", (Product product) =>
{
    return Results.Created($"/products/{product.Id}", product);
});
```

PUT

```csharp
app.MapPut("/products/{id}", (int id, Product product) =>
{
    return Results.NoContent();
});
```

DELETE

```csharp
app.MapDelete("/products/{id}", (int id) =>
{
    return Results.NoContent();
});
```

## Route Parameters

```csharp
app.MapGet("/products/{id}", (int id) =>
{
    return Results.Ok(id);
});
```

## Query Parameters

```
GET /products?page=2&pageSize=20
```

```csharp
app.MapGet("/products", (int page, int pageSize) =>
{
    return Results.Ok();
});
```

## Dependency Injection

Works exactly like for controllers.

Register service:

```csharp
builder.Services.AddScoped<IProductService, ProductService>();
```

Use it:

```csharp
app.MapGet("/products", (IProductService service) =>
{
    return service.GetAll();
});
```

**ASP.NET** automatically injects the service.

## Validation

```csharp
public class Product
{
    public string Name { get; set; }

    public decimal Price { get; set; }
}
```

POST

```csharp
app.MapPost("/products", (Product product) =>
{
    return Results.Ok(product);
});
```

The framework automatically binds JSON.

Input

```json
{
  "name": "Laptop",
  "price": 1200
}
```

becomes

```csharp
Product product
```

## Returning Results

Instead of

```csharp
return Ok();
```

Minimal APIs use **Results**

```csharp
Results.Ok()

Results.Created()

Results.NotFound()

Results.BadRequest()

Results.NoContent()

Results.Unauthorized()

Results.Forbid()
```

## Endpoint Filters (.NET 7+)

Think of them like middleware for one endpoint.

```csharp
app.MapPost("/products", CreateProduct).AddEndpointFilter<MyFilter>();
```

Useful for

- logging
- validation
- authorization
- auditing

## Route Groups

For cleaner organization instead of

```csharp
app.MapGet("/products");

app.MapPost("/products");

app.MapDelete("/products/{id}");
```

Use

```csharp
var group = app.MapGroup("/products");

group.MapGet("");

group.MapPost("");

group.MapDelete("/{id}");
```

## Swagger

```csharp
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();

app.UseSwagger();

app.UseSwaggerUI();
```

## Authentication

Exactly the same as Controllers.

JWT

```csharp
builder.Services.AddAuthentication()
                .AddJwtBearer();
```

Protect endpoint

```csharp
app.MapGet("/orders", GetOrders).RequireAuthorization();
```

## Advantages

✅ Very little code

✅ Fast startup

✅ Excellent for microservices

✅ Easy testing

✅ High performance

## Disadvantages

Large enterprise applications may become messy if every endpoint stays in `Program.cs`. A common practice is to split endpoints into extension methods or feature folders to keep the code organized.

## When should you use Minimal APIs?

Excellent for

- Microservices
- CRUD APIs
- Cloud-native apps
- Serverless APIs
- Internal APIs

Not ideal for

- Large MVC applications
- Applications with many views
- Complex controller-based architectures

# Interview Tips

- **Minimal APIs** are an ASP.NET Core feature for building lightweight HTTP APIs with less boilerplate than MVC controllers. They still use the same dependency injection, middleware, authentication, and hosting model.
- In a typical microservices architecture, **external clients** (web, mobile, third-party systems) communicate with your system through **REST APIs**, often via an API Gateway, while **internal microservices** communicate using **gRPC** for better performance.
- Minimal APIs and gRPC are not competitors—you can host both in the same ASP.NET Core application if your architecture benefits from exposing both REST and gRPC endpoints.
