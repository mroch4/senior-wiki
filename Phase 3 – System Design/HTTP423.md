# HTTP 423 — Locked

HTTP **423 Locked** means the requested resource is **currently locked** and cannot be accessed or modified.

It comes from **WebDAV (RFC 4918)** and is less common than standard HTTP status codes.

### Example

```http
HTTP/1.1 423 Locked
Content-Type: application/json

{
  "error": "Resource is locked"
}
```

### Typical use cases

- A document is being edited by another user
- A file/resource has an exclusive lock
- A database-like resource is temporarily locked
- Concurrent modification is prevented

### 423 vs 409

| Status           | Meaning                                                  |
| ---------------- | -------------------------------------------------------- |
| **409 Conflict** | Request conflicts with the current state of the resource |
| **423 Locked**   | Resource is specifically **locked**                      |

**Simple way to remember:**
**423 = "The resource exists, but it's locked right now."**
