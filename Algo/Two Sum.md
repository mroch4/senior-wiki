# Two Sum

Given an array and a target, find two numbers that add up to the target.

```csharp
int[] TwoSum(int[] numbers, int target)
{
    var dictionary = new Dictionary<int, int>();

    for (int i = 0; i < numbers.Length; i++)
    {
        int needed = target - numbers[i];

        if (dictionary.ContainsKey(needed))
        {
            return new int[] { dictionary[needed], i };
        }

        dictionary[numbers[i]] = i;
    }

    return Array.Empty<int>();
}
```
