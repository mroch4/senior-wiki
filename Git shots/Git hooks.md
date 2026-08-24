# Git hooks

> Git hooks are scripts that Git automatically runs at specific points in the Git workflow.

They’re commonly used to **validate code, enforce rules, or automate tasks** before a commit, push, merge, etc.

### Common Git hooks

| Hook           | When it runs                  | Typical use              |
| -------------- | ----------------------------- | ------------------------ |
| `pre-commit`   | Before commit is created      | Lint, formatting, tests  |
| `commit-msg`   | After message entered         | Validate commit message  |
| `post-commit`  | After commit                  | Notifications            |
| `pre-push`     | Before pushing                | Run tests/checks         |
| `pre-rebase`   | Before rebase                 | Prevent unsafe rebases   |
| `post-merge`   | After merge                   | Update dependencies      |
| `pre-receive`  | Server, before accepting push | Enforce repository rules |
| `update`       | Server, per branch/ref        | Control branch updates   |
| `post-receive` | Server, after push accepted   | Trigger deployment       |

### Example: `pre-commit`

Suppose you want to prevent commits if tests fail.

`.git/hooks/pre-commit`:

```bash
#!/bin/sh

echo "Running tests..."

dotnet test

if [ $? -ne 0 ]; then
    echo "Tests failed. Commit aborted."
    exit 1
fi
```

Make it executable:

```bash
chmod +x .git/hooks/pre-commit
```

Now:

```bash
git commit -m "Add feature"
```

Git runs the hook first:

```
Running tests...
...
Tests failed. Commit aborted.
```

The commit won't be created.

### Important interview point

Hooks in `.git/hooks` are **local to the repository clone** and aren't normally committed to Git.

For team projects, tools such as **Husky**, **pre-commit**, or custom scripts are often used to make hooks easier to distribute and manage.

### Client-side vs server-side

**Client-side hooks:**

- `pre-commit`
- `commit-msg`
- `pre-push`
- `pre-rebase`

Run on the developer's machine.

**Server-side hooks:**

- `pre-receive`
- `update`
- `post-receive`

Run on the Git server and are useful for enforcing rules that developers shouldn't be able to bypass.

**Senior interview takeaway:** Git hooks are a form of **automation/enforcement around Git events**, but client-side hooks should not be your only security mechanism because developers can bypass them with options such as `--no-verify`.
