# JOINs

## Table of content

1. [What is JOIN?](#what-is-join)
2. [INNER JOIN](#1-inner-join)
3. [LEFT JOIN (LEFT OUTER JOIN)](#2-left-join-left-outer-join)
4. [RIGHT JOIN](#3-right-join)
5. [FULL OUTER JOIN](#4-full-outer-join)
6. [CROSS JOIN](#5-cross-join)
7. [SELF JOIN](#6-self-join)
8. [Finding Missing Data](#finding-missing-data)
9. [JOIN Order of Execution](#join-order-of-execution)
10. [JOIN vs WHERE](#join-vs-where)
11. [Performance Considerations](#performance-considerations)
12. [Quick Summary](#quick-summary)
13. [Interview Tips](#interview-tips)

## What is JOIN?

> SQL joins combine rows from two or more tables based on a related column (usually a foreign key).

Let's use two simple tables throughout the examples.

**Customers**

| CustomerId | Name    |
| ---------- | ------- |
| 1          | Alice   |
| 2          | Bob     |
| 3          | Charlie |

**Orders**

| OrderId | CustomerId | Product  |
| ------- | ---------- | -------- |
| 101     | 1          | Laptop   |
| 102     | 1          | Mouse    |
| 103     | 2          | Keyboard |
| 104     | 4          | Monitor  |

Notice that:

- Charlie has no orders.
- Order 104 references CustomerId 4, which doesn't exist (bad data or orphaned record).

## 1. INNER JOIN

Returns only matching rows from both tables.

```sql
SELECT
    c.Name,
    o.Product
FROM Customers c
INNER JOIN Orders o
    ON c.CustomerId = o.CustomerId;
```

Result:

| Name  | Product  |
| ----- | -------- |
| Alice | Laptop   |
| Alice | Mouse    |
| Bob   | Keyboard |

**Use when**

- You only care about records that exist in both tables.

Example:

- Orders with valid customers.
- Employees with departments.

## 2. LEFT JOIN (LEFT OUTER JOIN)

Returns:

- all rows from the left table
- matching rows from the right table
- NULL when no match exists

```sql
SELECT
    c.Name,
    o.Product
FROM Customers c
LEFT JOIN Orders o
    ON c.CustomerId = o.CustomerId;
```

Result:

| Name    | Product  |
| ------- | -------- |
| Alice   | Laptop   |
| Alice   | Mouse    |
| Bob     | Keyboard |
| Charlie | NULL     |

Charlie appears because the left table is preserved.

Common uses:

- Customers who may not have orders
- Products with optional reviews

## 3. RIGHT JOIN

Opposite of LEFT JOIN.

Returns:

- all rows from the right table
- matching rows from the left table

```sql
SELECT
    c.Name,
    o.Product
FROM Customers c
RIGHT JOIN Orders o
    ON c.CustomerId = o.CustomerId;
```

Result:

| Name  | Product  |
| ----- | -------- |
| Alice | Laptop   |
| Alice | Mouse    |
| Bob   | Keyboard |
| NULL  | Monitor  |

The monitor order has no matching customer.

> Many teams avoid `RIGHT JOIN` because you can usually rewrite it as a `LEFT JOIN` by swapping table order, making queries easier to read.

## 4. FULL OUTER JOIN

Returns:

- everything from both tables
- matching rows where possible
- NULL where no match exists

```sql
SELECT
    c.Name,
    o.Product
FROM Customers c
FULL OUTER JOIN Orders o
    ON c.CustomerId = o.CustomerId;
```

Result:

| Name    | Product  |
| ------- | -------- |
| Alice   | Laptop   |
| Alice   | Mouse    |
| Bob     | Keyboard |
| Charlie | NULL     |
| NULL    | Monitor  |

Useful for:

- Data reconciliation
- Comparing two datasets
- Finding missing relationships

## 5. CROSS JOIN

Produces every possible combination.

```sql
SELECT
    c.Name,
    o.Product
FROM Customers c
CROSS JOIN Orders o;
```

Result:

3 customers × 4 orders = **12 rows**

| Name  | Product  |
| ----- | -------- |
| Alice | Laptop   |
| Alice | Mouse    |
| Alice | Keyboard |
| Alice | Monitor  |
| Bob   | Laptop   |
| ...   | ...      |

Useful for:

- Generating combinations
- Test data
- Calendars

Be careful—it grows as **N × M**.

---

## 6. SELF JOIN

Join a table to itself.

Example Employee table:

| EmployeeId | Name  | ManagerId |
| ---------- | ----- | --------- |
| 1          | CEO   | NULL      |
| 2          | Alice | 1         |
| 3          | Bob   | 2         |

```sql
SELECT
    e.Name AS Employee,
    m.Name AS Manager
FROM Employees e
LEFT JOIN Employees m
    ON e.ManagerId = m.EmployeeId;
```

Result

| Employee | Manager |
| -------- | ------- |
| CEO      | NULL    |
| Alice    | CEO     |
| Bob      | Alice   |

Useful for:

- Organizational hierarchies
- Categories
- Parent-child relationships

## Finding Missing Data

This is a common interview question.

Customers with no orders:

```sql
SELECT c.*
FROM Customers c
LEFT JOIN Orders o
    ON c.CustomerId = o.CustomerId
WHERE o.CustomerId IS NULL;
```

Result: `Charlie`

Orders with invalid customers:

```sql
SELECT o.*
FROM Orders o
LEFT JOIN Customers c
    ON o.CustomerId = c.CustomerId
WHERE c.CustomerId IS NULL;
```

Result: `Order 104`

## JOIN Order of Execution

Logically, SQL processes a query in this order:

1. `FROM`
2. `JOIN`
3. `ON`
4. `WHERE`
5. `GROUP BY`
6. `HAVING`
7. `SELECT`
8. `ORDER BY`
9. `TOP` / `LIMIT`

Understanding this helps explain why filtering in `ON` versus `WHERE` can produce different results for outer joins.

## JOIN vs WHERE

With an `INNER JOIN`, these are effectively equivalent:

```sql
SELECT *
FROM Customers c
INNER JOIN Orders o
    ON c.CustomerId = o.CustomerId
WHERE o.Product = 'Laptop';
```

```sql
SELECT *
FROM Customers c
INNER JOIN Orders o
    ON c.CustomerId = o.CustomerId
   AND o.Product = 'Laptop';
```

For a `LEFT JOIN`, however, they differ.

**Filtering in the `WHERE` clause** removes rows where the right table is `NULL`, effectively turning the query into an `INNER JOIN`:

```sql
SELECT *
FROM Customers c
LEFT JOIN Orders o
    ON c.CustomerId = o.CustomerId
WHERE o.Product = 'Laptop';
```

Only customers with a laptop order are returned.

**Filtering in the `ON` clause** preserves all customers while only joining matching laptop orders:

```sql
SELECT *
FROM Customers c
LEFT JOIN Orders o
    ON c.CustomerId = o.CustomerId
   AND o.Product = 'Laptop';
```

Customers without laptop orders still appear with `NULL` values for the order columns.

## Performance Considerations

- Join on indexed columns whenever possible (typically primary keys and foreign keys).
- Keep join predicates sargable (avoid wrapping indexed columns in functions).
- Return only the columns you need instead of `SELECT *`.
- Join the smallest filtered datasets you can.
- Always review the execution plan for expensive joins, table scans, or key lookups.

## Quick Summary

| Join            | Returns                             |
| --------------- | ----------------------------------- |
| INNER JOIN      | Only matching rows                  |
| LEFT JOIN       | All left rows + matching right rows |
| RIGHT JOIN      | All right rows + matching left rows |
| FULL OUTER JOIN | All rows from both tables           |
| CROSS JOIN      | Every possible combination          |
| SELF JOIN       | A table joined to itself            |

# Interview Tips

- Be ready to explain the difference between `INNER`, `LEFT`, and `FULL OUTER JOIN` without relying on diagrams.
- A very common interview task is: "Find customers with no orders." The expected solution is a `LEFT JOIN` followed by `WHERE rightTable.Key IS NULL`.
- Know why putting a filter in the `WHERE` clause after a `LEFT JOIN` can unintentionally turn it into an `INNER JOIN`.
- Mention that joins perform best when the join columns (primary keys/foreign keys) are indexed.
- If asked about execution plans, explain that SQL Server chooses physical join operators such as **Nested Loops**, **Merge Join**, or **Hash Match** based on table sizes, indexes, and estimated row counts.
