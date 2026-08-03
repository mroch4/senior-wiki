# Clustered vs nonclustered index

## Table of content

1. [What is clustered index?](#what-is-clustered-index)
   - [Simple analogy](#simple-analogy)
2. [Example](#example)
3. [Internal structure](#internal-structure)
4. [Range query example](#range-query-example)
5. [Why only one?](#why-only-one)
6. [Clustered Index Seek vs Scan](#clustered-index-seek-vs-scan)
   - [Index Seek](#index-seek)
   - [Index Scan](#index-scan)
7. [Choosing a good clustered index](#choosing-a-good-clustered-index)
8. [Key Lookup](#key-lookup)
9. [Clustered vs Nonclustered](#clustered-vs-nonclustered)
10. [Clustered index](#clustered-index)
11. [Nonclustered index](#nonclustered-index)
12. [When to choose each](#when-to-choose-each)
    - [Clustered index](#clustered-index-1)
    - [Nonclustered index](#nonclustered-index-1)
13. [Insert](#insert)
    - [Clustered index](#clustered-index-2)
    - [Nonclustered index](#nonclustered-index-2)
14. [Impact on nonclustered indexes](#impact-on-nonclustered-indexes)
    - [Nonclustered index on Name](#nonclustered-index-on-name)
    - [Covering index](#covering-index)
15. [Which queries benefit?](#which-queries-benefit)
    - [Clustered index](#clustered-index-3)
    - [Nonclustered index](#nonclustered-index-3)
16. [Interview Tips](#interview-tips)

## What is clustered index?

> A clustered index stores the actual table data in the order of the index key. Because the data can only be stored in one physical order, a table can have only one clustered index.

A **clustered index** determines the **physical order of rows in a table**. Think of it as the table itself being sorted by the indexed column(s).

### Simple analogy

Imagine a dictionary.

- The words are stored alphabetically.
- Finding "Apple" is fast because the pages are already ordered.
- You can't have the same dictionary physically ordered both alphabetically and by word length.

A clustered index works the same way.

## Example

```sql
CREATE TABLE Employees
(
    Id INT PRIMARY KEY,
    Name NVARCHAR(100),
    Department NVARCHAR(50)
);
```

In SQL Server:

```sql
PRIMARY KEY
```

creates a **clustered index by default** (unless specified otherwise).

The rows are physically stored:

```
1 John
2 Alice
3 Bob
4 David
```

Searching for

```sql
WHERE Id = 3
```

requires very little work because SQL Server navigates directly through the B-tree to the row.

## Internal structure

A clustered index is implemented as a **B-tree**.

```
Root
 |
 ├── Branch ── Leaf
 ├── Branch
        ├───── Leaf
        ├───── Leaf
```

For a clustered index:

**Leaf pages contain the actual data rows.**

This is an important interview point.

## Range query example

```sql
SELECT *
FROM Orders
WHERE OrderDate
BETWEEN '2025-01-01'
AND '2025-01-31';
```

If `OrderDate` is the clustered index:

```
Jan 1
Jan 2
Jan 3
...
Jan 31
```

The database reads one continuous set of pages.

Very efficient.

Without a clustered index, rows might be scattered across many pages.

## Why only one?

Because the rows can only be physically arranged one way.

Example: Can a bookshelf be simultaneously ordered by:

- author
- publication year
- title

No. Same principle.

## Clustered Index Seek vs Scan

### Index Seek

```sql
WHERE Id = 150
```

SQL Server navigates directly to the row.

Complexity approximately: `O(log n)`

### Index Scan

```sql
WHERE Salary > 0
```

If no suitable index exists:

```
Read page 1
Read page 2
Read page 3
...
```

This is much slower.

## Choosing a good clustered index

Ideal characteristics:

- Unique (or nearly unique)
- Narrow (small data type)
- Immutable (rarely updated)
- Ever-increasing values
- Frequently used for joins and searches

✅ Excellent candidates:

```
INT IDENTITY
BIGINT IDENTITY
GUID using NEWSEQUENTIALID()
```

❌ Poor candidates:

```
Name
Email
Status
Country
```

because they change more often, have duplicates, or are wider.

## Key Lookup

Suppose:

```sql
CREATE NONCLUSTERED INDEX IX_Name
ON Employees(Name);
```

Then execute:

```sql
SELECT Salary
FROM Employees
WHERE Name = 'Alice';
```

The nonclustered index contains:

```
Name
Clustered Key
```

It does **not** contain `Salary`.

SQL Server:

1. Finds "Alice" in `IX_Name`.
2. Reads the clustered key.
3. Performs a **Key Lookup** into the clustered index to retrieve `Salary`.

If many rows require lookups, this can become expensive. One optimization is a **covering index**, which includes the needed columns:

```sql
CREATE NONCLUSTERED INDEX IX_Name
ON Employees(Name)
INCLUDE (Salary);
```

Now the query can be satisfied entirely from the nonclustered index, avoiding the lookup.

## Clustered vs Nonclustered

> A clustered index stores the table data itself in sorted order, while a nonclustered index is a **separate structure** that points to the data.

| Feature                  | Clustered Index                    | Nonclustered Index                                       |
| ------------------------ | ---------------------------------- | -------------------------------------------------------- |
| Physical data order      | **Yes**                            | **No** (separate structure)                              |
| Stores actual table rows | **Yes (leaf level contains data)** | **No (leaf level contains key + row locator)**           |
| Number allowed           | **1**                              | Many (up to SQL Server limits)                           |
| Best for                 | Range queries                      | Point lookups, filtering, sorting                        |
| Size                     | Larger                             | Smaller                                                  |
| Insert performance       | Slower due to maintaining order    | Usually faster, but each index adds maintenance overhead |

### Clustered index

```

Id   Name
-------------
1    Alice
2    Bob
3    Charlie
4    David
```

The table itself is stored in `Id` order.

```
Root
 |
Branch
 |
Leaf
 |
Actual table rows
```

The leaf nodes **are the table**.

### Nonclustered index

Suppose we create:

```sql
CREATE INDEX IX_Employee_Name
ON Employees(Name);
```

SQL Server creates a separate B-tree.

```
Root
 |
Branch
 |
Leaf

Alice  -> Id = 1
Bob    -> Id = 2
Charlie-> Id = 3
```

The leaf pages contain:

- indexed column (`Name`)
- pointer to the actual row

The actual table remains unchanged.

## When to choose each

### Clustered index

Choose columns that are:

- Primary key
- Unique
- Narrow
- Never (or rarely) updated
- Increasing (e.g., `IDENTITY`, timestamp)

Examples:

```
OrderId
CustomerId
InvoiceId
```

### Nonclustered index

Choose columns that are:

- Frequently searched
- Frequently joined
- Frequently sorted
- Frequently filtered

Examples:

```
Email
Username
OrderDate
Status
LastName
```

## Insert

### Clustered index

Suppose rows are ordered by `Id`.

Insert:

```
1
2
3
4
```

Insert `5`:

```
1
2
3
4
5
```

Easy—append to the end.

But if clustered on `LastName`:

```
Brown
Clark
Smith
```

Insert `Adams`:

```
Adams
Brown
Clark
Smith
```

SQL Server may need to move rows, split pages, and update index pointers, making inserts more expensive.

### Nonclustered index

Every insert also updates each nonclustered index.

For example, if a table has:

- 1 clustered index
- 8 nonclustered indexes

Every insert must update **9 index structures**.

This speeds up reads but slows writes.

## Impact on nonclustered indexes

This is a favorite senior interview question.

Every nonclustered index stores a **row locator**.

- If the table has a clustered index, the row locator is the **clustered index key**.
- If the table is a heap (no clustered index), the row locator is a **RID (Row Identifier)** consisting of File ID, Page ID, and Slot ID.

For example:

```
Nonclustered Index

Name

Alice -> Id=12
Bob   -> Id=37
John  -> Id=81
```

To find "Alice":

1. Search the nonclustered index.
2. Retrieve the clustered key (`Id = 12`).
3. Traverse the clustered index to fetch the full row.

This second step is called a **Key Lookup**.

### Nonclustered index on Name

```sql
SELECT Salary
FROM Employees
WHERE Name = 'Bob';
```

Execution:

```
Search IX_Name
 |
Find clustered key (Id = 2)
 |
Go to clustered index
 |
Read Salary
```

This extra step is a **Key Lookup**.

### Covering index

Suppose this query is frequent:

```sql
SELECT Salary
FROM Employees
WHERE Name = 'Bob';
```

Instead of performing a Key Lookup every time:

```sql
CREATE INDEX IX_Name
ON Employees(Name)
INCLUDE (Salary);
```

Now the index contains:

```
Name
Salary
```

SQL Server answers the query using only the nonclustered index (no Key Lookup).

## Which queries benefit?

### Clustered index

Excellent for:

```sql
WHERE Id = 100
```

```sql
WHERE OrderDate BETWEEN ...
```

```sql
ORDER BY Id
```

```sql
TOP (100)
```

because rows are already physically ordered.

### Nonclustered index

Excellent for:

```sql
WHERE Email = ...
```

```sql
WHERE LastName = ...
```

```sql
WHERE Status = 'Active'
```

especially when only a small percentage of rows match.

# Interview Tips

> A clustered index is more than a search structure—it defines how table data is physically organized. Understanding its relationship with B-trees, nonclustered indexes, key lookups, page splits, and covering indexes is essential for diagnosing SQL Server performance issues and designing efficient schemas.

- Remember the core distinction: **clustered index = data**, **nonclustered index = pointers to data**.
- Know that **leaf nodes differ**:

  - Clustered index → actual data rows.
  - Nonclustered index → index key plus a row locator (clustered key or RID).

- Be ready to explain **Key Lookups** and how **covering indexes (`INCLUDE`)** eliminate them.
- Mention the trade-off: **indexes improve read performance but increase the cost of inserts, updates, and deletes**.
- A well-chosen clustered index is typically **unique, narrow, stable, and ever-increasing** (e.g., an `IDENTITY` column).

Can a table have no clustered index?

> Yes. Such a table is called a **heap**. Rows are stored without a defined physical order.

Does a clustered index always improve performance?

> No. It speeds up many reads—especially range queries—but inserts and updates may become more expensive because maintaining the sorted order can cause page splits and additional I/O.

Why is an `IDENTITY` column a common clustered index?

> Because values are increasing, new rows are typically appended at the end of the index, minimizing page splits and fragmentation.

When would you avoid clustering on a GUID?

> Random GUIDs (`NEWID()`) cause inserts throughout the index, leading to page splits and fragmentation. If a GUID is required, `NEWSEQUENTIALID()` is often a better choice.

Why can there only be one clustered index?

> Because data rows can only be stored in **one physical order**.

Can a table have only nonclustered indexes?

> Yes. Such a table is called a **heap**. The rows have no defined physical order, and nonclustered indexes use a **RID (Row Identifier)** to locate them.

Can a clustered index be non-unique?

> Yes. SQL Server automatically adds a hidden **uniqueifier** to distinguish duplicate key values.

Does every primary key create a clustered index?

> No. In SQL Server, a primary key is **clustered by default**, but you can explicitly create it as a **nonclustered** primary key if another column is a better choice for clustering.
