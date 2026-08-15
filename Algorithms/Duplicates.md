# Find duplicates using `HashSet`

```csharp
List<int> FindDuplicates(int[] numbers)
{
    var seen = new HashSet<int>();
    var duplicates = new List<int>();

    foreach (int number in numbers)
    {
        if (!seen.Add(number))
        {
            duplicates.Add(number);
        }
    }

    return duplicates;
}
```

`HashSet.Add()` returns:

- `true` → item was not already there
- `false` → item already existed
