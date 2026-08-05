# ## SARGable (Search ARGument Able) in SQL (Search ARGument Able) in SQL

## Table of content

1. [What is SARGable?](#what-is-sargable)
2. [Example of SARGable query](#example-of-sargable-query)
3. [Non-SARGable example](#non-sargable-example)
4. [Common things that make queries non-SARGable](#common-things-that-make-queries-non-sargable)
   - [Functions on columns](#functions-on-columns)
   - [Calculations on columns](#calculations-on-columns)
   - [Leading wildcard LIKE](#leading-wildcard-like)
   - [Implicit conversions](#implicit-conversions)
5. [SARGable vs non-SARGable comparison](#sargable-vs-non-sargable-comparison)
6. [Why it matters](#why-it-matters)
7. [How to find non-SARGable queries](#how-to-find-non-sargable-queries)
8. [Interview Tips](#interview-tips)

## What is SARGable?

> A **SARGable query** is a query where the database engine can use an **index efficiently to find rows**, instead of scanning the entire table.

The term comes from **SARG = Search ARGument**.

A predicate is SARGable when the column is compared directly to a value or parameter in a way that allows an **index seek**.

## Example of SARGable query

Assume we have:

```sql
CREATE INDEX IX_Customers_Name
ON Customers(Name);
```

Query:

```sql
SELECT *
FROM Customers
WHERE Name = 'John';
```

SQL Server can use:

```
Index Seek
```

It knows exactly where to look in the index.

## Non-SARGable example

```sql
SELECT *
FROM Customers
WHERE UPPER(Name) = 'JOHN';
```

The database must apply `UPPER()` to every row first:

```
Read row → convert Name → compare
```

It cannot efficiently navigate the index.

Execution plan:

```
Index Scan / Table Scan
```

## Common things that make queries non-SARGable

### 1. Functions on columns

❌ Non-SARGable:

```sql
WHERE YEAR(OrderDate) = 2026
```

The engine must calculate the year for every row.

✅ SARGable:

```sql
WHERE OrderDate >= '2026-01-01'
AND OrderDate < '2027-01-01'
```

### 2. Calculations on columns

❌

```sql
WHERE Price * 1.2 > 100
```

✅

```sql
WHERE Price > 83.33
```

### 3. Leading wildcard LIKE

❌

```sql
WHERE Name LIKE '%Smith'
```

The engine cannot use the beginning of the index.

Usually:

```
Index Scan
```

✅

```sql
WHERE Name LIKE 'Smith%'
```

Can use:

```
Index Seek
```

### 4. Implicit conversions

Example:

Column:

```sql
CustomerId VARCHAR(20)
```

Query:

```sql
WHERE CustomerId = 123
```

SQL Server converts every value:

```sql
CONVERT(CustomerId) = 123
```

Better:

```sql
WHERE CustomerId = '123'
```

## SARGable vs non-SARGable comparison

| Query                                              | SARGable? | Reason             |
| -------------------------------------------------- | --------- | ------------------ |
| `WHERE Id = 10`                                    | ✅        | Direct lookup      |
| `WHERE Id > 100`                                   | ✅        | Range seek         |
| `WHERE Name LIKE 'A%'`                             | ✅        | Uses index prefix  |
| `WHERE Name LIKE '%A'`                             | ❌        | Cannot seek        |
| `WHERE YEAR(Date)=2026`                            | ❌        | Function on column |
| `WHERE Date BETWEEN '2026-01-01' AND '2026-12-31'` | ✅        | Range search       |
| `WHERE LOWER(Name)='bob'`                          | ❌        | Function on column |

## Why it matters

With millions of rows:

### SARGable

```
Index
 |
Seek directly to matching rows
 |
Return data
```

Example:

```
10 rows read
```

### Non-SARGable

```
Table
 |
Read every row
 |
Apply function
 |
Filter
```

Example:

```
10,000,000 rows read
```

## How to find non-SARGable queries

Look at the execution plan:

Bad signs:

- Table Scan
- Index Scan
- Compute Scalar before filtering
- Warnings about implicit conversions

Good signs:

- Index Seek
- Key Lookup (sometimes acceptable)
- Range Seek

## Interview Tips

A strong interview answer:

> "SARGable means Search ARGument Able. A SARGable predicate allows SQL Server to use an index seek by comparing the indexed column directly to a value. Applying functions, calculations, implicit conversions, or leading wildcards to indexed columns often makes predicates non-SARGable, forcing scans and reducing performance."

A common follow-up question is: **"How would you rewrite `WHERE YEAR(OrderDate)=2026` to make it SARGable?"** — answer with a date range query.
