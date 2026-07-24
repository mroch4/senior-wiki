# Middleware

## Table of content

## What is middleware?

A middleware is simply a class (or delegate) that:

- receives the current `HttpContext`
- performs some work
- optionally calls the next middleware
- optionally modifies the response

```csharp
public async Task InvokeAsync(HttpContext context, RequestDelegate next)
{
    // Before

    await next(context);

    // After
}
```

Notice there is code both **before** and **after** `next()`. That allows things like timing requests.

```csharp
public async Task InvokeAsync(HttpContext context)
{
    var sw = Stopwatch.StartNew();

    await _next(context);

    sw.Stop();

    Console.WriteLine(sw.ElapsedMilliseconds);
}
```

Think of middleware as a **pipeline** where every HTTP request flows through a series of components before reaching your endpoint, and the response flows back through the same components in reverse.

```
HTTP Request
 |
Exception Middleware
 |
Logging Middleware
 |
Authentication
 |
Authorization
 |
Routing
 |
Endpoint (API/Blazor)
 |
Response travels back
```

## Pipeline execution

Imagine this order:

```
A
B
C
Endpoint
```

Execution looks exactly like nested method calls.

```
Request →

A Before
    B Before
        C Before
            Endpoint
        C After
    B After
A After

← Response
```

## How middleware is registered

Inside Program.cs

```csharp
var app = builder.Build();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
```

Every `Use...()` adds middleware to the pipeline.

## Three middleware registration methods

### 1. Use()

Continues execution. This is the most common.

```csharp
app.Use(async (context, next) =>
{
    Console.WriteLine("Before");

    await next();

    Console.WriteLine("After");
});
```

### 2. Run()

Terminates the pipeline. Nothing after `Run()` executes.

```csharp
app.Run(async context =>
{
    await context.Response.WriteAsync("Hello");
});
```

Think of it as:

```
Request
 |
Run
 |
Finished
```

### 3. Map()

Branches the pipeline.

```csharp
app.Map("/health", health =>
{
    health.Run(async context =>
    {
        await context.Response.WriteAsync("Healthy");
    });
});
```

## Built-in middleware

### Exception handling

```csharp
app.UseExceptionHandler("/error");
```

First one, wraps the rest of the pipeline in a try/catch. It just wraps **everything**. Otherwise exceptions thrown earlier won't be caught.

Without it:

```
Controller throws
 |
Unhandled exception
 |
Kestrel closes request
 |
500
```

With it:

```
Controller throws
 |
Exception middleware
 |
Logs error
 |
Returns ProblemDetails JSON
```

A common implementation:

```csharp
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.StatusCode = 500;

        await context.Response.WriteAsJsonAsync(
            new ProblemDetails
            {
                Title = "Unexpected error"
            });
    });
});
```

### HTTPS redirection

```csharp
app.UseHttpsRedirection();
```

Redirects HTTP → HTTPS. If a request arrives via

```
http://myapi.com
```

it returns

```
301/307 Redirect
 |
https://myapi.com
```

Internally:

```
Request
 |
Is HTTPS?
 |
No
 |
Redirect
 |
Done
```

### HSTS

Often seen with HTTPS.

```csharp
app.UseHsts();
```

Adds `Strict-Transport-Security` header. Browser remembers to never use HTTP again for this site.

```
Strict-Transport-Security:
max-age=31536000
```

```
User types

http://site.com
 |
Browser upgrades
 |
https://site.com

(no network redirect)
```

### Static files

```csharp
app.UseStaticFiles();
```

Serves wwwroot/ without reaching MVC or Minimal APIs.

Suppose `wwwroot/logo.png` and `GET /logo.png`

Pipeline:

```
StaticFiles
 |
Exists?
 |
Yes
 |
Return file
 |
Stop
```

Controller never executes. Without middleware: `404`

### Routing

```csharp
app.UseRouting();
```

Routing doesn't execute endpoints. It only finds which endpoint matches.

Example: `GET /users/10` matches

```csharp
app.MapGet("/users/{id}", ...)
```

```
URL
 |
Routing table
 |
Matched endpoint stored in HttpContext
 |
Continue
```

No endpoint has run yet.

### Authentication

```csharp
app.UseAuthentication();
```

Validates `Cookie, JWT, OpenID, API Key`:

```
Reads header
 |
Validates signature
 |
Checks expiration
 |
Creates ClaimsPrincipal
 |
Stores in HttpContext.User
```

Now:

```csharp
context.User.Identity.Name
```

works.

Without it:

```
HttpContext.User
 |
Empty
```

Internally `Cookie, JWT, OpenID, API Key` all implement `IAuthenticationHandler`

Authentication middleware asks

```
Default scheme
 |
Authenticate
 |
Return ClaimsPrincipal
```

### Authorization

```csharp
app.UseAuthorization();
```

Checks whether you have to correct permissions

### CORS

```csharp
app.UseCors();
```

Adds browser CORS (Cross-Origin Resource Sharing) headers (Access-Control-Allow-Methods, Access-Control-Allow-Headers, Access-Control-Max-Age) to the browser whether a web page from one origin is allowed to access resources from another origin.

### Response Compression

```csharp
app.UseResponseCompression();
```

Compresses JSON using gzip/Brotli.

### Response Caching

