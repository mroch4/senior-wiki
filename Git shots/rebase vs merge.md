# `merge` vs. `rebase`

Both **merge** and **rebase** integrate changes from one branch into another, but they do it differently.

Suppose you have:

```
main:    A---B---C
              \
feature:       D---E
```

### `git merge`

Merge combines the two histories and typically creates a **merge commit**:

```
A---B---C-------M
     \         /
      D---E---
```

Command:

```bash
git checkout main
git merge feature
```

#### ✅ Advantages

- **Doesn't rewrite existing history**
- Safe for branches that are shared with other developers
- Preserves the actual branching history

#### ❌ Disadvantages

- Can create many merge commits
- History can become harder to read

### `rebase`

Rebase takes your feature commits and **replays them on top of the latest main**:

Before:

```
A---B---C       main
     \
      D---E     feature
```

After:

```
A---B---C---D'---E'    feature
```

Command:

```bash
git checkout feature
git rebase main
```

The commits `D` and `E` are effectively recreated as `D'` and `E'`.

#### ✅ Advantages

- Produces a **linear, clean history**
- Makes `git log` easier to understand
- Avoids unnecessary merge commits

#### ❌ Disadvantages

- **Rewrites commit history**
- Can cause problems if the branch has already been pushed and others are working from it
- Conflicts may need to be resolved during the rebase

### When would you use each?

| Situation                                      | Prefer                           |
| ---------------------------------------------- | -------------------------------- |
| Feature branch is private to you               | **Rebase**                       |
| Feature branch is shared with other developers | **Merge**                        |
| Want a clean linear history                    | **Rebase**                       |
| Want to preserve exact branching history       | **Merge**                        |
| Integrating a completed PR                     | Either, depending on team policy |
| Already pushed/shared commits                  | Usually **Merge**                |

### Important example

If `main` has moved forward while you're developing:

```
A---B---C---F       main
     \
      D---E         feature
```

You can update your feature branch with:

```bash
git checkout feature
git rebase main
```

Result:

```
A---B---C---F---D'---E'
```

Then, if the feature branch is already on the remote, you may need:

```bash
git push --force-with-lease
```

**Use `--force-with-lease`, not plain `--force`**, because it provides protection against overwriting someone else's newer remote changes.

# Interview Tips

> `merge` preserves history

> `rebase` rewrites history to create a linear history

> I generally rebase my private feature branch onto the latest main to keep the history clean. Once a branch is shared with other developers, I avoid rebasing it because that rewrites history. In that case, I prefer merging.
