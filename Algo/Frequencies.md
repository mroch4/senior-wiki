# Count character frequencies

```csharp
Dictionary<char, int> CountCharacters(string input)
{
    var dictionary = new Dictionary<char, int>();

    foreach (char letter in input)
    {
        if (dictionary.ContainsKey(letter))
        {
            dictionary[letter]++;
        }
        else
        {
            dictionary[letter] = 1;
        }
    }

    return dictionary;
}
```
