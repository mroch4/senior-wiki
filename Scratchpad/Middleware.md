# 7. Authorization

```csharp
app.UseAuthorization();
```

Now that user exists

```
HttpContext.User
```

Authorization evaluates policies.

Example

```csharp
.RequireAuthorization("Admin")
```

Pipeline

```
Authenticated?

|

Yes

|

Has role Admin?

|

No

|

403
```

If unauthenticated

```
401
```

## Difference

Authentication

```
Who are you?
```

Authorization

```
What may you do?
```

# 8. CORS

```csharp
app.UseCors();
```

One of the most misunderstood middleware.

Browser sends

```
Origin

https://client.com
```

Server decides

```
Allowed?

|

Yes

|

Access-Control-Allow-Origin
```

Browser then allows JavaScript.

Without header

```
Browser blocks response
```

Notice:

Server **did** return the response.

Browser refused to expose it.

## Preflight request

Before

```
PUT

DELETE

PATCH
```

browser often sends

```
OPTIONS
```

Middleware answers

```
Allowed methods

Allowed headers

Allowed origin
```

Only then is the actual request sent.

# 9. Response Compression

```csharp
app.UseResponseCompression();
```

Suppose

```
2 MB JSON
```

Middleware

```
Endpoint returns JSON

|

Compress

|

Send gzip

|

Client decompresses
```

No controller changes required.

# 10. Response Caching

```csharp
app.UseResponseCaching();
```

Client

```
GET /products
```

Middleware

```
Cached?

|

Yes

|

Return cache

|

Skip controller
```

Unlike `IMemoryCache`, this is driven by HTTP cache headers and follows HTTP caching rules.

# 11. Output Cache (.NET 7+)

Newer and generally preferred over Response Caching for many APIs.

```csharp
app.UseOutputCache();

app.MapGet("/products", GetProducts)
   .CacheOutput();
```

Unlike Response Caching

- caches on the server
- doesn't rely on browser behavior
- configurable per endpoint
- much faster for repeated requests

# 12. Rate Limiting (.NET 7+)

```csharp
app.UseRateLimiter();
```

Request

```
100 requests/sec
```

Policy

```
50/sec
```

Result

```
429 Too Many Requests
```

before controller executes.

Internally

```
Request

|

Acquire permit

|

Available?

|

Yes

|

Continue

No

|

429
```

Useful against abuse and accidental traffic spikes.

# 13. Request Localization

```csharp
app.UseRequestLocalization();
```

Reads

```
Accept-Language

pl-PL
```

Sets

```
CurrentCulture

CurrentUICulture
```

Now

```csharp
DateTime.Now.ToString()
```

formats according to the user's culture.

# 14. Forwarded Headers

Critical behind reverse proxies like Nginx, Azure App Service, or Kubernetes ingress.

```csharp
app.UseForwardedHeaders();
```

Without it

```
Request.IsHttps

false
```

because Kestrel receives HTTP from the proxy.

Proxy adds

```
X-Forwarded-Proto

https
```

Middleware restores

```
Request.Scheme = https
```

Without this, redirect loops and incorrect URL generation are common.
