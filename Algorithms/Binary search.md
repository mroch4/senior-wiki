# Binary search

Instead of checking every number, binary search repeatedly cuts the search area in half. Requires a **sorted array**.

```csharp
int BinarySearch(int[] numbers, int target)
{
    int leftIndex = 0;
    int rightIndex = numbers.Length - 1;

    while (leftIndex <= rightIndex)
    {
        int middleIndex = leftIndex + (rightIndex - leftIndex) / 2;

        if (numbers[middleIndex] == target)
        {
            return middleIndex;
        }

        if (numbers[middleIndex] < target)
        {
            leftIndex = middleIndex + 1;
        }
        else
        {
            rightIndex = middleIndex - 1;
        }
    }

    return -1;
}
```

# Find first occurence

```csharp
public static int findFirstOccurrence(List<int> nums, int target)
{
    int leftIndex = 0;
    int rightIndex = nums.Count - 1;
    int index = -1;

    while (leftIndex <= rightIndex)
    {
        int middleIndex = leftIndex + (rightIndex - leftIndex) / 2;

        if (nums[middleIndex] == target)
        {
            index = mid;
            rightIndex = middleIndex - 1;
        }
        else if (nums[middleIndex] < target)
        {
            leftIndex = middleIndex + 1;
        }
        else
        {
            rightIndex = middleIndex - 1;
        }
    }

    return index;
}
```
