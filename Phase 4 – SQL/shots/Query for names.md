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

### Other

In **SQL Server `LIKE`**, the main wildcards are:

| Wildcard | Meaning                          | Example    | Matches                   |
| -------- | -------------------------------- | ---------- | ------------------------- |
| `%`      | Zero or more characters          | `'Jo%'`    | `Jo`, `John`, `Jonathan`  |
| `_`      | Exactly one character            | `'Jo_'`    | `Joe`, `Jon`              |
| `[ ]`    | One character from a set         | `'[JSA]%'` | `John`, `Sam`, `Alice`    |
| `[ - ]`  | One character within a range     | `'[A-Z]%'` | Names starting with A–Z   |
| `[^ ]`   | One character **not** in the set | `'[^J]%'`  | Names not starting with J |

### Useful examples

```sql
-- Starts with Jo
WHERE user_name LIKE 'Jo%'

-- Ends with son
WHERE user_name LIKE '%son'

-- Contains "oh"
WHERE user_name LIKE '%oh%'

-- Second letter is o
WHERE user_name LIKE '_o%'

-- Exactly 5 characters
WHERE user_name LIKE '_____'

-- Starts with J, S, or A
WHERE user_name LIKE '[JSA]%'

-- Starts with a letter from A to Z
WHERE user_name LIKE '[A-Z]%'

-- Does NOT start with J
WHERE user_name LIKE '[^J]%'
```

**Important:** `$`, `^`, `+`, `?`, `*`, etc. are **not** SQL Server `LIKE` wildcards. Those are commonly associated with **regular expressions**, which are a different pattern-matching system.
