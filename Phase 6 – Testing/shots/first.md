# F.I.R.S.T.

**F.I.R.S.T.** is a set of principles for writing **good unit tests**:

| Letter | Principle           | Meaning                                                                                       |
| ------ | ------------------- | --------------------------------------------------------------------------------------------- |
| **F**  | **Fast**            | Tests should execute quickly so developers can run them frequently.                           |
| **I**  | **Independent**     | Tests should not depend on other tests or their execution order.                              |
| **R**  | **Repeatable**      | A test should produce the same result every time, regardless of environment.                  |
| **S**  | **Self-validating** | Tests should automatically determine pass/fail—no manual inspection should be needed.         |
| **T**  | **Timely**          | Tests should be written at the right time, ideally alongside the production code (e.g., TDD). |

## Example

A good unit test:

```csharp
[Fact]
public void Add_ShouldReturnSumOfTwoNumbers()
{
    // Arrange
    var calculator = new Calculator();

    // Act
    var result = calculator.Add(2, 3);

    // Assert
    Assert.Equal(5, result);
}
```

This test is:

- **Fast** → no database/network calls
- **Independent** → doesn't rely on another test
- **Repeatable** → always expects `5`
- **Self-validating** → `Assert.Equal` determines the result
- **Timely** → can be written together with the `Add` functionality

**Interview tip:** If asked _“What makes a good unit test?”_, remembering **FIRST** gives you a concise framework for answering.
