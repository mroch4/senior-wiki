# Merge two sorted arrays

```csharp
int[] MergeSortedArrays(int[] first, int[] second)
{
    var result = new List<int>();

    int i = 0;
    int j = 0;

    while (i < first.Length && j < second.Length)
    {
        if (first[i] < second[j])
        {
            result.Add(first[i]);
            i++;
        }
        else
        {
            result.Add(second[j]);
            j++;
        }
    }

    while (i < first.Length)
    {
        result.Add(first[i]);
        i++;
    }

    while (j < second.Length)
    {
        result.Add(second[j]);
        j++;
    }

    return result.ToArray();
}
```
