# CORS

## What are CORS headers?

CORS (Cross-Origin Resource Sharing) headers tell the browser whether a web page from one origin is allowed to access resources from another origin.

For example:

- Frontend: `https://app.example.com`
- API: `https://api.example.com`

Since these are different origins (different subdomains), the browser blocks the request unless the API explicitly allows it.

## 1. Access-Control-Allow-Origin

Specifies which origin can access the resource.

```
Access-Control-Allow-Origin: https://app.example.com
```

or

```
Access-Control-Allow-Origin: *
```

### When to use `*`

Only for public APIs that don't use credentials.

If cookies or authentication are involved:

❌ Invalid

```
Access-Control-Allow-Origin: *
Access-Control-Allow-Credentials: true
```

Instead:

```
Access-Control-Allow-Origin: https://app.example.com
Access-Control-Allow-Credentials: true
```

---

## 2. Access-Control-Allow-Methods

Specifies allowed HTTP methods.

```
Access-Control-Allow-Methods: GET, POST, PUT, DELETE
```

Without it, the browser may reject non-simple requests.

## 3. Access-Control-Allow-Headers

Specifies which request headers the browser may send.

Example:

```
Access-Control-Allow-Headers: Authorization, Content-Type
```

Without this header, a request like:

```
Authorization: Bearer eyJ...
```

would be blocked after the preflight.

## 4. Access-Control-Allow-Credentials

Allows cookies or HTTP authentication to be included.

```
Access-Control-Allow-Credentials: true
```

Frontend:

```javascript
fetch(url, {
  credentials: "include",
});
```

Server:

```
Access-Control-Allow-Credentials: true
Access-Control-Allow-Origin: https://app.example.com
```

Cannot be used with `*`.

## 5. Access-Control-Expose-Headers

By default, JavaScript can only read a small set of response headers.

If your API returns:

```
X-Request-ID: 12345
```

JavaScript cannot access it unless you expose it:

```
Access-Control-Expose-Headers: X-Request-ID
```

Then:

```javascript
response.headers.get("X-Request-ID");
```

works.

## 6. Access-Control-Max-Age

Specifies how long the browser may cache the preflight response.

```
Access-Control-Max-Age: 3600
```

Meaning:

```
1 hour
```

The browser won't send another OPTIONS request during that time (subject to browser-specific limits).

## Preflight Request

When a request is "non-simple" (e.g., uses `PUT`, `DELETE`, or custom headers like `Authorization`), the browser first sends an `OPTIONS` request.

### Browser

```
OPTIONS /api/orders HTTP/1.1

Origin: https://app.example.com
Access-Control-Request-Method: PUT
Access-Control-Request-Headers: Authorization
```

### Server response

```
HTTP/1.1 204 No Content

Access-Control-Allow-Origin: https://app.example.com
Access-Control-Allow-Methods: PUT
Access-Control-Allow-Headers: Authorization
Access-Control-Max-Age: 3600
```

Only after receiving this response does the browser send the actual `PUT` request.

## ASP.NET Core Configuration

Typical configuration:

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy.WithOrigins("https://app.example.com")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});
```

Enable it:

```csharp
app.UseCors("Frontend");
```

For cookies:

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy.WithOrigins("https://app.example.com")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});
```

## Middleware Order

The correct order is important:

```csharp
app.UseRouting();

app.UseCors();          // Before authentication/authorization and endpoint execution

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
```

If `UseCors()` is placed after `MapControllers()`, the CORS middleware won't process the requests correctly, and browsers may reject cross-origin calls.

## Browser vs. Server

A common interview point is that **CORS is enforced by browsers, not by servers**.

For example:

```bash
curl https://api.example.com/orders
```

works even if no CORS headers are present, because `curl` doesn't enforce CORS.

However, this JavaScript call from another origin:

```javascript
fetch("https://api.example.com/orders");
```

may fail in the browser if the API doesn't return the appropriate CORS headers.

# Interview Tips

## Why is a preflight request needed?

To let the browser verify that the server permits the intended HTTP method and request headers before sending a potentially unsafe cross-origin request.

## What triggers a preflight request?

- Methods other than `GET`, `HEAD`, or `POST` (e.g., `PUT`, `PATCH`, `DELETE`)
- Custom request headers (e.g., `Authorization`)
- A `POST` with a non-simple `Content-Type` such as `application/json`

## Why doesn't Postman have CORS issues?

Postman is not a browser and does not enforce the browser's same-origin policy.

## Can CORS secure an API?

No. CORS is a browser security feature to protect users from unauthorized cross-origin access. It is **not** an authentication or authorization mechanism. APIs should still use proper authentication (e.g., JWT or cookies) and authorization checks regardless of CORS settings.

## Why do I get a CORS error even though my API returns `200 OK`?

Because the browser received the response but blocked JavaScript from accessing it due to missing or incorrect CORS headers. From the browser's perspective, it's as if the response doesn't exist.
