# Reverse a string

```csharp
string ReverseString(string input)
{
    char[] chars = input.ToCharArray();
    Array.Reverse(chars);
    return new string(chars);
}
```

A common alternative using a loop:

```csharp
string ReverseString(string input)
{
    string result = "";

    for (int i = input.Length - 1; i >= 0; i--)
    {
        result += input[i];
    }

    return result;
}
```
