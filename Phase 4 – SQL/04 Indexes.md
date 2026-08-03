# Indexes

## Table of content

1. [What is Index?](#what-is-index)
2. [Why indexes exist](#why-indexes-exist)
   - [Without an index](#without-an-index)
   - [With an index on `Email`](#with-an-index-on-email)
3. [What an index stores](#what-an-index-stores)
4. [Example](#example)
5. [Why indexes speed up queries](#why-indexes-speed-up-queries)
6. [What operations benefit from indexes?](#what-operations-benefit-from-indexes)
7. [Downsides of indexes](#downsides-of-indexes)
   - [Storage cost](#storage-cost)
8. [Primary key and indexes](#primary-key-and-indexes)
9. [When should you create an index?](#when-should-you-create-an-index)
10. [Interview Tips](#interview-tips)

## What is Index?

> A **SQL index** is a data structure that helps the database **find rows much faster** without scanning the entire table.

Think of it like the **index at the back of a book**:

- **Without an index**: You read every page until you find the topic.
- **With an index**: You look up the topic in the index, get the page number, and jump directly to it.

## Why indexes exist

Suppose you have a `Users` table with 10 million rows.

```sql
SELECT *
FROM Users
WHERE Email = 'john@example.com';
```

### Without an index

The database performs a **table scan**:

```
Row 1
Row 2
Row 3
...
Row 10,000,000
```

Time complexity is roughly **O(n)**.

### With an index on `Email`

```sql
CREATE INDEX IX_Users_Email
ON Users(Email);
```

The database navigates the index (typically a **B-tree**) to locate the matching row.

```
Root
 │
 ├── A-F
 ├── G-L
 │      └── J
 │          └── john@example.com
 └── M-Z
```

Instead of reading millions of rows, it reads only a few index pages.

Time complexity is approximately **O(log n)**.

## What an index stores

An index does **not** store the entire table.

It stores:

```
Indexed value
 |
john@example.com ---> Row ID 512
adam@test.com ------> Row ID 102
```

So the database:

1. Finds the indexed value.
2. Retrieves the corresponding row from the table.

## Example

Table:

| Id  | Name  | Email                                   |
| --- | ----- | --------------------------------------- |
| 1   | John  | [john@mail.com](mailto:john@mail.com)   |
| 2   | Alice | [alice@mail.com](mailto:alice@mail.com) |
| 3   | Bob   | [bob@mail.com](mailto:bob@mail.com)     |

Create index:

```sql
CREATE INDEX IX_Users_Email
ON Users(Email);
```

Internally (simplified):

```
alice@mail.com -> Row 2
bob@mail.com   -> Row 3
john@mail.com  -> Row 1
```

Searching by email becomes very fast.

## Why indexes speed up queries

Without an index:

```
Scan every row
```

With an index:

```
Search tree
 |
Find pointer
 |
Retrieve row
```

The difference becomes dramatic on large tables.

## What operations benefit from indexes?

Indexes help queries that use:

```sql
WHERE
JOIN
ORDER BY
GROUP BY
```

Examples:

```sql
SELECT *
FROM Orders
WHERE CustomerId = 25;
```

```sql
SELECT *
FROM Orders
ORDER BY OrderDate;
```

## Downsides of indexes

Indexes are **not free**.

Every time data changes, the index must also be updated.

```sql
INSERT
UPDATE
DELETE
```

becomes slightly slower because:

1. Write row
2. Update one or more indexes
3. Rebalance the B-tree if needed

More indexes = slower writes.

### Storage cost

Indexes occupy disk space. For large tables, indexes can consume gigabytes of storage.

## Primary key and indexes

A primary key is usually backed by an index automatically.

```sql
CREATE TABLE Users
(
    Id INT PRIMARY KEY
);
```

The database automatically creates an index on `Id`.

## When should you create an index?

Good candidates:

- Frequently searched columns
- Foreign keys
- Columns used in joins
- Columns used in sorting
- Columns with high selectivity (many distinct values), such as `Email`

Poor candidates:

- Columns with very few distinct values (e.g., `IsDeleted`, `Gender`), unless combined with other columns.
- Very small tables, where scanning is often faster than using an index.

# Interview Tips

> A SQL index is a separate data structure, usually implemented as a B-tree, that stores indexed column values together with references to the corresponding table rows. It allows the database engine to locate rows efficiently without scanning the entire table, reducing lookup time from roughly O(n) to O(log n). Indexes significantly improve read performance for queries using `WHERE`, `JOIN`, `ORDER BY`, and `GROUP BY`, but they increase storage usage and slow down `INSERT`, `UPDATE`, and `DELETE` operations because the index must also be maintained.

- Be ready to explain the **book index analogy**—it's a simple way to introduce the concept.
- Mention that **indexes optimize reads at the cost of slower writes and extra storage**; interviewers often ask about this trade-off.
- Know that **clustered** and **nonclustered** indexes are the two primary index types in relational databases. Since you've already studied them, they're the natural next topic after understanding what an index is.
