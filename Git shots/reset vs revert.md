# `reset` vs `revert`

If you mean **Git `reset` vs `revert`**, the key difference is:

|                          | `git reset`                         | `git revert`                             |
| ------------------------ | ----------------------------------- | ---------------------------------------- |
| What it does             | Moves the branch pointer            | Creates a new commit that undoes changes |
| Existing commits         | Can remove them from branch history | Preserves them                           |
| History                  | Rewrites history                    | Does not rewrite history                 |
| Safe on shared branches? | ❌ Usually no                       | ✅ Yes                                   |
| Typical use              | Local/private work                  | Already-pushed/shared work               |

### `git reset`

Suppose:

```
A -- B -- C -- D   main
```

You want to go back to `B`:

```bash
git reset --hard B
```

Now:

```
A -- B   main

      C -- D   (no longer on main)
```

`C` and `D` are removed from the branch's history.

Common modes:

```bash
git reset --soft HEAD~1
git reset --mixed HEAD~1
git reset --hard HEAD~1
```

- **soft** → move HEAD, keep changes staged
- **mixed** → move HEAD, keep changes unstaged
- **hard** → move HEAD and discard working-tree changes

### `git revert`

Starting with:

```
A -- B -- C -- D   main
```

Run:

```bash
git revert D
```

Git creates a new commit:

```
A -- B -- C -- D -- D'   main
```

`D'` contains the inverse of `D`.

So the history says **"D happened, and then we undid D."**

# Interview Tips

> `reset` rewrites history

> `revert` preserves history

If the commit has **already been pushed to a shared branch**, prefer:

```bash
git revert <commit>
```

If you're fixing your **own local commits before pushing**, `reset` is often appropriate.
