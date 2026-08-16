# HttpOnly

## What is HttpOnly?

**HttpOnly** is a **cookie attribute** that tells the browser:

> "Do not allow JavaScript to read or modify this cookie."

Example:

```http
Set-Cookie: accessToken=eyJhbGciOi...; HttpOnly; Secure; SameSite=Lax
```

Here:

- `HttpOnly` → JavaScript cannot access it.
- `Secure` → Sent only over HTTPS.
- `SameSite=Lax` → Helps protect against CSRF attacks.

### Why is it used?

The main purpose is to protect against **XSS (Cross-Site Scripting)** attacks.

Without HttpOnly:

```javascript
document.cookie;
```

could return

```
accessToken=eyJhbGciOi...
```

An attacker who injects JavaScript could steal the JWT.

With HttpOnly:

```javascript
document.cookie;
```

does **not** include the cookie.

The browser still sends the cookie automatically with HTTP requests to the server.

## How authentication works

1. User logs in.
2. Server generates a JWT or session ID.
3. Server responds:

```http
Set-Cookie:
accessToken=jwt...;
HttpOnly;
Secure;
SameSite=Strict
```

4. Browser stores it.

Every future request automatically includes:

```http
GET /api/profile HTTP/1.1
Cookie: accessToken=jwt...
```

The frontend JavaScript never has to read or attach the token.

## HttpOnly vs Local Storage

| HttpOnly Cookie                                                  | Local Storage                       |
| ---------------------------------------------------------------- | ----------------------------------- |
| JS cannot read                                                   | JS can read                         |
| Browser sends automatically                                      | App must add `Authorization` header |
| Safer against XSS                                                | Vulnerable to XSS token theft       |
| Can be vulnerable to CSRF (mitigate with `SameSite`/CSRF tokens) | Not vulnerable to CSRF              |

## Common interview question

**Q: If JavaScript can't read the token, how does the request get authenticated?**

Because **the browser automatically attaches cookies** to matching requests.

```
Browser
 |
GET /api/orders
Cookie: accessToken=...
 |
Server validates cookie
 |
Returns data
```

The frontend doesn't need to manually set the `Authorization` header.

## In ASP.NET Core

Setting an HttpOnly cookie:

```csharp
Response.Cookies.Append("accessToken", token, new CookieOptions
{
    HttpOnly = true,
    Secure = true,
    SameSite = SameSiteMode.Strict,
    Expires = DateTimeOffset.UtcNow.AddHours(1)
});
```

Reading it on the server:

```csharp
var token = Request.Cookies["accessToken"];
```

# Interview Tips

- **HttpOnly** is a **cookie attribute**, not an HTTP header.
- It prevents JavaScript from accessing the cookie.
- It protects authentication cookies from being stolen via XSS.
- The browser still sends HttpOnly cookies automatically with matching requests.
- It's commonly combined with:

  - `Secure` (HTTPS only)
  - `SameSite=Lax` or `Strict` (CSRF protection)
  - `Expires` or `Max-Age` (cookie lifetime)

This is one of the standard ways to store authentication tokens securely in web applications.
