# Primary vs Unique Keys

The main difference is **purpose and NULL handling**.

|                                  | **Primary Key**         | **Unique Key**                                 |
| -------------------------------- | ----------------------- | ---------------------------------------------- |
| Purpose                          | **Identifies** each row | Ensures values are unique                      |
| Duplicate values                 | Not allowed             | Not allowed                                    |
| `NULL` values                    | Not allowed             | Usually allowed (SQL Server allows one `NULL`) |
| How many per table?              | **Only 1**              | **Multiple**                                   |
| Automatically creates an index   | Yes                     | Yes                                            |
| Can be referenced by Foreign Key | Yes                     | Yes, if it meets the required uniqueness       |

## Example

```sql
CREATE TABLE Users (
    UserID INT PRIMARY KEY,
    Email VARCHAR(255) UNIQUE,
    Username VARCHAR(100) UNIQUE
);
```

Here:

- `UserID` → **Primary Key**: uniquely identifies the user.
- `Email` → **Unique Key**: prevents two users from having the same email.
- `Username` → **Unique Key**: prevents duplicate usernames.

So think of it as:

> Primary Key = the main **identity** of the row.

> Unique Key = another column that **must also contain unique** values.

# Interview Tips

A table can have **only one Primary Key but multiple Unique Keys**.

Also, a Primary Key is **always NOT NULL**, whereas a Unique constraint in SQL Server can allow **one NULL**.

## One NULL

In **SQL Server**, a `UNIQUE` constraint means:

- One row can have `NULL`
- Other rows can have actual unique values
- A second `NULL` is **not allowed**

Example:

```sql
CREATE TABLE Users (
    UserID INT PRIMARY KEY,
    Email VARCHAR(100) UNIQUE
);
```

This is valid:

| UserID | Email                                   |
| ------ | --------------------------------------- |
| 1      | [john@email.com](mailto:john@email.com) |
| 2      | [anna@email.com](mailto:anna@email.com) |
| 3      | NULL                                    |

But this is **not valid** in SQL Server:

```text
UserID | Email
1      | john@email.com
2      | NULL
3      | NULL  ← Error: duplicate NULL
```

So:

> **`PRIMARY KEY` → no `NULL`s at all** > **`UNIQUE` → one `NULL` allowed in SQL Server**

Small note: this behavior can differ between database systems.
