# Check if a string is a palindrome

**Key idea:** compare the first and last characters, then move towards the middle.

```csharp
bool IsPalindrome(string input)
{
    //var letters = new string(code.Where(char.IsLetter).ToArray()).ToLower();

    int left = 0;
    int right = input.Length - 1;

    while (left < right)
    {
        if (input[left] != input[right])
            return false;

        left++;
        right--;
    }

    return true;
}
```
