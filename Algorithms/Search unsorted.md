# Search unsorted

```csharp
int SearchThirdLargest(int[] numbers)
{
    int? first = null;
    int? second = null;
    int? third = null;

    foreach (int n in array)
    {
        if (n == first || n == second || n == third)
            continue;

        if (first == null || n > first)
        {
            third = second;
            second = first;
            first = n;
        }
        else if (second == null || n > second)
        {
            third = second;
            second = n;
        }
        else if (third == null || n > third)
        {
            third = n;
        }
    }

    return -1;

    //int thirdLargest = array.Distinct().OrderByDescending(x => x).Skip(2).First();
}
```

```csharp
int SearchThirdSmallest(int[] numbers)
{
    int? first = null;
    int? second = null;
    int? third = null;

    foreach (int n in array)
    {
        if (n == first || n == second || n == third)
            continue;

        if (first == null || n < first)
        {
            third = second;
            second = first;
            first = n;
        }
        else if (second == null || n < second)
        {
            third = second;
            second = n;
        }
        else if (third == null || n < third)
        {
            third = n;
        }
    }

    return -1;

    //int thirdSmallest = array.Distinct().OrderBy(x => x).Skip(2).First();
}
```
