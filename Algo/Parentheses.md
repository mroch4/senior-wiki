# Validate parentheses using `Stack<char>`

For example:

```text
"()[]{}"       -> true
"([{}])"       -> true
"([)]"         -> false
```

```csharp
bool IsValid(string input)
{
    var stack = new Stack<char>();

    foreach (char c in input)
    {
        if (c == '(' || c == '[' || c == '{')
        {
            stack.Push(c);
        }
        else
        {
            if (stack.Count == 0)
                return false;

            char opening = stack.Pop();

            if (c == ')' && opening != '(')
                return false;

            if (c == ']' && opening != '[')
                return false;

            if (c == '}' && opening != '{')
                return false;
        }
    }

    return stack.Count == 0;
}
```

The key concept is **LIFO**:

> Last In, First Out.

For:

```text
([{}])
```

the stack behaves roughly like:

```text
(
(
[
(
[
{
```

Then the closing brackets must match the most recently opened bracket.
