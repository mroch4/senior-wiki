# `HAVING`

`HAVING` is used to **filter grouped/aggregated results**.

Think of it as:

- `WHERE` → filters **rows before grouping**
- `HAVING` → filters **groups after grouping**

## Example

Suppose you have:

```sql
Orders
-------
CustomerId
Amount
```

Find customers who have placed **more than 5 orders**:

```sql
SELECT CustomerId, COUNT(*) AS OrderCount
FROM Orders
GROUP BY CustomerId
HAVING COUNT(*) > 5;
```

The process is roughly:

1. `FROM` → get orders
2. `GROUP BY` → group orders by `CustomerId`
3. `COUNT(*)` → count orders in each group
4. `HAVING` → keep only groups where count > 5

### `WHERE` vs `HAVING`

```sql
SELECT CustomerId, COUNT(*) AS OrderCount
FROM Orders
WHERE Amount > 100
GROUP BY CustomerId
HAVING COUNT(*) > 5;
```

Here:

- `WHERE Amount > 100` → removes individual orders under 100 **before** grouping.
- `GROUP BY CustomerId` → groups the remaining orders.
- `HAVING COUNT(*) > 5` → keeps customers with more than 5 qualifying orders.

# Interview Tips

> Use `WHERE` to filter individual rows; use `HAVING` to filter aggregated groups.
