# JWT

## Table of content

## What is JWT?

> JWT (JSON Web Token) is a compact, signed token used to securely transmit information between parties.

Instead of storing authentication information on the server (session), the server issues a token that the client sends with every request. JWT is **stateless** because the server doesn't need to remember the user between requests.

## Authentication Flow

```
User
 |
POST /login
 |
Username/password
 |
Database
 |
Password valid?
 |
Create JWT
 |
Return JWT
 |
Client stores JWT
 |
Authorization: Bearer token
 |
JWT Middleware validates
 |
Endpoint executes
```

## Access Token vs Refresh Token

**Access Token**

- JWT
- Short-lived (e.g., 15–60 minutes)
- Sent with every request
- Used to access APIs

**Refresh Token**

- Long random string
- Stored securely (typically server-side or hashed in a database)
- Longer lifetime (days or weeks)
- Used **only to obtain a new access token** when the current one expires

```
Login
 |
Access Token (15 min)
Refresh Token (30 days)
 |
Access expires
 |
POST /refresh
 |
New Access Token
```

This lets users stay signed in without making access tokens long-lived.

## JWT Structure

A JWT consists of three Base64Url-encoded parts separated by dots.

```
Header.Payload.Signature
```

```
eyJhbGciOiJIUzI1NiIs...
.
eyJzdWIiOiIxMjMiLC...
.
SflKxwRJSMeKKF2QT4...
```

### Header

Contains metadata.

```json
{
  "alg": "HS256",
  "typ": "JWT"
}
```

```
Algorithm = HMAC SHA256
Type = JWT
```

### Payload

Contains claims describing the authenticated user:

```json
{
  "sub": "123",
  "name": "John Smith",
  "email": "john@test.com",
  "role": "Admin",
  "exp": 1755000000
}
```

#### Common Claims

| Claim | Meaning         |
| ----- | --------------- |
| sub   | User Id         |
| name  | Username        |
| email | Email           |
| role  | Role            |
| iss   | Issuer          |
| aud   | Audience        |
| exp   | Expiration      |
| nbf   | Not before      |
| iat   | Issued at       |
| jti   | Unique token id |

Example

```json
{
  "sub": "42",
  "role": "Admin",
  "exp": 1755000000
}
```

### Signature

This protects the token from tampering. If anyone changes even one character in the payload, the signature no longer matches and validation fails.

```
HMACSHA256(base64(header) + "." + base64(payload), secretKey)
```

Suppose someone changes `Role` from `User` to `Admin`

The payload changes. The signature no longer matches. The server rejects the token.

## Is JWT encrypted?

No. It is **not encrypted** by default. Anyone can decode the payload using an online decoder. JWT is encoded and signed:

```
Authorization:
Bearer eyJhbGc...
```

Paste into jwt.io and you'll see

```
Name

Email

Roles

Expiration
```

**Never** store passwords or sensitive data in a JWT.

## Middleware Order

Authentication must run first because authorization needs an authenticated user (`HttpContext.User`) to evaluate policies and roles:

```csharp
app.UseAuthentication();
app.UseAuthorization();
```

## How ASP.NET Core validates JWT

```
GET /orders
Authorization: Bearer xxx
```

The JWT middleware:

1. Extracts token
2. Checks signature
3. Checks expiration
4. Checks issuer
5. Checks audience
6. Creates `ClaimsPrincipal`
7. Assigns it to `HttpContext.User`

Then your endpoint can access

```csharp
var user = HttpContext.User;
```

## Configuring JWT

```csharp
builder.Services
.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(options =>
{
    options.TokenValidationParameters =
        new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = "MyApi",
            ValidAudience = "MyClient",

            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret))
        };
});
```

## Creating a JWT

```csharp
var claims = new[]
{
    new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
    new Claim(ClaimTypes.Name, user.Name),
    new Claim(ClaimTypes.Role, "Admin")
};

var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));

var creds = new SigningCredentials(key,SecurityAlgorithms.HmacSha256);

var token = new JwtSecurityToken(
    issuer: "MyApi",
    audience: "MyClient",
    claims: claims,
    expires: DateTime.UtcNow.AddHours(1),
    signingCredentials: creds);

var jwt = new JwtSecurityTokenHandler().WriteToken(token);
```

## Protecting an Endpoint

Controller:

```csharp
[Authorize]
public class OrdersController : ControllerBase
{
}
```

Minimal API:

```csharp
app.MapGet("/orders", () =>
{
    return "Secret";
})
.RequireAuthorization();
```

## Role-Based Authorization

```csharp
[Authorize(Roles = "Admin")]
```

or

```csharp
.RequireAuthorization(policy => policy.RequireRole("Admin"));
```

## Policy-Based Authorization

Instead of checking roles directly, define a policy:

```csharp
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("ManagersOnly", policy => policy.RequireRole("Manager"));
});
```

Usage

```csharp
.RequireAuthorization("ManagersOnly");
```

Policies can also require custom claims or more complex logic via authorization handlers.

# JWT in Microservices

Instead of each service authenticating the user independently:

```
Client
 |
Identity Service
 |
JWT
 |
Gateway
 |
Orders Service
 |
Payments Service
 |
Inventory Service
```

Each service validates the JWT locally using the signing key (or the identity provider's public key) and reads the claims. This avoids a database lookup on every request.

# Interview Tips

## Why JWT over sessions?

> JWT is stateless, making it easier to scale across multiple servers without shared session storage.

## Can users modify a JWT?

> They can modify the encoded payload, but the signature validation will fail unless they can also generate a valid signature.

## Why shouldn't passwords be stored in JWTs?

> JWT payloads are only encoded, not encrypted by default, so anyone holding the token can read the contents.

## What happens after a JWT expires?

> The API returns `401 Unauthorized`. The client should use a refresh token (if available) to request a new access token.

## What does `UseAuthentication()` do?

> It validates the incoming token and populates `HttpContext.User` with a `ClaimsPrincipal`.

## What does `UseAuthorization()` do?

> It evaluates authorization requirements (roles, policies, claims) using the authenticated user.

For a senior .NET interview, it's also worth understanding **OAuth 2.0 and OpenID Connect (OIDC)**. JWT is just a token format, whereas OAuth 2.0 defines how clients obtain access tokens and OIDC adds user authentication on top of OAuth. Many production .NET applications use JWTs issued by an identity provider such as Microsoft Entra ID, Auth0, or IdentityServer rather than creating tokens themselves.
