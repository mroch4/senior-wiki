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

# isAnagram

```csharp
public static int isAnagram(string s, string t)
{
    var sDict = CountCharacters(s);
    var tDict = CountCharacters(t);

    foreach (var kvp in sDict)
    {
        if (!tDict.ContainsKey(kvp.Key))
        {
            return 0;
        };

        if (tDict[kvp.Key] != kvp.Value)
        {
            return 0;
        };
    }

    return 1;
}
```
