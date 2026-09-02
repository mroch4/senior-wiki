**XSJS** stands for **Extended Services for JavaScript**. It’s a server-side JavaScript technology historically associated with **SAP HANA / SAP HANA XS (XS Classic)**.

In simple terms:

- **JavaScript running on the server**, rather than in the browser.
- Used to create **HTTP endpoints / REST-like services**.
- Could directly interact with **SAP HANA databases**.
- Often used in older SAP applications before newer approaches such as **Node.js-based SAP HANA XS Advanced** became common.

Example of a very simple XSJS service:

```javascript
$.response.contentType = "application/json";

var result = {
  message: "Hello from XSJS",
};

$.response.setBody(JSON.stringify(result));
```

A request to that XSJS endpoint could return:

```json
{
  "message": "Hello from XSJS"
}
```

### XSJS vs normal JavaScript

|                 | JavaScript             | XSJS                              |
| --------------- | ---------------------- | --------------------------------- |
| Runs            | Browser / Node.js      | SAP HANA server                   |
| Purpose         | General programming    | SAP backend services              |
| Database access | Depends on environment | Designed for HANA                 |
| Typical era     | Current                | Mostly legacy SAP HANA XS Classic |
| Syntax          | JavaScript             | JavaScript + SAP XS APIs          |

You'll often see things like:

```javascript
$.hdb.getConnection();
$.request;
$.response;
```

Those `$` APIs are **SAP-specific XSJS APIs**, not standard JavaScript.

If you're encountering **XSJS in a job/project**, it's probably worth learning the relationship between **XSJS → HANA XS Classic → XS Advanced → Node.js**, because that explains a lot of older SAP architectures.

---

Sure — the important thing to understand is **where XSJS sits in the SAP architecture** and why you might encounter it in a legacy application.

### 1. What problem did XSJS solve?

Imagine you have an SAP HANA database containing:

```text
Ships
 ├── ShipId
 ├── Name
 ├── Location
 └── EngineStatus
```

A web application needs to retrieve that data.

Traditionally, you'd have:

```text
Browser
   ↓ HTTP
Backend application
   ↓
Database
```

With **XSJS**, SAP provided a way to put JavaScript directly into the HANA application server:

```text
Browser
   ↓ HTTP
XSJS
   ↓
SAP HANA
   ↓
Database
```

So an `.xsjs` file could effectively act as a **backend API endpoint**.

---

### 2. What does an XSJS file look like?

For example:

```javascript
var connection = $.hdb.getConnection();

var result = connection.executeQuery('SELECT * FROM "MY_SCHEMA"."SHIPS"');

connection.commit();

$.response.contentType = "application/json";
$.response.setBody(JSON.stringify(result));
```

The browser could call:

```text
GET /services/ships.xsjs
```

and receive:

```json
[
  {
    "SHIP_ID": 101,
    "NAME": "Aurora",
    "ENGINE_STATUS": "OK"
  },
  {
    "SHIP_ID": 102,
    "NAME": "Baltic Star",
    "ENGINE_STATUS": "WARNING"
  }
]
```

So conceptually, XSJS is somewhat similar to writing this in a modern backend:

```csharp
[HttpGet("ships")]
public IActionResult GetShips()
{
    var ships = db.Ships.ToList();
    return Ok(ships);
}
```

The **architecture is different**, but the role is similar: **receive an HTTP request, execute backend logic, talk to a database, return a response**.

---

## 3. Why the `$` everywhere?

XSJS provides SAP-specific APIs through the `$` object.

For example:

```javascript
$.request;
```

contains information about the incoming HTTP request.

```javascript
$.response;
```

allows you to construct the HTTP response.

For example:

```javascript
$.response.status = $.net.http.OK;
$.response.contentType = "application/json";
$.response.setBody(JSON.stringify({ message: "Success" }));
```

There were also APIs for database access, logging, HTTP calls, etc.

So this:

```javascript
$.response.setBody("Hello");
```

is **not standard JavaScript**.

It's JavaScript running inside the **SAP HANA XS runtime**.

---

# 4. XSJS and XS Classic

This is probably the most important terminology.

**XSJS belongs primarily to SAP HANA XS Classic.**

The architecture looked roughly like:

