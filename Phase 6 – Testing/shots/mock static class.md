# Mock static class

You generally **cannot mock a static class directly** with traditional mocking frameworks like Moq, because static methods are resolved at compile time rather than through an interface/virtual dispatch.

For example:

```csharp
public static class PaymentHelper
{
    public static bool IsValid(string cardNumber)
    {
        // complex logic
        return true;
    }
}
```

If your production code does:

```csharp
public bool ProcessPayment(string cardNumber)
{
    if (PaymentHelper.IsValid(cardNumber))
        return true;

    return false;
}
```

You can't simply do:

```csharp
Mock<PaymentHelper> mock = new Mock<PaymentHelper>(); // ❌
```

## Better approach: wrap the static class

Create an interface:

```csharp
public interface IPaymentValidator
{
    bool IsValid(string cardNumber);
}
```

Then create an adapter:

```csharp
public class PaymentValidator : IPaymentValidator
{
    public bool IsValid(string cardNumber)
    {
        return PaymentHelper.IsValid(cardNumber);
    }
}
```

Use the interface in your service:

```csharp
public class PaymentService
{
    private readonly IPaymentValidator _validator;

    public PaymentService(IPaymentValidator validator)
    {
        _validator = validator;
    }

    public bool ProcessPayment(string cardNumber)
    {
        return _validator.IsValid(cardNumber);
    }
}
```

Now the test is easy with Moq:

```csharp
var mockValidator = new Mock<IPaymentValidator>();

mockValidator
    .Setup(x => x.IsValid("1234"))
    .Returns(true);

var service = new PaymentService(mockValidator.Object);

var result = service.ProcessPayment("1234");

Assert.True(result);
```

## What if you cannot change the static class?

Some specialized frameworks/tools can mock static methods, but this is usually more cumbersome and can require commercial tooling or specific runtime support.

For **unit-testable design**, the preferred solution is:

**Static class → wrapper/adapter → interface → dependency injection → mock interface**

Also, if the static method contains **pure logic** (same input → same output, no external dependencies), you often don't need to mock it at all. Test that static method separately and test your service with a mocked abstraction around it.
