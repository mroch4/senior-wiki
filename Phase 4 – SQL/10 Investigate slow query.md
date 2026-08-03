# Investigate slow query

## Table of content

1. [Identify the slow query](#1-identify-the-slow-query)
2. [Check the execution plan](#2-check-the-execution-plan)
3. [Look for expensive operators](#3-look-for-expensive-operators)
4. [Check indexes](#4-check-indexes)
5. [Check statistics](#5-check-statistics)
6. [Examine the WHERE clause](#6-examine-the-where-clause)
7. [Look for SELECT \*](#7-look-for-select-)
8. [Check joins](#8-check-joins)
9. [Investigate blocking and deadlocks](#9-investigate-blocking-and-deadlocks)
10. [Check parameter sniffing](#10-check-parameter-sniffing)
11. [Look at I/O](#11-look-at-io)
12. [Check database waits](#12-check-database-waits)
13. [Measure before and after](#13-measure-before-and-after)
14. [Common Causes of Slow Queries](#common-causes-of-slow-queries)
15. [Interview Tips](#interview-tips)

## 1. Identify the slow query

Ask:

- Which query is slow?
- How long does it take?
- Is it always slow or only sometimes?
- Is it slow only in production?
- Did it become slow recently?

Useful tools:

- SQL Server Profiler (older)
- Extended Events (recommended)
- Query Store
- Application logs
- Application Performance Monitoring (App Insights, Datadog, New Relic)

## 2. Check the execution plan

This is usually the first thing experienced SQL developers do.

Execution plans show how SQL Server executes the query.

Look for:

❌ Table Scan

Instead of using an index, SQL Server reads every row.

```
SELECT *
FROM Orders
WHERE CustomerId = 15
```

If **CustomerId** isn't indexed:

```
Table Scan
```

With an index:

```
Index Seek
```

which is much faster.

## 3. Look for expensive operators

Common expensive operators include:

- Table Scan
- Clustered Index Scan
- Sort
- Hash Match
- Key Lookup
- Nested Loop (sometimes)
- Parallelism (not always bad)

Execution plans show percentage cost.

Example:

```
Hash Match 70%
Sort 20%
Key Lookup 10%
```

You immediately know where to investigate.

## 4. Check indexes

Ask:

- Is there an index?
- Is SQL Server using it?
- Is it fragmented?
- Is it selective enough?

Useful queries:

```
sp_helpindex 'Orders'
```

or

```
sys.indexes
```

Missing indexes can often improve performance dramatically.

## 5. Check statistics

SQL Server relies on statistics to estimate row counts.

Outdated statistics can cause SQL Server to choose a poor execution plan.

Update them:

```sql
UPDATE STATISTICS Orders;
```

or

```sql
EXEC sp_updatestats;
```

## 6. Examine the WHERE clause

Bad:

```sql
WHERE YEAR(OrderDate) = 2025
```

The function **prevents** index usage.

Better:

```sql
WHERE OrderDate >= '2025-01-01'
AND OrderDate < '2026-01-01'
```

Now SQL Server can perform an Index Seek.

## 7. Look for SELECT \*

❌ Bad:

```sql
SELECT *
```

✅ Good:

```sql
SELECT OrderId, CustomerName, Total
```

Retrieving unnecessary columns increases I/O and network traffic.

## 8. Check joins

Questions:

- Are the join columns indexed?
- Are unnecessary joins included?
- Are the join types appropriate?

Example:

```
Orders
JOIN Customers
JOIN Products
JOIN Categories
JOIN Suppliers
```

Sometimes one or more joins are unnecessary.

## 9. Investigate blocking and deadlocks

Sometimes the query itself is efficient, but it waits for locks held by other transactions.

Check:

```sql
sp_who2
```

or Dynamic Management Views (DMVs):

```sql
sys.dm_exec_requests
```

Blocking is a common production issue.

## 10. Check parameter sniffing

A classic SQL Server performance problem.

Suppose:

```
CustomerId = 5
```

returns 2 rows.

SQL Server caches a plan optimized for 2 rows.

Later:

```
CustomerId = 100
```

returns 3 million rows.

SQL Server may reuse the same plan, resulting in poor performance.

Possible solutions include:

- `OPTION (RECOMPILE)`
- `OPTIMIZE FOR`
- Refactoring the query or stored procedure

## 11. Look at I/O

A query may spend most of its time reading from disk.

Enable statistics:

```sql
SET STATISTICS IO ON;
SET STATISTICS TIME ON;
```

Example output:

```
Logical reads: 250000
CPU time: 1200 ms
Elapsed time: 1800 ms
```

High logical reads often indicate inefficient access patterns.

## 12. Check database waits

Sometimes SQL isn't the bottleneck.

The server may be waiting on:

- Disk I/O
- Memory
- Locks
- CPU
- Network

Useful DMVs:

```sql
sys.dm_os_wait_stats
```

This helps identify whether the issue lies outside the query itself.

## 13. Measure before and after

Always compare:

- Duration
- CPU time
- Logical reads
- Physical reads
- Execution plan
- Memory grant

Optimization should be based on measurable improvements.

## Common Causes of Slow Queries

| Problem              | Solution                                           |
| -------------------- | -------------------------------------------------- |
| Table Scan           | Add or improve indexes                             |
| Missing index        | Create an appropriate index                        |
| Outdated statistics  | Update statistics                                  |
| Functions in `WHERE` | Rewrite predicates to be index-friendly (SARGable) |
| `SELECT *`           | Select only required columns                       |
| Large joins          | Reduce joins or index join columns                 |
| Parameter sniffing   | Recompile or optimize query plan                   |
| Blocking             | Reduce lock duration or investigate transactions   |
| High I/O             | Improve indexing and query design                  |
| Poor execution plan  | Analyze and tune the plan                          |

# Interview Tips

> "I start by identifying the specific slow query using application logs, Query Store, or monitoring tools. Next, I examine the execution plan to identify expensive operators such as table scans, key lookups, or costly joins. I verify that appropriate indexes exist and are being used, and I check whether statistics are current. I review the query for non-SARGable predicates, unnecessary `SELECT *` usage, and inefficient joins. If the plan looks reasonable, I investigate blocking, parameter sniffing, and server resource waits using SQL Server DMVs. Finally, I compare execution time, CPU usage, and logical reads before and after any changes to ensure the optimization delivers measurable improvements."