```text
                 SAP HANA
┌─────────────────────────────────────┐
│                                     │
│       XS Classic Application        │
│                                     │
│   ┌─────────┐       ┌───────────┐  │
│   │  XSJS   │ ────→ │   HANA    │  │
│   │ Backend │       │ Database  │  │
│   └─────────┘       └───────────┘  │
│                                     │
└─────────────────────────────────────┘
          ↑
          │ HTTP
          ↓
     Web application
```

XS Classic wasn't just XSJS.

It included several concepts:

- **XSJS** — server-side JavaScript
- **XSODATA** — OData services
- **XSDS** — database/service definitions
- **`.xsapp`** — application configuration
- **`.xsaccess`** — security/access configuration
- HANA calculation views
- SQLScript
- database artifacts

So if you see a project containing files such as:

```text
.xsapp
.xsaccess
.xsjs
.xsodata
.hdbdd
.hdbschema
```

you're probably looking at an **older HANA XS Classic application**.

---

# 5. XSJS vs Node.js

This is where things get confusing.

SAP later introduced **SAP HANA XS Advanced (XSA)**.

Instead of SAP's proprietary JavaScript runtime, applications could use more standard technologies such as:

```text
Node.js
Java
JavaScript
npm
Express
```

So the evolution is roughly:

```text
                 Older
                   ↓
        HANA XS Classic
              ┌────┴────┐
              │  XSJS   │
              └────┬────┘
                   ↓
          HANA XS Advanced
              ┌────┴────┐
              │ Node.js │
              └────┬────┘
                   ↓
        Modern SAP development
```

A modern Node.js backend might look like:

```javascript
app.get("/ships", async (req, res) => {
  const ships = await getShipsFromDatabase();
  res.json(ships);
});
```

XSJS might look more like:

```javascript
var connection = $.hdb.getConnection();

var ships = connection.executeQuery('SELECT * FROM "SHIPS"');

$.response.contentType = "application/json";
$.response.setBody(JSON.stringify(ships));
```

The idea is similar, but the runtime and APIs are completely different.

---

# 6. Why would a company still have XSJS?

This is actually quite common in **legacy SAP environments**.

Imagine a company has been collecting data for 10–15 years.

Their architecture might look like:

```text
                  Ships
                    │
                    ↓
              IoT / sensors
                    │
                    ↓
              Azure / Cloud
                    │
                    ↓
             Integration layer
                    │
                    ↓
              SAP HANA
                    │
             ┌──────┴──────┐
             ↓             ↓
           XSJS          SQL
             │
             ↓
        Web application
```

The company doesn't necessarily want to rewrite the entire backend just because the technology is old.

Therefore you might encounter:

```text
Angular
   ↓
REST/OData
   ↓
XSJS
   ↓
HANA
```

while newer applications might be:

```text
Angular
   ↓
.NET / Node.js
   ↓
REST API
   ↓
HANA / Azure
```

---

# 7. XSJS is especially relevant if you're interviewing for a legacy SAP project

If you see **XSJS** in a job description, I'd immediately want to understand:

1. Is it **XS Classic**?
2. Is the application being **maintained** or **migrated**?
3. Are they moving from XSJS to **Node.js/.NET**?
4. Is the frontend **Angular**?
5. Is HANA still the primary database?
6. Are they using **OData**?
7. Are there Azure integrations?

For example, an application could have:

```text
Angular
   │
   │ HTTP/OData
   ↓
XSJS
   │
   │ SQL
   ↓
SAP HANA
```

And your job might involve gradually moving toward:

```text
Angular
   │
   ↓
.NET Web API
   │
   ├────→ SAP HANA
   │
   └────→ Azure services
```

In that situation, you **don't need to become an XSJS expert**. You mainly need to understand the existing architecture and be able to maintain/replace the old services.

### The key takeaway

Think of **XSJS as an old SAP-specific backend technology**:

> **XSJS = server-side JavaScript used in SAP HANA XS Classic to expose backend logic and interact with HANA.**

If you're coming from **.NET + Angular**, the easiest mental mapping is:

```text
XSJS                  ≈      .NET Web API
$.request             ≈      HttpRequest
$.response            ≈      IActionResult / HttpResponse
$.hdb                  ≈      database connection/API
HANA SQL              ≈      SQL / EF queries
XSODATA               ≈      OData API
Angular                →      Angular
```

The mapping isn't exact, but it's useful for understanding a legacy codebase quickly.
