# Window Functions

## Table of content

1. [What is window function?](#what-is-window-function)
2. [Syntax](#syntax)
3. [Sample Data](#sample-data)
4. [ROW_NUMBER()](#1-row_number)
   - [Per department](#per-department)
5. [RANK()](#2-rank)
6. [DENSE_RANK()](#3-dense_rank)
7. [NTILE()](#4-ntile)
8. [SUM() OVER()](#5-sum-over)
9. [Running Total](#6-running-total)
10. [AVG() OVER()](#7-avg-over)
11. [LAG()](#8-lag)
12. [LEAD()](#9-lead)
13. [FIRST_VALUE()](#10-first_value)
14. [LAST_VALUE()](#11-last_value)
15. [Window Frame](#window-frame)
16. [GROUP BY vs Window Function](#group-by-vs-window-function)
17. [Common Interview Questions](#common-interview-questions)
    - [Top 3 highest-paid employees per department](#top-3-highest-paid-employees-per-department)
    - [Find employees earning above the department average](#find-employees-earning-above-the-department-average)
    - [Calculate month-over-month sales growth](#calculate-month-over-month-sales-growth)
18. [When are window functions most useful?](#when-are-window-functions-most-useful)
19. [Interview Tips](#interview-tips)

## What is window function?

SQL **window functions** perform calculations across a set of rows related to the current row **without collapsing the result into a single row** (unlike `GROUP BY`).

Think of it this way:

- `GROUP BY` → combines rows into one result per group.
- `WINDOW FUNCTION` → keeps every row but adds calculated information.

### Syntax

```sql
FUNCTION() OVER (
    PARTITION BY ...
    ORDER BY ...
)
```

- **PARTITION BY** – splits data into groups (similar to GROUP BY but doesn't collapse rows)
- **ORDER BY** – defines the order inside each partition
- Some functions also use **ROWS** or **RANGE** to define a window frame.

## Sample Data

| Employee | Department | Salary |
| -------- | ---------- | ------ |
| Alice    | IT         | 5000   |
| Bob      | IT         | 6000   |
| Charlie  | IT         | 7000   |
| Dave     | HR         | 4000   |
| Eve      | HR         | 4500   |

## 1. ROW_NUMBER()

Assigns a unique sequential number.

```sql
SELECT
    Employee,
    Salary,
    ROW_NUMBER() OVER(ORDER BY Salary DESC) AS RowNum
FROM Employees;
```

Result

| Employee | Salary | RowNum |
| -------- | ------ | ------ |
| Charlie  | 7000   | 1      |
| Bob      | 6000   | 2      |
| Alice    | 5000   | 3      |
| Eve      | 4500   | 4      |
| Dave     | 4000   | 5      |

### Per department

```sql
SELECT
    Employee,
    Department,
    Salary,
    ROW_NUMBER() OVER(
        PARTITION BY Department
        ORDER BY Salary DESC
    ) AS RankInDepartment
FROM Employees;
```

Result

| Employee | Department | Rank |
| -------- | ---------- | ---- |
| Charlie  | IT         | 1    |
| Bob      | IT         | 2    |
| Alice    | IT         | 3    |
| Eve      | HR         | 1    |
| Dave     | HR         | 2    |

## 2. RANK()

Handles ties by leaving gaps.

```sql
Salary
7000
6000
6000
5000
```

Produces

| Salary | Rank |
| ------ | ---- |
| 7000   | 1    |
| 6000   | 2    |
| 6000   | 2    |
| 5000   | 4    |

Notice there is **no rank 3**.

## 3. DENSE_RANK()

Similar to `RANK()` but **no gaps**.

| Salary | DenseRank |
| ------ | --------- |
| 7000   | 1         |
| 6000   | 2         |
| 6000   | 2         |
| 5000   | 3         |

## 4. NTILE()

Splits rows into buckets.

```sql
SELECT
    Employee,
    Salary,
    NTILE(4) OVER(ORDER BY Salary DESC) AS Quartile
FROM Employees;
```

Useful for:

- Quartiles
- Top 10%
- Performance bands

## 5. SUM() OVER()

Running totals or totals per partition.

Department total salary:

```sql
SELECT
    Employee,
    Department,
    Salary,
    SUM(Salary) OVER(PARTITION BY Department) AS DepartmentTotal
FROM Employees;
```

Result

| Employee | Salary | DepartmentTotal |
| -------- | ------ | --------------- |
| Alice    | 5000   | 18000           |
| Bob      | 6000   | 18000           |
| Charlie  | 7000   | 18000           |
| Dave     | 4000   | 8500            |
| Eve      | 4500   | 8500            |

Notice every employee still appears.

## 6. Running Total

```sql
SELECT
    Employee,
    Salary,
    SUM(Salary) OVER(
        ORDER BY Salary
    ) AS RunningTotal
FROM Employees;
```

Result

| Salary | Running Total |
| ------ | ------------- |
| 4000   | 4000          |
| 4500   | 8500          |
| 5000   | 13500         |
| 6000   | 19500         |
| 7000   | 26500         |

## 7. AVG() OVER()

Average salary by department.

```sql
SELECT
    Employee,
    Salary,
    AVG(Salary) OVER(PARTITION BY Department) AS AvgSalary
FROM Employees;
```

## 8. LAG()

Looks at the previous row.

```sql
SELECT
    Employee,
    Salary,
    LAG(Salary) OVER(ORDER BY Salary) AS PreviousSalary
FROM Employees;
```

Result

| Salary | Previous |
| ------ | -------- |
| 4000   | NULL     |
| 4500   | 4000     |
| 5000   | 4500     |
| 6000   | 5000     |
| 7000   | 6000     |

Useful for:

- Comparing with previous month
- Detecting changes
- Time-series analysis

## 9. LEAD()

Looks ahead.

```sql
SELECT
    Employee,
    Salary,
    LEAD(Salary) OVER(ORDER BY Salary) AS NextSalary
FROM Employees;
```

## 10. FIRST_VALUE()

Returns the first value in the window.

```sql
SELECT
    Employee,
    Salary,
    FIRST_VALUE(Salary) OVER(
        ORDER BY Salary DESC
    ) AS HighestSalary
FROM Employees;
```

Every row gets `7000`.

## 11. LAST_VALUE()

Gets the last value **within the current window frame**.

Without specifying the frame, it often returns the current row because the default frame ends at the current row.

To get the true last value:

```sql
LAST_VALUE(Salary) OVER(
    ORDER BY Salary
    ROWS BETWEEN UNBOUNDED PRECEDING
         AND UNBOUNDED FOLLOWING
)
```

This is a common interview gotcha.

## Window Frame

By default:

```sql
ORDER BY Salary
```

usually means

```sql
ROWS BETWEEN UNBOUNDED PRECEDING
     AND CURRENT ROW
```

You can change the frame.

Current row + previous row:

```sql
ROWS BETWEEN 1 PRECEDING
     AND CURRENT ROW
```

Current row + next two:

```sql
ROWS BETWEEN CURRENT ROW
     AND 2 FOLLOWING
```

Entire partition:

```sql
ROWS BETWEEN UNBOUNDED PRECEDING
     AND UNBOUNDED FOLLOWING
```

## GROUP BY vs Window Function

Using `GROUP BY`:

```sql
SELECT Department,
       SUM(Salary)
FROM Employees
GROUP BY Department;
```

Returns:

| Department | Total |
| ---------- | ----- |
| IT         | 18000 |
| HR         | 8500  |

Only one row per department.

Window function:

```sql
SELECT
    Employee,
    Department,
    Salary,
    SUM(Salary) OVER(PARTITION BY Department)
FROM Employees;
```

Returns every employee plus the department total.

## Common Interview Questions

### Top 3 highest-paid employees per department

```sql
WITH RankedEmployees AS
(
    SELECT *,
           ROW_NUMBER() OVER(
               PARTITION BY Department
               ORDER BY Salary DESC
           ) AS rn
    FROM Employees
)
SELECT *
FROM RankedEmployees
WHERE rn <= 3;
```

### Find employees earning above the department average

```sql
WITH EmployeeStats AS
(
    SELECT *,
           AVG(Salary) OVER(PARTITION BY Department) AS AvgSalary
    FROM Employees
)
SELECT *
FROM EmployeeStats
WHERE Salary > AvgSalary;
```

### Calculate month-over-month sales growth

```sql
SELECT
    Month,
    Sales,
    Sales - LAG(Sales) OVER(ORDER BY Month) AS Growth
FROM Sales;
```

## When are window functions most useful?

Use them when you need row-level detail **and** aggregate or ranking information at the same time, such as:

- Ranking employees or products.
- Running totals and moving averages.
- Comparing a row with the previous or next row.
- Finding the top N records within each category.
- Calculating percentages of totals without losing individual rows.

# Interview Tips

- Be ready to explain that **window functions do not reduce the number of rows**, unlike `GROUP BY`.
- Clearly distinguish **`ROW_NUMBER()`**, **`RANK()`**, and **`DENSE_RANK()`**, especially how they handle ties.
- Remember that **`PARTITION BY` creates logical groups**, while **`ORDER BY` determines the sequence within those groups**.
- A common SQL Server interview question is why `LAST_VALUE()` doesn't return the last row by default—the answer is the default window frame ends at the current row unless you explicitly extend it with `ROWS BETWEEN UNBOUNDED PRECEDING AND UNBOUNDED FOLLOWING`.
- Practice combining CTEs with window functions, as many real-world interview problems (Top N per group, duplicate detection, running totals) are solved this way.