```csharp
app.UseResponseCaching();
```

Adds cache support.

## Middleware ordering

This is **extremely** important.

Correct order:

```csharp
app.UseExceptionHandler();

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseCors();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();
```

A typical production pipeline looks like this:

```
Incoming Request
 |
Exception Handler
 |
HSTS (production)
 |
HTTPS Redirection
 |
Forwarded Headers (if behind proxy)
 |
Static Files
 |
Routing
 |
CORS
 |
Authentication
 |
Authorization
 |
Rate Limiting
 |
Output Cache
 |
Endpoint (Minimal API / Controller / Blazor)
 |
Response Compression
 |
Outgoing Response
```

## Custom middleware

```csharp
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;

    public RequestLoggingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        Console.WriteLine($"{context.Request.Method} {context.Request.Path}");

        await _next(context);
    }
}
```

```csharp
app.UseMiddleware<RequestLoggingMiddleware>();
```

## Extension method

```csharp
public static class MiddlewareExtensions
{
    public static IApplicationBuilder UseRequestLogging(
        this IApplicationBuilder app)
    {
        return app.UseMiddleware<RequestLoggingMiddleware>();
    }
}
```

Cleaner registration

```csharp
app.UseRequestLogging();
```

## Middleware vs Filters

| Middleware                            | Filters                                       |
| ------------------------------------- | --------------------------------------------- |
| Entire HTTP pipeline                  | MVC/Minimal API pipeline                      |
| Runs before routing or after response | Runs around controller/action execution       |
| Has access to all requests            | Only endpoint requests                        |
| Good for logging, auth, CORS          | Good for validation, action logic, formatting |

A useful mental model:

- **Middleware** handles cross-cutting concerns for every request.
- **Filters** (for MVC/controllers) handle concerns around action execution. Minimal APIs use **endpoint filters**, which provide similar behavior around endpoint handlers.

## Middleware vs Delegating Handler

Middleware works on **incoming server requests** (processes requests received by your ASP.NET Core application)

```
Browser
 |
API
 |
Middleware
 |
Controller
```

Delegating Handler works on **outgoing HTTP requests** made by your application (processes requests sent by your application)

```
API
 |
HttpClient
 |
External API
```

> Middleware is part of the ASP.NET Core request pipeline. It processes every incoming HTTP request to the application before it reaches an endpoint, and can also inspect or modify the response on the way back. A DelegatingHandler belongs to the HttpClient pipeline. It intercepts outgoing HTTP requests to external services and the corresponding responses. I use middleware for cross-cutting concerns on inbound traffic, such as authentication or exception handling, and DelegatingHandlers for outbound concerns like adding authentication headers, correlation IDs, logging, retries, and resilience policies.

**Note:** Response compression wraps the pipeline, so although it's registered before endpoint execution, its main work happens on the way back out when the response body is written.

# Interview Tips

## Why use middleware instead of putting code in controllers?

Because concerns like logging, authentication, exception handling, and CORS apply to many or all requests. Middleware centralizes this logic, keeping controllers and Minimal API handlers focused on business logic.

## Can middleware short-circuit the pipeline?

Yes. If it doesn't call `await next(context)`, the request stops there. This is useful for serving cached responses, rejecting unauthorized requests early, or handling health checks.

## Is middleware singleton?

Middleware instances are effectively created once when the pipeline is built, so you should treat them as singleton-like. Avoid storing per-request state in instance fields. Instead, use the `HttpContext`, or inject scoped services as parameters to the `InvokeAsync` method:

```csharp
public async Task InvokeAsync(HttpContext context, IMyScopedService service)
{
    // Safe: service is resolved per request
    await _next(context);
}
```

## Can middleware access dependency injection?

Yes. Constructor injection is suitable for singleton services. For scoped services (such as a `DbContext`), inject them into `InvokeAsync` rather than the constructor so they are resolved for each request.

## Why is exception handling first?

It needs to wrap the remainder of the pipeline so it can catch exceptions from any downstream middleware or endpoint.

## Why can static files be early?

Serving a static file doesn't require routing, authentication, model binding, or controller execution in most applications. Handling these requests early avoids unnecessary work and improves performance. If your static files require authorization, you'd need a different configuration.

## Why is `UseRouting()` before authorization?

Routing determines which endpoint is being requested and attaches its metadata (such as `[Authorize]` attributes or endpoint policies) to `HttpContext`. Authorization needs that endpoint metadata to know which policies to enforce.

## Why must `UseAuthentication()` come before `UseAuthorization()`?

Because authorization evaluates the current `HttpContext.User`. If authentication hasn't run yet, there is no authenticated principal to evaluate, so authorization will fail or treat the user as anonymous.

Understanding **why the middleware is ordered this way**—rather than simply memorizing the order—is what typically distinguishes a senior .NET developer in interviews.

## Senior-level takeaway

A strong ASP.NET Core application keeps concerns separated:

- **Middleware**: request/response pipeline (logging, exception handling, authentication, CORS, compression).
- **Endpoint filters**: endpoint-specific cross-cutting logic for Minimal APIs.
- **MediatR pipeline behaviors**: application-layer concerns around commands and queries (validation, transactions, logging).
- **Business services/handlers**: domain logic only.

Understanding where each concern belongs is a key distinction expected from senior .NET developers.
