## Table of content

1. [Which columns are indexed?](#1-which-columns-are-indexed)
2. [Can I see the actual indexed values?](#2-can-i-see-the-actual-indexed-values)
3. [View index information in SQL Server Management Studio (SSMS)](#3-view-index-information-in-sql-server-management-studio-ssms)
4. [View index usage statistics](#4-view-index-usage-statistics)
5. [See whether a query uses an index](#5-see-whether-a-query-uses-an-index)
6. [Interview Tips](#interview-tips)

## 1. Which columns are indexed?

You can query **SQL Server system views**:

```sql
SELECT
    i.name AS IndexName,
    i.type_desc,
    c.name AS ColumnName
FROM sys.indexes i
JOIN sys.index_columns ic
    ON i.object_id = ic.object_id
   AND i.index_id = ic.index_id
JOIN sys.columns c
    ON c.object_id = ic.object_id
   AND c.column_id = ic.column_id
WHERE i.object_id = OBJECT_ID('dbo.Employees')
ORDER BY i.name, ic.key_ordinal;
```

Example output:

| Index        | Type         | Column |
| ------------ | ------------ | ------ |
| PK_Employees | CLUSTERED    | Id     |
| IX_Name      | NONCLUSTERED | Name   |
| IX_Email     | NONCLUSTERED | Email  |

This tells you **which columns are indexed**, not the index contents.

## 2. Can I see the actual indexed values?

Yes, but not directly in a simple table. The values are stored in the B-tree structure.

For example:

```sql
CREATE INDEX IX_Name
ON Employees(Name);
```

Internally, SQL Server stores something conceptually like:

| Name    | Row Locator |
| ------- | ----------- |
| Alice   | Id = 1      |
| Bob     | Id = 2      |
| Charlie | Id = 3      |

You normally **cannot query this structure directly** with standard SQL.

## 3. View index information in SQL Server Management Studio (SSMS)

Expand:

```text
Database
 └── Tables
      └── Employees
            └── Indexes
```

You'll see entries such as:

```text
PK_Employees (Clustered)
IX_Name (Nonclustered)
IX_Email (Nonclustered)
```

Opening an index's properties shows:

- Indexed columns
- Included columns
- Whether it is unique
- Whether it is clustered

## 4. View index usage statistics

To see whether SQL Server is actually using an index:

```sql
SELECT
    OBJECT_NAME(s.object_id) AS TableName,
    i.name AS IndexName,
    s.user_seeks,
    s.user_scans,
    s.user_lookups,
    s.user_updates
FROM sys.dm_db_index_usage_stats s
JOIN sys.indexes i
    ON s.object_id = i.object_id
   AND s.index_id = i.index_id
WHERE OBJECT_NAME(s.object_id) = 'Employees';
```

This shows how often each index has been:

- **Seeked** (efficient lookups)
- **Scanned**
- **Used for lookups**
- **Updated**

## 5. See whether a query uses an index

A very common way to check is by viewing the **execution plan**.

In SSMS:

1. Click **Include Actual Execution Plan** (or press **Ctrl+M**).
2. Run your query.
3. Inspect the execution plan.

You might see operators such as:

- **Clustered Index Seek** ✅ (efficient)
- **Nonclustered Index Seek** ✅
- **Clustered Index Scan** ⚠️
- **Table Scan** ❌ (usually indicates no useful index for that query)

This is often the fastest way to verify that SQL Server is using the index you expect.

# Interview Tips

- Be clear about the distinction between **index metadata** (which columns are indexed) and the **index contents** (the internal B-tree, which isn't typically queried directly).
- Know the key catalog views:

  - `sys.indexes` → index definitions.
  - `sys.index_columns` → columns in each index.
  - `sys.dm_db_index_usage_stats` → how indexes are being used.

- When troubleshooting performance, **execution plans** are the primary tool for confirming whether SQL Server is performing an **Index Seek**, **Index Scan**, or **Table Scan**.
