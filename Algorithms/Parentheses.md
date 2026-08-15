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
    //var brackets = "(){}[]";
    //var filtered = input.Where(x => brackets.Contains(x));

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
