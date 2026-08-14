# Count Elements Greater Than Previous Average

```csharp
public static int CountResponseTimeRegressions(List<int> responseTimes)
{
    if (responseTimes.Count < 2)
    {
        return 0;
    }

    int count = 0;
    long sum = responseTimes[0];

    for (int i = 1; i < responseTimes.Count; i++)
    {
        var avg = (decimal)sum / i;

        if (responseTimes[i] > avg)
        {
            count++;
        }

        sum += responseTimes[i];
    }

    return count;
}
```
