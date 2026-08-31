# `xUnit` vs. `NUnit`

Both **xUnit** and **NUnit** are popular unit testing frameworks for .NET. They provide the same core capabilities (writing tests, assertions, setup/teardown, test discovery), but they differ in philosophy and modern .NET support.

| Feature                    | xUnit                         | NUnit                                            |
| -------------------------- | ----------------------------- | ------------------------------------------------ |
| Age                        | Newer                         | Older, very mature                               |
| Creator                    | Original NUnit developers     | NUnit team                                       |
| Popularity                 | Very popular for ASP.NET Core | Still widely used, especially in legacy projects |
| Default in .NET templates  | ✅ Often used                 | ❌ No                                            |
| Test attributes            | `[Fact]`, `[Theory]`          | `[Test]`, `[TestCase]`                           |
| Setup/Teardown             | Constructor/`IDisposable`     | `[SetUp]`, `[TearDown]`                          |
| Parallel execution         | Enabled by default            | Configurable                                     |
| Dependency Injection style | Fits modern DI patterns       | More traditional                                 |

## Basic Test Example

### xUnit

```csharp
public class CalculatorTests
{
    [Fact]
    public void Add_ReturnsCorrectResult()
    {
        var calculator = new Calculator();

        var result = calculator.Add(2, 3);

        Assert.Equal(5, result);
    }
}
```

### NUnit

```csharp
[TestFixture]
public class CalculatorTests
{
    [Test]
    public void Add_ReturnsCorrectResult()
    {
        var calculator = new Calculator();

        var result = calculator.Add(2, 3);

        Assert.That(result, Is.EqualTo(5));
    }
}
```

## Parameterized Tests

### xUnit

Uses **Theory**.

```csharp
[Theory]
[InlineData(2, 3, 5)]
[InlineData(10, 5, 15)]
public void Add_ReturnsCorrectResult(int a, int b, int expected)
{
    Assert.Equal(expected, calculator.Add(a, b));
}
```

### NUnit

Uses **TestCase**.

```csharp
[TestCase(2, 3, 5)]
[TestCase(10, 5, 15)]
public void Add_ReturnsCorrectResult(int a, int b, int expected)
{
    Assert.That(calculator.Add(a, b), Is.EqualTo(expected));
}
```

## Test Initialization

### xUnit

No `[SetUp]` attribute.

```csharp
public class CalculatorTests
{
    private readonly Calculator _calculator;

    public CalculatorTests()
    {
        _calculator = new Calculator();
    }
}
```

Cleanup:

```csharp
public class CalculatorTests : IDisposable
{
    public void Dispose()
    {
        // Cleanup
    }
}
```

### NUnit

```csharp
[SetUp]
public void Setup()
{
    _calculator = new Calculator();
}

[TearDown]
public void TearDown()
{
}
```

## Sharing Fixtures

### xUnit

Uses fixtures.

```csharp
public class DatabaseFixture
{
}

public class Tests : IClassFixture<DatabaseFixture>
{
}
```

This encourages explicit dependency management and aligns well with ASP.NET Core's dependency injection patterns.

### NUnit

Typically uses:

```csharp
[OneTimeSetUp]
public void OneTimeSetup()
{
}
```

## Assertions

### xUnit

```csharp
Assert.Equal(expected, actual);
Assert.True(result);
Assert.NotNull(obj);
Assert.Throws<Exception>(() => ...);
```

### NUnit

```csharp
Assert.That(actual, Is.EqualTo(expected));
Assert.That(result, Is.True);
Assert.That(obj, Is.Not.Null);
Assert.Throws<Exception>(() => ...);
```

Many developers find NUnit's constraint-based syntax (`Assert.That`) more expressive for complex assertions.

## Parallel Execution

### xUnit

- Runs tests in parallel by default.
- Good for reducing test execution time.
- Tests should avoid shared mutable state.

### NUnit

- Supports parallel execution.
- Requires explicit configuration or attributes.

## Which Should You Choose?

### Choose xUnit if:

- You're starting a new .NET or ASP.NET Core project.
- You want the framework that best aligns with modern .NET practices.
- You prefer constructor-based setup and fixture injection over lifecycle attributes.
- You want a framework commonly used in newer projects.

### Choose NUnit if:

- You're working on an existing project that already uses NUnit.
- You prefer `[SetUp]`/`[TearDown]` and `Assert.That(...)`.
- You have extensive existing NUnit tests or tooling.

## Recommendation

For **new ASP.NET Core, Blazor, or microservices projects**, **xUnit** is generally the preferred choice because it integrates naturally with modern .NET patterns and is the framework most commonly seen in current .NET ecosystems.

For **maintaining or extending existing applications**, it's usually best to stay with whichever framework the project already uses, since the functional differences are relatively small and migrating tests rarely provides enough benefit to justify the effort.
