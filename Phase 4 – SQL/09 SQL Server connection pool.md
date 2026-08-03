# SQL Server connection pool

## Table of content

1. [What is SQL Server connection pool?](#what-is-sql-server-connection-pool)
2. [Why do we need connection pooling?](#why-do-we-need-connection-pooling)
3. [How connection pooling works](#how-connection-pooling-works)
   - [First request](#first-request)
   - [Second request](#second-request)
4. [Important point](#important-point)
5. [Visual example](#visual-example)
6. [Pool lifecycle](#pool-lifecycle)
7. [Maximum pool size](#maximum-pool-size)
8. [Minimum pool size](#minimum-pool-size)
9. [Pool is based on the connection string](#pool-is-based-on-the-connection-string)
10. [EF Core and connection pooling](#ef-core-and-connection-pooling)
11. [DbContext pooling vs connection pooling](#dbcontext-pooling-vs-connection-pooling)
12. [When pooling causes problems](#when-pooling-causes-problems)
13. [Advanced interview topics](#advanced-interview-topics)
    - [Pool exhaustion](#pool-exhaustion)
    - [Clearing the pool](#clearing-the-pool)
    - [Idle connections](#idle-connections)
    - [Thread safety](#thread-safety)
14. [Interview Tips](#interview-tips)
15. [Typical interview questions and answers](#typical-interview-questions-and-answers)

## What is SQL Server connection pool?

> A **SQL Server connection pool** is a mechanism used by **ADO.NET** (and therefore EF Core, Dapper, etc.) to **reuse existing database connections instead of creating a new TCP/database connection for every request**.

## Why do we need connection pooling?

Opening a SQL Server connection is expensive because it involves:

1. Creating a TCP connection
2. Authentication
3. SQL Server session creation
4. Security checks
5. Allocating server resources

This may take **10–100+ ms**, depending on the environment.

If every request opened a brand-new connection:

```
Request 1
Open -> Authenticate -> Execute -> Close

Request 2
Open -> Authenticate -> Execute -> Close

Request 3
Open -> Authenticate -> Execute -> Close
```

Most of the time is wasted establishing connections.

Instead, .NET keeps a pool of already-open connections.

## How connection pooling works

Suppose your application starts.

Initially:

```
Pool

(empty)
```

### First request

```csharp
using var connection = new SqlConnection(connectionString);
await connection.OpenAsync();
```

No existing connection exists.

.NET creates one.

```
Pool

Connection #1
```

Query executes.

Then:

```csharp
connection.Close();
```

or

```csharp
Dispose();
```

The connection is **NOT actually closed**.

It returns to the pool.

```
Pool

Connection #1 (idle)
```

### Second request

```
OpenAsync()
```

Instead of creating another SQL connection:

```
Pool

Connection #1
```

is reused immediately.

No authentication.
No TCP handshake.
Much faster.

---

## Important point

Calling

```csharp
connection.Close();
```

does **not** usually disconnect from SQL Server.

Instead:

```
Application
 |
Returns connection
 |
Connection Pool
 |
SQL Server
```

The **physical connection** stays open.

## Visual example

Without pooling:

```
Request 1:
App -> Open -> SQL

Request 2:
App -> Open -> SQL

Request 3:
App -> Open -> SQL
```

Many expensive opens.

With pooling

```
Pool:

+---------------+
| Connection #1 |
| Connection #2 |
| Connection #3 |
+---------------+

Request

Take connection
Execute
Return connection
```

## Pool lifecycle

```
Open()
 |
Pool searched
 |
Free connection?
 |
YES -------------> Reuse

NO
 |
Pool not full?
 |
YES -------------> Create new connection

NO
 |
Wait until one becomes available
 |
Timeout
```

## Maximum pool size

Default:

```
Max Pool Size = 100
```

Meaning:

Up to **100 physical SQL Server connections** for a given pool.

Example:

100 simultaneous requests

```
Request 1 -> Conn 1
Request 2 -> Conn 2
...
Request100 -> Conn100
```

Request 101

```
Wait...
```

If no connection becomes available before the connection timeout, you'll get an exception similar to:

> Timeout expired. The timeout period elapsed prior to obtaining a connection from the pool.

## Minimum pool size

Default:

```
Min Pool Size = 0
```

Meaning: `Initially no connections exist.`

If you configure:

```
Min Pool Size = 20
```

the pool creates (or maintains) at least 20 connections so the application is ready to serve traffic more quickly after startup.

## Pool is based on the connection string

Each **unique connection string** gets its own connection pool.

`Server=A;Database=Shop` -----> Pool A

`Server=A;Database=Orders` ---> Pool B

Even a **different user** or **application name** in the connection string creates a separate pool.

## EF Core and connection pooling

EF Core does **not** implement pooling itself.

Instead:

```
EF Core
 |
ADO.NET SqlConnection
 |
Connection Pool
 |
SQL Server
```

Whenever EF Core needs a connection:

```csharp
await context.SaveChangesAsync();
```

it asks ADO.NET for one.

## DbContext pooling vs connection pooling

These are different concepts.

| Connection Pooling                 | DbContext Pooling                                   |
| ---------------------------------- | --------------------------------------------------- |
| Reuses SQL connections             | Reuses DbContext instances                          |
| Implemented by ADO.NET             | Implemented by EF Core                              |
| Enabled by default                 | Must be explicitly enabled                          |
| Saves database connection overhead | Saves object allocation and initialization overhead |

## When pooling causes problems

Pooling is usually beneficial, but issues can occur if:

- Connections are **not disposed** (connection leak), eventually exhausting the pool.
- Long-running transactions hold connections for too long.
- Many different connection strings create many separate pools.
- The database server cannot handle the total number of concurrent physical connections.

Always wrap connections (or `DbContext` instances) in `using`/`await using`, or let dependency injection manage `DbContext` lifetimes in ASP.NET Core.

## Advanced interview topics

### 1. Pool exhaustion

If every connection is in use:

```
100 active connections
 |
New request
 |
Wait
 |
Timeout
```

Common causes include:

- Forgotten `Dispose()`/`Close()`
- Slow queries
- Long-running transactions
- Blocking or deadlocks

### 2. Clearing the pool

You can clear pools manually:

```csharp
SqlConnection.ClearPool(connection);
```

Or clear all pools:

```csharp
SqlConnection.ClearAllPools();
```

This is useful after certain fatal connection errors or database failovers.

### 3. Idle connections

Connections returned to the pool are not kept forever. The pool periodically removes idle connections that are no longer needed, while respecting the configured minimum pool size.

### 4. Thread safety

`SqlConnection` objects are **not thread-safe**. Never share a single `SqlConnection` instance across threads. Instead, each operation should obtain its own connection; thanks to pooling, this is inexpensive.

# Interview Tips

- Be ready to distinguish **connection pooling** from **`DbContext` instances pooling** — they solve different problems.
- Explain that **opening and closing connections frequently is the recommended pattern** because "closing" usually returns the connection to the pool.
- If asked about connection pool timeouts, mention both **pool exhaustion** (all pooled connections are busy) and **connection leaks** (connections not disposed) as common root causes.
- Mention that each unique **connection string** has its own pool, which can unexpectedly increase the total number of database connections if connection strings vary slightly.

## Typical interview questions and answers

Is connection pooling enabled by default?

> Yes, for SQL Server connections in ADO.NET unless explicitly disabled in the connection string.

Does `Close()` really close the connection?

> Usually no. It returns the connection to the pool, leaving the **physical** connection open for reuse.

Who manages connection pooling in EF Core?

> ADO.NET (`SqlConnection`), not EF Core itself.

What identifies a connection pool?

> The connection string (including credentials and other connection options). Different connection strings create different pools.

Can you share one `SqlConnection` across multiple requests?

> No. Acquire a connection when needed, use it, and dispose it promptly.

Should you keep a SQL connection open for the lifetime of the application?

> No. Instead:

```
Open
 |
Execute
 |
Close (returns to pool)
```

Open connections **as late as possible** and close them **as soon as possible**. Because pooling is enabled by default, this pattern is both efficient and scalable.
