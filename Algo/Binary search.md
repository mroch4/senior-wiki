# Binary search

Instead of checking every number, binary search repeatedly cuts the search area in half. Requires a **sorted array**.

```csharp
int BinarySearch(int[] numbers, int target)
{
    int left = 0;
    int right = numbers.Length - 1;

    while (left <= right)
    {
        int middle = left + (right - left) / 2;

        if (numbers[middle] == target)
            return middle;

        if (numbers[middle] < target)
            left = middle + 1;
        else
            right = middle - 1;
    }

    return -1;
}
```
