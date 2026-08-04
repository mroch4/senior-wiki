# CTE

## Table of content

1. [What is CTE?](#what-is-cte)
   - [Basic Syntax](#basic-syntax)
2. [Why use a CTE?](#why-use-a-cte)
   - [Improve readability](#improve-readability)
   - [Break a problem into steps](#break-a-problem-into-steps)
   - [Reuse a calculated result](#reuse-a-calculated-result)
3. [Recursive CTE](#recursive-cte)
4. [CTE vs Subquery](#cte-vs-subquery)
5. [CTE vs Temporary Table](#cte-vs-temporary-table)
6. [Interview Tips](#interview-tips)
   - [SQL Server Interview Tips](#sql-server-interview-tips)
   - [Is a CTE stored in memory?](#is-a-cte-stored-in-memory)
   - [Does a CTE improve performance?](#does-a-cte-improve-performance)
   - [Can multiple CTEs be declared?](#can-multiple-ctes-be-declared)

## What is CTE?

> A **CTE (Common Table Expression)** is a **temporary named result set** that exists only for the duration of a single SQL statement. It is defined using the `WITH` keyword and is often used to make complex queries easier to read and maintain.

### Basic Syntax

```sql
WITH EmployeeCTE AS
(
    SELECT EmployeeId, Name, Salary
    FROM Employees
    WHERE Salary > 50000
)
SELECT *
FROM EmployeeCTE;
```

Think of it as:

- Write a query.
- Give it a name.
- Use it like a table in the main query.

## Why use a CTE?

### 1. Improve readability

Instead of nesting subqueries:

```sql
SELECT *
FROM (
    SELECT DepartmentId, AVG(Salary) AS AvgSalary
    FROM Employees
    GROUP BY DepartmentId
) d
WHERE AvgSalary > 60000;
```

Use a CTE:

```sql
WITH DepartmentSalaries AS
(
    SELECT DepartmentId,
           AVG(Salary) AS AvgSalary
    FROM Employees
    GROUP BY DepartmentId
)
SELECT *
FROM DepartmentSalaries
WHERE AvgSalary > 60000;
```

### 2. Break a problem into steps

Example: Find departments whose average salary is above the company average.

```sql
WITH DepartmentAverage AS
(
    SELECT DepartmentId,
           AVG(Salary) AS AvgSalary
    FROM Employees
    GROUP BY DepartmentId
),
CompanyAverage AS
(
    SELECT AVG(Salary) AS AvgSalary
    FROM Employees
)
SELECT d.DepartmentId,
       d.AvgSalary
FROM DepartmentAverage d
CROSS JOIN CompanyAverage c
WHERE d.AvgSalary > c.AvgSalary;
```

Notice one CTE can reference another.

### 3. Reuse a calculated result

```sql
WITH HighEarners AS
(
    SELECT *
    FROM Employees
    WHERE Salary > 100000
)
SELECT COUNT(*)
FROM HighEarners;

-- Same CTE can be referenced again in the same statement
SELECT AVG(Salary)
FROM HighEarners;
```

## Recursive CTE

One of the biggest advantages of CTEs is recursion.

Example: Employee hierarchy.

```
CEO
 ├── Manager A
 │      ├── Dev1
 │      └── Dev2
 └── Manager B
        └── Dev3
```

```sql
WITH EmployeeHierarchy AS
(
    -- Anchor member
    SELECT EmployeeId,
           ManagerId,
           Name
    FROM Employees
    WHERE ManagerId IS NULL

    UNION ALL

    -- Recursive member
    SELECT e.EmployeeId,
           e.ManagerId,
           e.Name
    FROM Employees e
    INNER JOIN EmployeeHierarchy h
        ON e.ManagerId = h.EmployeeId
)
SELECT *
FROM EmployeeHierarchy;
```

Recursive CTEs are commonly used for:

- Organization charts
- Folder structures
- Bill of materials
- Category trees
- Graph traversal

## CTE vs Subquery

| CTE                     | Subquery                        |
| ----------------------- | ------------------------------- |
| Named                   | Anonymous                       |
| More readable           | Can become deeply nested        |
| Can chain multiple CTEs | Harder to organize              |
| Supports recursion      | Does not                        |
| Good for complex logic  | Good for simple one-off queries |

## CTE vs Temporary Table

| CTE                      | Temp Table                                                               |
| ------------------------ | ------------------------------------------------------------------------ |
| Exists for one statement | Exists until dropped or session ends                                     |
| No indexes               | Can have indexes                                                         |
| Cannot be modified       | Can insert/update/delete                                                 |
| Great for readability    | Better for large intermediate datasets reused across multiple statements |

# Interview Tips

- Define a CTE as a **temporary named result set** created with `WITH` that exists only for a single SQL statement.
- Explain that its primary benefits are **readability, modularity, and support for recursive queries**.
- Be ready to compare **CTEs vs subqueries** and **CTEs vs temporary tables**, including when each is the better choice.
- Mention that CTEs **do not inherently improve performance**; always verify with the execution plan.

## SQL Server Interview Tips

Is a CTE stored in memory?

> No. A CTE is **not** a physical table. It is a logical query expression that the optimizer incorporates into the overall execution plan. Whether intermediate results are materialized depends on the optimizer and query plan, not on the fact that you used a CTE. ([SQL Practice Online][2])

Does a CTE improve performance?

> Not necessarily.

CTEs are primarily for:

- readability
- maintainability
- simplifying complex SQL

Performance depends on indexes, joins, predicates, statistics, and the execution plan—not simply on using a CTE. ([Baeldung on Kotlin][1])

Can multiple CTEs be declared?

> Yes.

```sql
WITH A AS
(
    SELECT ...
),
B AS
(
    SELECT ...
    FROM A
),
C AS
(
    SELECT ...
    FROM B
)
SELECT *
FROM C;
```

This style is common in analytical SQL and ETL queries.
