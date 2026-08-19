Yes — **technically, a GET request can insert a row into a database**, but **it should not** in a properly designed RESTful API.

### Why?

HTTP `GET` is intended to be **safe** and **read-only**:

```http
GET /users/123
```

should retrieve user `123` without changing server state.

If you do:

```http
GET /users/create
```

and the server inserts a user into the database, then simply visiting/caching/retrying that URL can create unintended side effects.

### Recommended approach

Use:

```http
POST /users
```

to create a row:

```http
POST /users
Content-Type: application/json

{
  "name": "John"
}
```

Then the server inserts the database row.

| HTTP method | Typical purpose | Should modify DB? |
| ----------- | --------------- | ----------------- |
| `GET`       | Read            | ❌ No             |
| `POST`      | Create          | ✅ Yes            |
| `PUT`       | Replace/update  | ✅ Yes            |
| `PATCH`     | Partial update  | ✅ Yes            |
| `DELETE`    | Delete          | ✅ Yes            |

**Key interview point:** HTTP doesn't technically prevent `GET` from modifying a database. The important point is that **REST semantics require GET to be safe**, meaning it should not cause state changes as a result of the request.
