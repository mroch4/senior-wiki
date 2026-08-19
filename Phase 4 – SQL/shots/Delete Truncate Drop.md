# DELETE, TRUNCATE and DROP

| Command    | Removes rows         | Removes table | `WHERE` allowed | Can rollback?\*                            | Typical use                 |
| ---------- | -------------------- | ------------- | --------------- | ------------------------------------------ | --------------------------- |
| `DELETE`   | ✅ Selected/all rows | ❌            | ✅              | ✅                                         | Remove specific records     |
| `TRUNCATE` | ✅ All rows          | ❌            | ❌              | ✅                                         | Quickly empty a table       |
| `DROP`     | ✅ All rows          | ✅            | ❌              | Usually ✅ in a transaction (DB-dependent) | Remove the table completely |

\*Transaction/rollback behavior has database-specific details; the above is particularly applicable to SQL Server.

## 1. DELETE

Removes rows from a table.

```sql
DELETE FROM Employees
WHERE EmployeeId = 10;
```

You can also delete everything:

```sql
DELETE FROM Employees;
```

The **table structure remains**.

Useful when you need a `WHERE` condition.

## 2. TRUNCATE

Removes **all rows** very quickly:

```sql
TRUNCATE TABLE Employees;
```

The table itself remains, including its columns and indexes.

But you **cannot** do:

```sql
TRUNCATE TABLE Employees
WHERE EmployeeId = 10; -- ❌
```

A key SQL Server difference: `TRUNCATE` deallocates data pages rather than deleting rows one by one, so it generally generates much less transaction-log activity than `DELETE`.

Also, in SQL Server, `TRUNCATE` resets an `IDENTITY` counter back to its seed.

## 3. DROP

Removes the **entire table**:

```sql
DROP TABLE Employees;
```

After this:

```sql
SELECT * FROM Employees;
```

will fail because the table no longer exists.

It removes the table definition, data, indexes, constraints, etc.

## Easy way to remember

Think of a house:

- **DELETE** → remove some/all furniture 🪑
- **TRUNCATE** → empty the entire house 🏠
- **DROP** → demolish the house 💥

# Interview Tips

I want to remove all records but keep the table. Which would you use?

> Usually:

**`TRUNCATE TABLE`** if you don't need a `WHERE` clause and the table's constraints/usage allow it.

**`DELETE`** if you need selective deletion or row-by-row deletion semantics.
