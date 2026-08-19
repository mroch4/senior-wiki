If you want names where the **second letter is `O`**, use `LIKE` with `_`:

```sql
SELECT user_name
FROM users
WHERE user_name LIKE '_O%';
```

### How it works

- `_` = exactly **one character**
- `O` = second character must be **O**
- `%` = any number of characters after that

So it matches:

- `John` ✅
- `Robert` ✅
- `Tom` ❌ (second letter is `O`? Actually `o` is second, so **Tom** ✅)
- `Alice` ❌
- `Oscar` ❌ (second letter is `s`)

If you're using **SQL Server**, `LIKE '_O%'` is the standard way to do this.
